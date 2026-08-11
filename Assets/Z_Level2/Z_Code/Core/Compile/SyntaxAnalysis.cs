using System;
using System.Collections.Generic;
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
                if (node != null)
                {
                    node.parentNode = this;
                }
            }
        }

        public SyntaxNode parentNode;
        public List<SyntaxNode> subNodes = new List<SyntaxNode>();

        public bool Contains(SyntaxNode target)
        {
            if (this == target)
            {
                return true;
            }
            foreach (var node in subNodes)
            {
                if (node != null && node.Contains(target))
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Recursive-descent parser for Z Code. Binary operator nodes intentionally retain the historical
    /// subNodes layout [right, left], while function arguments remain in source order.
    /// </summary>
    public class SyntaxAnalysis
    {
        public static bool DEBUG = false;

        public const int MaxNestingDepth = 256;
        public const string EmptyNodeCode = "__empty";

        private sealed class ParseException : Exception
        {
        }

        private List<LexicalNode> _nodes;
        private List<CompileError> _errors;
        private int _current;
        private int _nestingDepth;

        public List<SyntaxNode> Execute(List<LexicalNode> nodes, List<CompileError> errors = null)
        {
            _nodes = nodes ?? new List<LexicalNode>();
            _errors = errors ?? new List<CompileError>();
            _current = 0;
            _nestingDepth = 0;

            var statements = new List<SyntaxNode>();
            while (!IsAtEnd())
            {
                if (Match(";"))
                {
                    continue;
                }
                if (Check("}"))
                {
                    Report(Peek(), "存在未匹配的 '}'");
                    Advance();
                    continue;
                }

                int start = _current;
                var statement = ParseStatementSafe();
                if (statement != null)
                {
                    statements.Add(statement);
                }
                if (_current == start && !IsAtEnd())
                {
                    Advance();
                }
            }

            if (DEBUG || Compiler.DEBUG)
            {
                var root = new SyntaxNode(new Desc("program", CodeType.Action), statements);
                Z_Log.LogTree(root, "subNodes", new[] { "desc.code" });
            }
            return statements;
        }

        private SyntaxNode ParseStatementSafe()
        {
            bool entered = false;
            try
            {
                EnterNesting(Peek());
                entered = true;
                return ParseStatement();
            }
            catch (ParseException)
            {
                Synchronize();
                return null;
            }
            catch (Exception ex)
            {
                var token = IsAtEnd() ? PreviousOrNull() : Peek();
                Report(token, $"语句解析失败: {ex.Message}", ex);
                Synchronize();
                return null;
            }
            finally
            {
                if (entered)
                {
                    ExitNesting();
                }
            }
        }

        private SyntaxNode ParseStatement()
        {
            if (Match("if"))
            {
                return ParseIf(Previous());
            }
            if (Match("for"))
            {
                return ParseFor(Previous());
            }
            if (Match("while"))
            {
                return ParseWhile(Previous());
            }
            if (Match("Wait"))
            {
                return ParseWait(Previous());
            }
            if (Match("Return"))
            {
                return ParseReturn(Previous());
            }
            if (Match("break"))
            {
                var keyword = Previous();
                Consume(";", "break 后需要 ';'");
                return ReservedNode(keyword);
            }
            if (Match("continue"))
            {
                var keyword = Previous();
                Consume(";", "continue 后需要 ';'");
                return ReservedNode(keyword);
            }
            if (Match("else"))
            {
                throw Error(Previous(), "else 前缺少对应的 if");
            }

            var expression = ParseExpression();
            Consume(";", "表达式语句末尾需要 ';'");
            return expression;
        }

        private SyntaxNode ParseIf(LexicalNode keyword)
        {
            Consume("(", "if 后需要 '('");
            var condition = ParseExpression();
            Consume(")", "if 条件后需要 ')'");
            var thenBlock = ParseBlock("if");

            var children = new List<SyntaxNode> { condition, thenBlock };
            if (Match("else"))
            {
                if (Match("if"))
                {
                    var nestedKeyword = Previous();
                    var nestedIf = ParseNested(nestedKeyword, () => ParseIf(nestedKeyword));
                    children.Add(ActionNode("else", nestedIf.desc.codeIndex, new List<SyntaxNode> { nestedIf }));
                }
                else
                {
                    children.Add(ParseBlock("else"));
                }
            }
            return new SyntaxNode(keyword.desc, children);
        }

        private SyntaxNode ParseWhile(LexicalNode keyword)
        {
            Consume("(", "while 后需要 '('");
            var condition = ParseExpression();
            Consume(")", "while 条件后需要 ')'");
            var body = ParseBlock("while");
            return new SyntaxNode(keyword.desc, new List<SyntaxNode> { condition, body });
        }

        private SyntaxNode ParseFor(LexicalNode keyword)
        {
            Consume("(", "for 后需要 '('");

            SyntaxNode init = Check(";") ? EmptyNode(CurrentCodeIndex()) : ParseExpression();
            Consume(";", "for 初始化段后需要 ';'");

            SyntaxNode condition = Check(";") ? EmptyNode(CurrentCodeIndex()) : ParseExpression();
            Consume(";", "for 条件段后需要 ';'");

            SyntaxNode iteration = Check(")") ? EmptyNode(CurrentCodeIndex()) : ParseExpression();
            Consume(")", "for 迭代段后需要 ')'");

            var body = ParseBlock("for");
            return new SyntaxNode(keyword.desc, new List<SyntaxNode> { init, condition, iteration, body });
        }

        private SyntaxNode ParseWait(LexicalNode keyword)
        {
            SyntaxNode duration;
            if (Match("("))
            {
                duration = ParseExpression();
                Consume(")", "Wait 参数后需要 ')'");
            }
            else
            {
                duration = ParseExpression();
            }
            Consume(";", "Wait 语句末尾需要 ';'");
            return new SyntaxNode(keyword.desc, new List<SyntaxNode> { duration });
        }

        private SyntaxNode ParseReturn(LexicalNode keyword)
        {
            var children = new List<SyntaxNode>();
            if (!Check(";"))
            {
                children.Add(ParseExpression());
            }
            Consume(";", "Return 语句末尾需要 ';'");
            return new SyntaxNode(keyword.desc, children);
        }

        private SyntaxNode ParseBlock(string owner)
        {
            var open = Consume("{", $"{owner} 后需要 '{{'");
            var statements = new List<SyntaxNode>();
            while (!Check("}") && !IsAtEnd())
            {
                if (Match(";"))
                {
                    continue;
                }
                int start = _current;
                var statement = ParseStatementSafe();
                if (statement != null)
                {
                    statements.Add(statement);
                }
                if (_current == start && !IsAtEnd() && !Check("}"))
                {
                    Advance();
                }
            }
            Consume("}", $"{owner} 代码块缺少 '}}'");
            return ActionNode(owner, open.startIndex, statements);
        }

        private SyntaxNode ParseExpression()
        {
            return ParseNested(Peek(), ParseAssignment);
        }

        private SyntaxNode ParseAssignment()
        {
            var left = ParseLogicalOr();
            if (Match("="))
            {
                var op = Previous();
                var right = ParseNested(Peek(), ParseAssignment);
                if (!IsAssignable(left))
                {
                    Report(op, "赋值左侧必须是变量或一层容器访问");
                }
                return BinaryNode(op, left, right);
            }
            return left;
        }

        private SyntaxNode ParseLogicalOr()
        {
            var expression = ParseLogicalAnd();
            while (Match("||"))
            {
                var op = Previous();
                expression = BinaryNode(op, expression, ParseLogicalAnd());
            }
            return expression;
        }

        private SyntaxNode ParseLogicalAnd()
        {
            var expression = ParseEquality();
            while (Match("&&"))
            {
                var op = Previous();
                expression = BinaryNode(op, expression, ParseEquality());
            }
            return expression;
        }

        private SyntaxNode ParseEquality()
        {
            var expression = ParseComparison();
            while (Match("==", "!="))
            {
                var op = Previous();
                expression = BinaryNode(op, expression, ParseComparison());
            }
            return expression;
        }

        private SyntaxNode ParseComparison()
        {
            var expression = ParseTerm();
            while (Match(">", "<", ">=", "<="))
            {
                var op = Previous();
                expression = BinaryNode(op, expression, ParseTerm());
            }
            return expression;
        }

        private SyntaxNode ParseTerm()
        {
            var expression = ParseFactor();
            while (Match("+", "-"))
            {
                var op = Previous();
                expression = BinaryNode(op, expression, ParseFactor());
            }
            return expression;
        }

        private SyntaxNode ParseFactor()
        {
            var expression = ParseUnary();
            while (Match("*", "/", "%"))
            {
                var op = Previous();
                expression = BinaryNode(op, expression, ParseUnary());
            }
            return expression;
        }

        private SyntaxNode ParseUnary()
        {
            if (Match("!", "+", "-"))
            {
                var op = Previous();
                return new SyntaxNode(op.desc,
                    new List<SyntaxNode> { ParseNested(Peek(), ParseUnary) });
            }
            return ParsePostfix();
        }

        private SyntaxNode ParsePostfix()
        {
            var expression = ParsePrimary();
            while (true)
            {
                if (Match("["))
                {
                    var op = Previous();
                    var key = ParseExpression();
                    Consume("]", "下标访问缺少 ']'");
                    expression = BinaryNode(op, expression, key);
                    continue;
                }
                if (Match("."))
                {
                    var op = Previous();
                    var member = ConsumeIdentifier("'.' 后需要成员名");
                    var memberNode = new SyntaxNode(new Desc(member.rawCode, CodeType.VarName, member.startIndex));
                    expression = BinaryNode(op, expression, memberNode);
                    continue;
                }
                break;
            }
            return expression;
        }

        private SyntaxNode ParsePrimary()
        {
            if (MatchType(CodeType.Num, CodeType.Str))
            {
                return new SyntaxNode(Previous().desc);
            }

            if (MatchType(CodeType.VarName, CodeType.FuncName))
            {
                var identifier = Previous();
                if (!Match("("))
                {
                    if (identifier.desc.type == CodeType.FuncName)
                    {
                        throw Error(identifier, $"函数 {identifier.rawCode} 缺少参数括号");
                    }
                    return new SyntaxNode(identifier.desc);
                }

                var args = new List<SyntaxNode>();
                if (!Check(")"))
                {
                    while (true)
                    {
                        args.Add(ParseExpression());
                        if (Match(","))
                        {
                            if (Check(")"))
                            {
                                break;
                            }
                            continue;
                        }
                        if (Check(")"))
                        {
                            break;
                        }
                        if (!CanStartExpression(Peek()))
                        {
                            throw Error(Peek(), "函数参数之间需要 ','");
                        }
                        // Preserve legacy whitespace-separated arguments.
                    }
                }
                Consume(")", $"函数 {identifier.rawCode} 调用缺少 ')'");
                return new SyntaxNode(
                    new Desc(identifier.rawCode, CodeType.FuncName, identifier.startIndex), args);
            }

            if (Match("("))
            {
                var expression = ParseExpression();
                Consume(")", "分组表达式缺少 ')'");
                return expression;
            }

            if (Match("["))
            {
                // Legacy syntax accepted a standalone [expr] as a grouping expression.
                var expression = ParseExpression();
                Consume("]", "分组表达式缺少 ']'");
                return expression;
            }

            throw Error(Peek(), $"无法从 Token '{Peek()?.rawCode ?? "<EOF>"}' 开始解析表达式");
        }

        private static SyntaxNode BinaryNode(LexicalNode op, SyntaxNode left, SyntaxNode right)
        {
            return new SyntaxNode(op.desc, new List<SyntaxNode> { right, left });
        }

        private static SyntaxNode ReservedNode(LexicalNode keyword)
        {
            return new SyntaxNode(keyword.desc);
        }

        private static SyntaxNode ActionNode(string name, int codeIndex, List<SyntaxNode> children)
        {
            return new SyntaxNode(new Desc(name, CodeType.Action, codeIndex), children);
        }

        private static SyntaxNode EmptyNode(int codeIndex)
        {
            return ActionNode(EmptyNodeCode, codeIndex, new List<SyntaxNode>());
        }

        public static bool IsEmptyNode(SyntaxNode node)
        {
            return node != null && node.desc.type == CodeType.Action && node.desc.code == EmptyNodeCode;
        }

        private static bool IsAssignable(SyntaxNode node)
        {
            if (node == null)
            {
                return false;
            }
            return node.desc.type == CodeType.VarName ||
                   (node.desc.type == CodeType.Operator && (node.desc.code == "." || node.desc.code == "["));
        }

        private bool CanStartExpression(LexicalNode node)
        {
            if (node?.desc == null)
            {
                return false;
            }
            if (node.desc.type == CodeType.Num || node.desc.type == CodeType.Str ||
                node.desc.type == CodeType.VarName || node.desc.type == CodeType.FuncName)
            {
                return true;
            }
            return node.rawCode == "(" || node.rawCode == "[" ||
                   node.rawCode == "!" || node.rawCode == "+" || node.rawCode == "-";
        }

        private void Synchronize()
        {
            while (!IsAtEnd())
            {
                if (_current > 0 && Previous().rawCode == ";")
                {
                    return;
                }
                if (Check("}"))
                {
                    return;
                }
                if (Check("if") || Check("for") || Check("while") || Check("Wait") ||
                    Check("Return") || Check("break") || Check("continue"))
                {
                    return;
                }
                Advance();
            }
        }

        private bool Match(params string[] codes)
        {
            foreach (var code in codes)
            {
                if (Check(code))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool MatchType(params CodeType[] types)
        {
            if (IsAtEnd() || Peek().desc == null)
            {
                return false;
            }
            foreach (var type in types)
            {
                if (Peek().desc.type == type)
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private LexicalNode Consume(string code, string message)
        {
            if (Check(code))
            {
                return Advance();
            }
            throw Error(Peek(), message);
        }

        private LexicalNode ConsumeIdentifier(string message)
        {
            if (!IsAtEnd() && Peek().desc != null &&
                (Peek().desc.type == CodeType.VarName || Peek().desc.type == CodeType.FuncName))
            {
                return Advance();
            }
            throw Error(Peek(), message);
        }

        private bool Check(string code)
        {
            return !IsAtEnd() && Peek().rawCode == code;
        }

        private LexicalNode Advance()
        {
            if (!IsAtEnd())
            {
                _current++;
            }
            return Previous();
        }

        private bool IsAtEnd()
        {
            return _current >= _nodes.Count;
        }

        private LexicalNode Peek()
        {
            return IsAtEnd() ? null : _nodes[_current];
        }

        private LexicalNode Previous()
        {
            return _nodes[_current - 1];
        }

        private LexicalNode PreviousOrNull()
        {
            return _current > 0 ? _nodes[_current - 1] : null;
        }

        private int CurrentCodeIndex()
        {
            var token = Peek() ?? PreviousOrNull();
            return token?.startIndex ?? -1;
        }

        private T ParseNested<T>(LexicalNode token, Func<T> parser)
        {
            EnterNesting(token);
            try
            {
                return parser();
            }
            finally
            {
                ExitNesting();
            }
        }

        private void EnterNesting(LexicalNode token)
        {
            if (_nestingDepth >= MaxNestingDepth)
            {
                throw Error(token, $"语法嵌套超过最大限制 {MaxNestingDepth}");
            }
            _nestingDepth++;
        }

        private void ExitNesting()
        {
            if (_nestingDepth > 0)
            {
                _nestingDepth--;
            }
        }

        private ParseException Error(LexicalNode token, string message)
        {
            Report(token, message);
            return new ParseException();
        }

        private void Report(LexicalNode token, string message, Exception exception = null)
        {
            int start = token?.startIndex ?? (_nodes.Count == 0 ? 0 : _nodes[_nodes.Count - 1].startIndex);
            int end = start + Math.Max(0, (token?.rawCode?.Length ?? 1) - 1);
            _errors.Add(new CompileError
            {
                StartIndex = start,
                EndIndex = end,
                LineNumber = 1,
                ColumnNumber = 1,
                Stage = "语法分析",
                Message = message,
                Exception = exception
            });
        }
    }
}
