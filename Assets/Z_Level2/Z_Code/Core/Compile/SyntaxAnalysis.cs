using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Debug;

namespace Z_Code
{


    public class SyntaxNode : Node
    {
        public SyntaxNode(Desc desc, List<SyntaxNode> subNodes = null)
        {
            this.desc = desc;
            if (subNodes != null)
            {
                this.subNodes = subNodes;
            }
            foreach (var node in this.subNodes)
            {
                node.parentNode = parentNode;
            }
        }
        public Desc desc;
        public SyntaxNode parentNode;
        public List<SyntaxNode> subNodes = new List<SyntaxNode>();
    }

    public class SyntaxAnalysis
    {
        public const bool DEBUG = true;
        public List<SyntaxNode> Execute(List<LexicalNode> nodes)
        {
            var res = BuildBlock(nodes, 0, nodes.Count - 1);
            if (DEBUG)
            {
                var test = new SyntaxNode(new Desc("test", CodeType.Action), res);
                Z_Log.LogTree(test, "subNodes", new[] { "desc.code" });
            }

            return res;
        }
        private List<SyntaxNode> BuildBlock(List<LexicalNode> nodes, int l, int r)
        {
            List<SyntaxNode> statements = new List<SyntaxNode>();

            for (int i = l; i <= r; i++)
            {
                if (nodes[i].desc.type == CodeType.Reserved)
                {
                    int oriPos = i;
                    List<SyntaxNode> subStatements = new List<SyntaxNode>();
                    switch (nodes[i].desc.code)
                    {
                        case "if":
                            int endConditionIf = GetFirstDepth0(nodes, i + 1, r, ")");
                            subStatements.AddRange(BuildStatement(nodes, i + 2, endConditionIf - 1));

                            int endThenIf = GetFirstDepth0(nodes, endConditionIf + 1, r, "}");
                            subStatements.Add(new SyntaxNode(new Desc("then", CodeType.Action), BuildBlock(nodes, endConditionIf + 2, endThenIf - 1)));
                            i = endThenIf;

                            if (endThenIf < r && (nodes[i].desc.type == CodeType.Reserved && nodes[i].desc.code == "else"))
                            {
                                int endElseIf = GetFirstDepth0(nodes, endThenIf + 2, r, "}");
                                subStatements.Add(new SyntaxNode(new Desc("else", CodeType.Action), BuildBlock(nodes, endThenIf + 3, endElseIf - 1)));
                                i = endElseIf;
                            }
                            break;
                        case "for":
                            int endForIf = GetFirstDepth0(nodes, i + 1, r, ")");
                            subStatements.AddRange(BuildStatement(nodes, i + 2, endForIf - 1));

                            int endForDo = GetFirstDepth0(nodes, endForIf + 1, r, "}");
                            subStatements.Add(new SyntaxNode(new Desc("do", CodeType.Action), BuildBlock(nodes, i, endForDo)));

                            i = endForDo;
                            break;
                    }
                    statements.Add(new SyntaxNode(nodes[oriPos].desc, subStatements));

                }
                else
                {
                    int split = GetFirstDepth0(nodes, i, r, ";");
                    if(split>0)
                    {
                        statements.AddRange(BuildStatement(nodes, i, split - 1));
                        i = split;
                    }
                }
            }
            return statements;
        }

        private List<SyntaxNode> BuildStatement(List<LexicalNode> nodes, int l, int r)
        {
            
            //ignore extern small bracket
            while (GetFirstDepth0(nodes, l, r, ")") == r&& GetFirstDepth0(nodes, l, r, "(")==l && l <= r)
            {
                l++;
                r--;
            }

            List<SyntaxNode> res = new List<SyntaxNode>();
            if (l <= r)
            {
                List<Node> cache = new List<Node>();
                //level1
                for (int i = l; i <= r; i++)
                {

                    if (nodes[i].desc.type == CodeType.FuncName)
                    {
                        int endBkt = GetFirstDepth0(nodes, i + 1, r, ")");
                        if (endBkt > 0)
                        {
                            cache.Add(new SyntaxNode(nodes[i].desc, BuildStatement(nodes, i + 1, endBkt)));
                            i = endBkt;
                        }
                    }
                    else if (nodes[i].desc.code == "(")
                    {
                        int endBkt = GetFirstDepth0(nodes, i, r, ")");
                        if (endBkt > 0)
                        {
                            cache.Add(BuildStatement(nodes, i, endBkt)[0]);
                            i = endBkt;
                        }
                    }
                    else
                    {
                        cache.Add(nodes[i]);
                    }
                }
                //level2
                for (int i = 0; i < cache.Count; i++)
                {
                    if (cache[i] is LexicalNode lex && lex.desc.type == CodeType.Operator)
                    {
                        switch (lex.desc.code)
                        {
                            case ".":
                                SetSub(cache, lex, ref i, 1, 1);
                                break;
                        }
                    }
                }

                //level3
                for (int i = 0; i < cache.Count; i++)
                {
                    if (cache[i] is LexicalNode lex && lex.desc.type == CodeType.Operator)
                    {
                        switch (lex.desc.code)
                        {
                            case "*":
                            case "/":
                                SetSub(cache, lex, ref i, 1, 1);
                                break;
                        }
                    }
                }

                //level4
                for (int i = 0; i < cache.Count; i++)
                {
                    if (cache[i] is LexicalNode lex && lex.desc.type == CodeType.Operator)
                    {
                        switch (lex.desc.code)
                        {
                            case "+":
                            case "-":
                                SetSub(cache, lex, ref i, 1, 1);
                                break;
                        }
                    }
                }
                //level5
                for (int i = 0; i < cache.Count; i++)
                {
                    if (cache[i] is LexicalNode lex && lex.desc.type == CodeType.Operator)
                    {
                        switch (lex.desc.code)
                        {
                            case "=":
                                SetSub(cache, lex, ref i, 1, 1);
                                break;
                        }
                    }
                }
                //level6
                for (int i = 0; i < cache.Count; i++)
                {
                    if (cache[i] is LexicalNode lex)
                    {
                        switch(lex.desc.type)
                        {
                            case CodeType.Num:
                            case CodeType.Str:
                            case CodeType.VarName:
                                cache[i] = new SyntaxNode(lex.desc);
                                break;
                        }
                    }
                }
                for (int i = 0; i < cache.Count; i++)
                {
                    if (cache[i] is SyntaxNode node)
                    {
                        res.Add(node);
                    }
                }
            }
            return res;
        }
        private void SetSub(List<Node> cache, LexicalNode lex, ref int root, int preCnt, int afterCnt)
        {
            var subNodes = new List<SyntaxNode>();
            for (int i = root + afterCnt; i >= root + 1; i--)
            {
                subNodes.Add(TryGetSyntaxNode(cache[i]));
            }
            for (int i = root - 1; i >= root - preCnt; i--)
            {
                subNodes.Add(TryGetSyntaxNode(cache[i]));
            }

            cache[root] = new SyntaxNode(lex.desc, subNodes);

            cache.RemoveRange(root - preCnt, preCnt);
            root -= preCnt;
            cache.RemoveRange(root + afterCnt, afterCnt);

        }


        private SyntaxNode TryGetSyntaxNode(Node node)
        {
            if (node is SyntaxNode syn)
                return syn;
            else if (node is LexicalNode lex)
                return new SyntaxNode(lex.desc);
            return null;

        }
        private bool IsFunc(Desc desc)
        {
            return desc.type == CodeType.FuncName;
        }



        private int GetFirstDepth0(List<LexicalNode> nodes, int start, int end, string content)
        {
            int depth = 0;
            for (int i = start; i <= end; i++)
            {
                
                if (nodes[i].desc.code == "}" || nodes[i].desc.code == ")")
                {
                    depth--;
                }
                if (depth == 0 && nodes[i].desc.code == content)
                {
                    return i;
                }
                if (nodes[i].desc.code == "{"|| nodes[i].desc.code =="(")
                {
                    depth++;
                }
            }
            return 0;
        }
    }
}
