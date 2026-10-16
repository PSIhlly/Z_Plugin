using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Z_Code;
using Z_Code.Form;
namespace Z_Code.Form
{
    public static partial class BoxDataForm
    {
        public partial class Data
        {
            // Only the interpreter's private, unregistered TemporaryBox uses
            // these paths. Ordinary/registered Boxes retain generated setters.
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void InitializeTemporaryValue(float number, string text, string reference)
            {
                _num = number;
                _str = text;
                _valName = reference;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void ClearTemporaryValue()
            {
                _uid = -1;
                _num = 0f;
                _str = null;
                _valName = null;
                // Copy-out can attach a shared dictionary. Only drop our pointer.
                _dic = null;
            }

            public Data DeepCopy(bool sameId = true)
            {
                Dictionary<string, BoxDataForm.Data> tmpDic = null;
                if (dic != null)
                {
                    tmpDic = new Dictionary<string, BoxDataForm.Data>(dic.Count);
                    foreach (var pair in dic)
                        tmpDic[pair.Key] = pair.Value.DeepCopy();
                }
                return new Data(sameId ? uid : uidChain.GetId(), str, valName, num, tmpDic);
            }
            public override string ToString()
            {
                if (dic != null && dic.Count > 0)
                {
                    var str = "(";
                    foreach (var kv in dic)
                    {
                        str += kv.Key + ":" + kv.Value.ToString()+" ";
                    }
                    str += ")";
                    return str;
                }
                else if (str == null)
                {
                    return num.ToString();
                }
                else
                {
                    return str;
                }
            }
        }
    }
    public static partial class ProgramDataForm
    {
        public partial class Data
        {
            // Runtime-only: generated persistence continues to serialize the
            // authored program, never this shared execution snapshot.
            private RuntimeCodeSnapshot runtimeCodeSnapshot;

            private sealed class RuntimeCodeSnapshot
            {
                internal readonly List<string> sourceCode, code;
                internal readonly List<int> sourceMap, map;

                internal RuntimeCodeSnapshot(List<string> sourceCode, List<int> sourceMap)
                {
                    this.sourceCode = sourceCode;
                    this.sourceMap = sourceMap;
                    code = sourceCode == null ? null : new List<string>(sourceCode);
                    map = sourceMap == null ? null : new List<int>(sourceMap);
                }

                internal bool Matches(List<string> currentCode, List<int> currentMap) =>
                    ReferenceEquals(sourceCode, currentCode) && ReferenceEquals(sourceMap, currentMap);
            }

            /// <summary>
            /// Capture metadata and the current compiled version for one run.
            /// Unchanged runs share private code/map lists, never heap/stack.
            /// Compiled code/map lists are immutable after publication. Updating
            /// a program replaces the lists; reference identity selects a revision.
            /// Callers must also treat snapshot code/map lists as read-only.
            /// </summary>
            public Data CreateRuntimeSnapshot()
            {
                if (runtimeCodeSnapshot == null || !runtimeCodeSnapshot.Matches(zCode, zCodeMap))
                    runtimeCodeSnapshot = new RuntimeCodeSnapshot(zCode, zCodeMap);
                var snapshot = (Data)MemberwiseClone();
                snapshot.runtimeCodeSnapshot = null;
                snapshot.zCode = runtimeCodeSnapshot.code;
                snapshot.zCodeMap = runtimeCodeSnapshot.map;
                return snapshot;
            }

            public bool TryApplyCode(string code, out List<SyntaxNode> syntaxNodes,
                out List<CompileError> errors, Compiler compiler = null)
            {
                if (compiler == null)
                    compiler = new Compiler();

                if (!compiler.TryCompile(code, out var compiledCode, out syntaxNodes,
                        out int count, out string ret, out var compiledCodeMap, out errors))
                {
                    return false;
                }

                this.code = code;
                zCode = compiledCode;
                zCodeMap = compiledCodeMap;
                paramCount = count;
                returnValue = ret;
                return true;
            }

            public List<SyntaxNode> ApplyCode(string code, Compiler compiler = null)
            {
                TryApplyCode(code, out var syntaxNodes, out _, compiler);
                return syntaxNodes;
            }
        }
    }
}
namespace Z_Code
{

    public static class CodeHelper
    {
        public static BoxDataForm.Data CreateBox()
        {
            return new BoxDataForm.Data(-1, null, null, 0, new Dictionary<string, BoxDataForm.Data>());
        }
        public static BoxDataForm.Data CreateBoxByVal(string valName)
        {
            return new BoxDataForm.Data(-1, null, valName, 0, null);
        }

        public static BoxDataForm.Data CreateBoxByStr(string str)
        {
            return new BoxDataForm.Data(-1, str, null, 0, null);
        }
        public static BoxDataForm.Data CreateBoxByNum(float num)
        {
            return new BoxDataForm.Data(-1, null, null, num, null);
        }

        // Scalar/reference Boxes need no dictionary until a member is accessed.
        // CreateBox remains the container factory for existing dictionary writers.
        public static Dictionary<string, BoxDataForm.Data> GetOrCreateDictionary(this BoxDataForm.Data box)
        {
            return box.dic ??= new Dictionary<string, BoxDataForm.Data>();
        }
        public static string GetBoxContent(this BoxDataForm.Data data)
        {
            if (data.str == null)
            {
                return data.num.ToString();
            }
            return data.str;
        }

    }

}
