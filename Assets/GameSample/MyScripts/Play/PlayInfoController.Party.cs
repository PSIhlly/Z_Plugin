using System;
using System.Collections.Generic;
using System.Linq;
using Form;
using Z_DesignStyle;

public partial class PlayInfoController
{
    private enum PartyAction { Add, Remove, Deploy, Undeploy, SetCurrent }

    public void AddCharacterToTeam(string reference) => ChangeParty(reference, PartyAction.Add);
    public void RemoveCharacterFromTeam(string reference) => ChangeParty(reference, PartyAction.Remove);
    public void DeployCharacter(string reference) => ChangeParty(reference, PartyAction.Deploy);
    public void UndeployCharacter(string reference) => ChangeParty(reference, PartyAction.Undeploy);
    public void SetCurrentCharacter(string reference) => ChangeParty(reference, PartyAction.SetCurrent);

    public static bool CanBeCurrentCharacter(CharacterProductForm.Data character)
    {
        if (character == null)
            return false;
        // Stories without a configured HP parameter retain their existing behavior.
        if (string.IsNullOrEmpty(character.hpParamName) || character.paramDic == null
            || !character.paramDic.TryGetValue(character.hpParamName, out var parameter) || parameter == null)
            return true;
        var hp = parameter.GetValue();
        return hp == null || hp.str != null || hp.num > 0;
    }

    private static bool IsAvailableActiveCharacter(ProgressForm.Data progress, int uid)
    {
        return uid > 0 && progress.teamActive != null && progress.teamActive.Contains(uid)
            && progress.team != null && progress.team.Contains(uid)
            && CharacterProductForm.DataByUid.TryGetValue(uid, out var character)
            && CanBeCurrentCharacter(character);
    }

    private static int GetFirstAvailableActiveCharacter(ProgressForm.Data progress)
    {
        if (progress.teamActive != null)
            foreach (int uid in progress.teamActive)
                if (IsAvailableActiveCharacter(progress, uid))
                    return uid;
        return 0;
    }

    // Also used before scene player creation; never change team/deployment membership.
    public static bool RepairCurrentCharacterHealth(ProgressForm.Data progress)
    {
        if (progress == null || progress.characterUid == 0
            || IsAvailableActiveCharacter(progress, progress.characterUid))
            return false;
        int replacement = GetFirstAvailableActiveCharacter(progress);
        if (replacement == progress.characterUid)
            return false;
        progress.characterUid = replacement;
        return true;
    }

    public void EnsureCurrentCharacterAlive()
    {
        var progress = GameManager.instance.curProgress;
        if (RepairCurrentCharacterHealth(progress))
            PublishCurrentCharacterChange(progress);
    }

    private void PublishCurrentCharacterChange(ProgressForm.Data progress)
    {
        _super.sceneCtrl?.RefreshCurrentCharacter();
        Z_EventHelper.Invoke(new StoryCharacterEvent
        {
            type = StoryCharacterEventType.ChangeCharacter,
            data = CharacterProductForm.DataByUid.TryGetValue(progress.characterUid, out var current) ? current : null
        });
    }

    private static List<CharacterProductForm.Data> GetPartyTargets(string reference, bool create, bool requireAlive = false)
    {
        var targets = CharacterProductForm.ResolveReferences(reference);
        // Team members are live Products (or authored singletons), even when called before Play starts.
        targets.RemoveAll(data => data.protoUid == 0 && !data.unique);
        if (requireAlive && targets.Count > 0)
        {
            // Do not manufacture a fresh copy when all existing copies are defeated.
            targets.RemoveAll(data => !CanBeCurrentCharacter(data));
            return targets;
        }
        if (targets.Count == 0 && create && CharacterProductForm.TryGetReferenceUid(reference, out int uid)
            && CharacterProductForm.DataByUid.TryGetValue(uid, out var proto) && proto.protoUid == 0
            && (!requireAlive || CanBeCurrentCharacter(proto)))
        {
            var product = proto.Copy(false);
            product.ToProduct(proto.uid);
            targets.Add(product);
        }
        return targets;
    }

    private void ChangeParty(string reference, PartyAction action)
    {
        var progress = GameManager.instance.curProgress;
        if (progress == null)
            return;
        var targets = GetPartyTargets(reference,
            action == PartyAction.Add || action == PartyAction.Deploy || action == PartyAction.SetCurrent,
            action == PartyAction.SetCurrent);
        if (targets.Count == 0)
            return;

        var selected = targets[0];
        if (action == PartyAction.SetCurrent)
        {
            selected = targets.Find(data => data.uid == progress.characterUid);
            if (selected == null)
            {
                var scalar = CharacterProductForm.ResolveReference(reference);
                selected = targets.Find(data => ReferenceEquals(data, scalar)) ?? targets[0];
            }
        }

        ApplyPartyChange(progress, selected, () =>
        {
            foreach (var target in action == PartyAction.SetCurrent
                ? new[] { selected } : (IEnumerable<CharacterProductForm.Data>)targets)
            {
                int uid = target.uid;
                switch (action)
                {
                    case PartyAction.Add:
                        AddPartyMember(progress.team, uid);
                        break;
                    case PartyAction.Remove:
                        progress.teamActive.Remove(uid);
                        progress.team.Remove(uid);
                        break;
                    case PartyAction.Deploy:
                    case PartyAction.SetCurrent:
                        AddPartyMember(progress.team, uid);
                        AddPartyMember(progress.teamActive, uid);
                        break;
                    case PartyAction.Undeploy:
                        progress.teamActive.Remove(uid);
                        break;
                }
            }
            if (action == PartyAction.SetCurrent)
                progress.characterUid = selected.uid;
        });
    }

    private void ReplacePartySlot(int oldUid, int newUid, int index)
    {
        var progress = GameManager.instance.curProgress;
        if (progress == null || !CharacterProductForm.DataByUid.TryGetValue(newUid, out var target)
            || (target.protoUid == 0 && !target.unique))
            return;
        ApplyPartyChange(progress, target, () =>
        {
            var active = progress.teamActive;
            int slot = oldUid == 0 ? index : active.IndexOf(oldUid);
            if (slot < 0)
                return;
            if (oldUid != 0)
                active.RemoveAt(slot);
            // Move an already deployed member into this slot, rather than duplicating it.
            int previous = active.IndexOf(newUid);
            if (previous >= 0)
            {
                active.RemoveAt(previous);
                if (previous < slot)
                    slot--;
            }
            active.Insert(Math.Max(0, Math.Min(slot, active.Count)), newUid);
            AddPartyMember(progress.team, newUid);
        });
    }

    private void ApplyPartyChange(ProgressForm.Data progress, CharacterProductForm.Data target, Action change)
    {
        var oldTeam = progress.team?.ToArray() ?? Array.Empty<int>();
        var oldActive = progress.teamActive?.ToArray() ?? Array.Empty<int>();
        int oldCurrent = progress.characterUid;
        progress.team ??= new List<int>();
        progress.teamActive ??= new List<int>();
        RemoveInvalidPartyMembers(progress.team);
        RemoveInvalidPartyMembers(progress.teamActive);
        foreach (int uid in progress.teamActive)
            AddPartyMember(progress.team, uid);

        change();
        if (!IsAvailableActiveCharacter(progress, progress.characterUid))
            progress.characterUid = GetFirstAvailableActiveCharacter(progress);

        bool currentChanged = oldCurrent != progress.characterUid;
        bool teamChanged = !oldTeam.SequenceEqual(progress.team) || !oldActive.SequenceEqual(progress.teamActive);
        if (currentChanged)
            _super.sceneCtrl?.RefreshCurrentCharacter();
        if (teamChanged)
            Z_EventHelper.Invoke(new StoryCharacterEvent { type = StoryCharacterEventType.ChangeTeamer, data = target });
        if (currentChanged)
            Z_EventHelper.Invoke(new StoryCharacterEvent
            {
                type = StoryCharacterEventType.ChangeCharacter,
                data = CharacterProductForm.DataByUid.TryGetValue(progress.characterUid, out var current) ? current : null
            });
    }

    private static void AddPartyMember(List<int> members, int uid)
    {
        if (!members.Contains(uid))
            members.Add(uid);
    }

    private static void RemoveInvalidPartyMembers(List<int> members)
    {
        var seen = new HashSet<int>();
        members.RemoveAll(uid => uid <= 0 || !CharacterProductForm.DataByUid.ContainsKey(uid) || !seen.Add(uid));
    }
}
