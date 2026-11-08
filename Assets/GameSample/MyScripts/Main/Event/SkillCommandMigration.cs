using System.Collections.Generic;
using System.Text;
using Z_Code;

/// <summary>In-memory source migration for the retired UseSkill cooldown argument.</summary>
public static class SkillCommandMigration
{
    public static string MigrateCode(string code)
    {
        if (string.IsNullOrEmpty(code) || code.IndexOf("UseSkill", System.StringComparison.Ordinal) < 0)
            return code;

        var errors = new List<CompileError>();
        var tokens = new LexicalAnalysis().Execute(code, errors);
        if (errors.Count != 0)
            return code;
        var statements = new SyntaxAnalysis().Execute(tokens, errors);
        if (errors.Count != 0)
            return code;

        var tokenByStart = new Dictionary<int, int>();
        for (int i = 0; i < tokens.Count; i++)
            tokenByStart[tokens[i].startIndex] = i;
        var removals = new List<(int start, int end)>();
        var pending = new Stack<SyntaxNode>(statements);
        while (pending.Count != 0)
        {
            var node = pending.Pop();
            if (node == null)
                continue;
            foreach (var child in node.subNodes)
                pending.Push(child);
            if (node.desc?.type != CodeType.FuncName || node.desc.code != "UseSkill"
                || node.subNodes.Count != 3
                || !tokenByStart.TryGetValue(node.desc.codeIndex, out int identifier)
                || identifier + 1 >= tokens.Count || tokens[identifier + 1].rawCode != "("
                || (identifier > 0 && tokens[identifier - 1].rawCode == "."))
                continue;

            int thirdStart = FirstSourceIndex(node.subNodes[2]);
            int depth = 1;
            int rootToken = identifier + 2;
            int thirdToken = -1;
            for (int i = identifier + 2; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if (depth == 1)
                    rootToken = i;
                if (token.startIndex == thirdStart)
                    thirdToken = rootToken;
                if (IsOpen(token.rawCode))
                    depth++;
                else if (IsClose(token.rawCode) && --depth == 0)
                {
                    // Keep the enclosing ')' and every byte before the third
                    // argument's separator. Token positions include grouping,
                    // even though SyntaxAnalysis omits grouping-only nodes.
                    if (thirdToken >= 0)
                    {
                        int start = thirdToken;
                        if (start > identifier + 2 && tokens[start - 1].rawCode == ",")
                            start--;
                        removals.Add((tokens[start].startIndex, token.startIndex));
                    }
                    break;
                }
            }
        }
        if (removals.Count == 0)
            return code;

        // A legacy call inside another call's removed third argument has a
        // wholly contained range. Merge before editing original source offsets.
        removals.Sort((left, right) => left.start.CompareTo(right.start));
        var merged = new List<(int start, int end)>();
        foreach (var range in removals)
        {
            if (merged.Count > 0 && range.start <= merged[merged.Count - 1].end)
            {
                var previous = merged[merged.Count - 1];
                merged[merged.Count - 1] = (previous.start, System.Math.Max(previous.end, range.end));
            }
            else
                merged.Add(range);
        }
        var result = new StringBuilder(code);
        for (int i = merged.Count - 1; i >= 0; i--)
            result.Remove(merged[i].start, merged[i].end - merged[i].start);
        return result.ToString();
    }

    private static int FirstSourceIndex(SyntaxNode node)
    {
        int first = int.MaxValue;
        var pending = new Stack<SyntaxNode>();
        pending.Push(node);
        while (pending.Count != 0)
        {
            var current = pending.Pop();
            if (current == null)
                continue;
            if (current.desc != null && current.desc.codeIndex >= 0)
                first = System.Math.Min(first, current.desc.codeIndex);
            foreach (var child in current.subNodes)
                pending.Push(child);
        }
        return first;
    }

    private static bool IsOpen(string token) => token == "(" || token == "[" || token == "{";
    private static bool IsClose(string token) => token == ")" || token == "]" || token == "}";
}
