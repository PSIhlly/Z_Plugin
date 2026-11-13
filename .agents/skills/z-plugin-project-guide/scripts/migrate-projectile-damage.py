"""Migrate one Story ef to projectile-owned Team/damage, preserving other JSON bytes.

Preview: --event-file <Core/ef> --output-file <preview/ef>
Apply:   --event-file <Core/ef> --apply [--backup-directory <directory>]
Only changed programs lose their compiled caches; LoadEvent recompiles them.
"""

import argparse
import datetime
import hashlib
import json
from pathlib import Path
import re


OWNER_SET = re.compile(
    r'(?m)^(?P<indent>[ \t]*)SetSceneObjectParameter\(\s*(?P<object>\w+)\s*,\s*'
    r'"所属人物"\s*,\s*param1\s*\);'
)
OWNER_TEAM = re.compile(
    r'GetCharacterParameter\(\s*GetSceneObjectParameter\(\s*param1\s*,\s*'
    r'"所属人物"\s*\)\s*,\s*"Team"\s*\)'
)
OWNER_DAMAGE = re.compile(
    r'GetCharacterParameter\(\s*GetSceneObjectParameter\(\s*param1\s*,\s*'
    r'"所属人物"\s*\)\s*,\s*"攻击"\s*\)'
)


def migrate_code(row):
    code = row.get("code", "")
    original = code

    def snapshot(match):
        indent, target = match["indent"], match["object"]
        return (
            f'{indent}SetSceneObjectParameter({target}, "所属Team", GetCharacterParameter(param1, "Team"));\n'
            f'{indent}SetSceneObjectParameter({target}, "伤害", GetCharacterParameter(param1, "攻击"));'
        )

    code = OWNER_SET.sub(snapshot, code)
    code = OWNER_TEAM.sub('GetSceneObjectParameter(param1, "所属Team")', code)
    code = OWNER_DAMAGE.sub('GetSceneObjectParameter(param1, "伤害")', code)
    if row.get("name") == "子弹打人" and '"所属人物"' in code:
        # Same-Team contact is a complete no-op. Retain the Story's 0/1 hostility
        # rule (Team 2 is neutral), effects and authored optional stun on enemies.
        stun = '        CharacterStun(param2, 0.1);\n' if 'CharacterStun(param2, 0.1);' in original else ''
        code = (
            'team = GetSceneObjectParameter(param1, "所属Team");\n'
            'targetTeam = GetCharacterParameter(param2, "Team");\n'
            'if (team != targetTeam)\n'
            '{\n'
            '    if ((team == 1 && targetTeam == 0) || (team == 0 && targetTeam == 1))\n'
            '    {\n'
            + stun +
            '        damage = GetSceneObjectParameter(param1, "伤害") - GetCharacterParameter(param2, "防御");\n'
            '        if (damage < 0)\n'
            '        {\n'
            '            damage = 0;\n'
            '        }\n'
            '        SetCharacterParameter(param2, "Hp", GetCharacterParameter(param2, "Hp") - damage);\n'
            '    }\n'
            '    ShowEffect("$ef$1$ef$", GetCharacterPosition(param2), 0);\n'
            '    PlayAudio("$a$55$a$");\n'
            '    DestroyObject(param1);\n'
            '}\n'
        )
    if '"所属人物"' in code:
        raise ValueError(f"Unhandled owner dependency: {row['uid']} {row['name']}")
    return code


def replace_field(record, key, value):
    # Decode the JSON value's exact span instead of serializing the whole table.
    match = re.search(r'"' + re.escape(key) + r'"\s*:\s*', record)
    if not match:
        raise ValueError(f"Missing serialized field: {key}")
    _, end = json.JSONDecoder().raw_decode(record, match.end())
    return record[:match.end()] + json.dumps(value, ensure_ascii=False) + record[end:]


def migrate(text):
    changed = []

    def record(match):
        raw = match[0]
        row = json.loads(raw)
        code = migrate_code(row)
        if code == row.get("code", ""):
            return raw
        changed.append((row["uid"], row["name"]))
        raw = replace_field(raw, "code", code)
        raw = replace_field(raw, "zCode", [])
        return replace_field(raw, "zCodeMap", [])

    result = re.sub(r'(?ms)^  \{\r?\n.*?^  \}', record, text)
    rows = json.loads(result)
    if any('"所属人物"' in row.get("code", "") for row in rows):
        raise ValueError("Some event records were not matched; nothing was written")
    return result, changed


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--event-file", type=Path, required=True)
    output = parser.add_mutually_exclusive_group(required=True)
    output.add_argument("--output-file", type=Path)
    output.add_argument("--apply", action="store_true")
    parser.add_argument("--backup-directory", type=Path)
    parser.add_argument("--expected-sha256", help="Reject a source changed since preview/validation")
    args = parser.parse_args()
    source = args.event_file.resolve(strict=True)
    if source.name != "ef" or not source.is_file():
        parser.error("The exact Story event file (ef) is required")
    before = source.read_bytes()
    if args.expected_sha256 and hashlib.sha256(before).hexdigest() != args.expected_sha256.lower():
        raise RuntimeError("The source changed since preview; regenerate and validate the new preview")
    text = before.decode("utf-8-sig")
    result, changed = migrate(text)
    after = result.encode("utf-8-sig" if before.startswith(b"\xef\xbb\xbf") else "utf-8")
    if args.apply and not changed:
        print(f"Unchanged: {source}")
        return
    target = source if args.apply else args.output_file.resolve()
    if target == source and not args.apply:
        parser.error("Use --apply to replace the source")
    if args.apply:
        backup_root = (args.backup_directory or source.parent.parent / "Backups").resolve()
        backup_root.mkdir(parents=True, exist_ok=True)
        stamp = datetime.datetime.now().strftime("%Y%m%d-%H%M%S-%f")
        backup = backup_root / f"ef.before-projectile-damage-{stamp}.bak"
        with backup.open("xb") as stream:
            stream.write(before)
        if source.read_bytes() != before:
            raise RuntimeError("The source changed during migration; reload and retry")
        print(f"Backup: {backup}")
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_bytes(after)
    print(f"Output: {target}")
    print(f"Original SHA256: {hashlib.sha256(before).hexdigest()}")
    print(f"Changed programs: {len(changed)} " + ", ".join(f"{uid}:{name}" for uid, name in changed))


if __name__ == "__main__":
    main()
