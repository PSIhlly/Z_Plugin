using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Z_Code.Form;

namespace Z_Code.Tests
{
    public class CodeLanguageRegressionTests
    {
        private Dictionary<string, CmdBase> originalCommands;
        private static object allocationSink;

        // RuntimeInitializeOnLoadMethod does not run in EditMode. Install real command templates
        // only for this fixture, restoring the editor's original registry afterwards.
        [OneTimeSetUp]
        public void RegisterCommandsForEditMode()
        {
            originalCommands = BaseData.cmdDic;
            BaseData.cmdDic = new Dictionary<string, CmdBase>(originalCommands, originalCommands.Comparer);
            try
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.IsDynamic)
                        continue;
                    Type[] types;
                    try { types = assembly.GetExportedTypes(); }
                    catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(type => type != null).ToArray(); }
                    foreach (var type in types)
                    {
                        if (type.IsAbstract || !typeof(CmdBase).IsAssignableFrom(type) ||
                            type.GetConstructor(Type.EmptyTypes) == null)
                            continue;
                        // Use actual registrations so legacy aliases are installed too.
                        foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public |
                            BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                            if (method.IsDefined(typeof(RuntimeInitializeOnLoadMethodAttribute), false) &&
                                method.GetParameters().Length == 0)
                                method.Invoke(null, null);
                        var cmd = (CmdBase)Activator.CreateInstance(type);
                        if (!BaseData.cmdDic.ContainsKey(cmd.GetName()))
                            BaseData.cmdDic[cmd.GetName()] = cmd;
                    }
                }
            }
            catch
            {
                RestoreCommandsAfterEditMode();
                throw;
            }
        }

        [OneTimeTearDown]
        public void RestoreCommandsAfterEditMode()
        {
            if (originalCommands != null)
                BaseData.cmdDic = originalCommands;
            originalCommands = null;
        }

        private sealed class RegistryEntry<T> : IDisposable
        {
            private readonly Dictionary<string, T> registry;
            private readonly string name;
            private readonly bool hadPrevious;
            private readonly T previous;

            public RegistryEntry(Dictionary<string, T> registry, string name, T value)
            {
                this.registry = registry;
                this.name = name;
                hadPrevious = registry.TryGetValue(name, out previous);
                registry[name] = value;
            }

            public void Dispose()
            {
                if (hadPrevious) registry[name] = previous;
                else registry.Remove(name);
            }
        }

        [Serializable]
        private sealed class EfEnvelope
        {
            public EfRecord[] records = Array.Empty<EfRecord>();
        }

        [Serializable]
        private sealed class EfRecord
        {
            public int uid = 0;
            public string name = string.Empty;
            public string code = string.Empty;
            public int paramCount = 0;
            public string returnValue = string.Empty;
        }

        private sealed class CancelInterpreterCmd : CmdBase
        {
            public override string GetName() => "Len";
            public override CmdBase GetNew() => new CancelInterpreterCmd();

            protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
            {
                asyncTask.interpreter.data.Cancel();
                return true;
            }
        }

        private sealed class MutatingBoxProbeCmd : CmdBase
        {
            public override string GetName() => "Len";
            public override CmdBase GetNew() => new MutatingBoxProbeCmd();

            protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
            {
                prm[0].num += 1f;
                asyncTask.res = new[] { prm[0] };
                return true;
            }
        }

        private sealed class DelayedProbeState
        {
            public int CreateCount;
            public int ExecuteCount;
            public InterpretAsyncTask Task;
            public BoxDataForm.Data[] Parameters;
        }

        // Use Len's real metadata (one argument and one return value), but do not finish until
        // the test explicitly completes the task. The registered alias remains fixture-local.
        private sealed class DelayedProbeCmd : CmdBase
        {
            private readonly DelayedProbeState state;

            public DelayedProbeCmd(DelayedProbeState state)
            {
                this.state = state;
            }

            public override string GetName() => "Len";

            public override CmdBase GetNew()
            {
                state.CreateCount++;
                return new DelayedProbeCmd(state);
            }

            protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
            {
                state.ExecuteCount++;
                state.Parameters = prm;
                state.Task = asyncTask;
                return false;
            }
        }

        private sealed class DerivedSnapshotProgram : ProgramDataForm.Data
        {
            public int Marker;

            public DerivedSnapshotProgram(ProgramDataForm.Data source, int marker)
                : base(source.uid, source.name, source.code, source.zCode, source.zCodeMap,
                    source.paramCount, source.returnValue)
            {
                Marker = marker;
            }
        }

        [Test]
        public void ExistingEfPrograms_CompileWithoutErrorsOrCarriageReturnOperands()
        {
            var efPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExtraAssets/2/Core/ef"));
            Assert.That(File.Exists(efPath), Is.True, $"Missing legacy ef fixture: {efPath}");

            var json = File.ReadAllText(efPath);
            var envelope = JsonUtility.FromJson<EfEnvelope>($"{{\"records\":{json}}}");
            Assert.That(envelope, Is.Not.Null);
            Assert.That(envelope.records, Is.Not.Null.And.Length.GreaterThan(0));

            var programs = new List<IDisposable>();
            try
            {
                // Match LoadEvent: register all names before compiling any forward/self calls.
                foreach (var record in envelope.records)
                    programs.Add(new RegistryEntry<ProgramDataForm.Data>(ProgramDataForm.DataByName,
                        record.name, new ProgramDataForm.Data(-1, record.name, record.code,
                            new List<string>(), new List<int>(), record.paramCount, record.returnValue)));

                foreach (var record in envelope.records)
                {
                    var zCode = Compile(
                        record.code,
                        out var zCodeMap,
                        out var paramCount,
                        out var returnValue,
                        $"ef uid={record.uid}, name={record.name}");

                    Assert.That(zCode.All(operand => operand == null || !operand.Contains("\r")), Is.True,
                        $"ef uid={record.uid}, name={record.name} compiled a CR into an operand");
                    Assert.That(zCodeMap, Has.Count.EqualTo(zCode.Count));
                    Assert.That(zCodeMap.All(index => index >= 0 && index < record.code.Length), Is.True,
                        $"ef uid={record.uid}, name={record.name} has an invalid source map");
                    Assert.That(paramCount, Is.EqualTo(record.paramCount),
                        $"ef uid={record.uid}, name={record.name} changed its parameter contract");
                    Assert.That(returnValue, Is.EqualTo(record.returnValue),
                        $"ef uid={record.uid}, name={record.name} changed its return contract");
                }
            }
            finally
            {
                for (int i = programs.Count - 1; i >= 0; i--) programs[i].Dispose();
            }
        }

        [Test]
        public void ModuloAndLogicalNot_ExecuteWithNumericBooleanSemantics()
        {
            Assert.That(Execute("Return 7%4;").ret.num, Is.EqualTo(3f).Within(0.0001f));
            Assert.That(Execute("Return !0;").ret.num, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(Execute("Return !2;").ret.num, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void DictionaryTimesScalar_ScalesEveryFieldInEitherOrderWithoutChangingTheSource()
        {
            const string source = @"
v.x=1;
v.y=-2;
v.height=3;
j=-2;
scaled=v*j;
reversed=j*v;
zero=v*0;
Return scaled;";

            var result = Execute(source, out var data);
            Assert.That(result.ret.dic.Keys, Is.EquivalentTo(new[] { "x", "y", "height" }));
            Assert.That(result.ret.dic["x"].num, Is.EqualTo(-2f).Within(0.0001f));
            Assert.That(result.ret.dic["y"].num, Is.EqualTo(4f).Within(0.0001f));
            Assert.That(result.ret.dic["height"].num, Is.EqualTo(-6f).Within(0.0001f));
            Assert.That(data.heap["reversed"].dic["x"].num, Is.EqualTo(-2f).Within(0.0001f));
            Assert.That(data.heap["reversed"].dic["y"].num, Is.EqualTo(4f).Within(0.0001f));
            Assert.That(data.heap["reversed"].dic["height"].num, Is.EqualTo(-6f).Within(0.0001f));
            Assert.That(data.heap["zero"].dic.Keys, Is.EquivalentTo(new[] { "x", "y", "height" }));
            Assert.That(data.heap["zero"].dic.Values.All(value => value.num == 0f), Is.True);
            Assert.That(data.heap["v"].dic["x"].num, Is.EqualTo(1f));
            Assert.That(data.heap["v"].dic["y"].num, Is.EqualTo(-2f));
            Assert.That(data.heap["v"].dic["height"].num, Is.EqualTo(3f));
            Assert.That(Execute("Return 2*3;").ret.num, Is.EqualTo(6f));
        }

        [TestCase("v+2", 10f, 6f, 0f)]
        [TestCase("2+v", 10f, 6f, 0f)]
        [TestCase("v-2", 6f, 2f, -4f)]
        [TestCase("2-v", -6f, -2f, 4f)]
        [TestCase("v*2", 16f, 8f, -4f)]
        [TestCase("2*v", 16f, 8f, -4f)]
        [TestCase("v/2", 4f, 2f, -1f)]
        [TestCase("2/v", 0.25f, 0.5f, -1f)]
        public void DictionaryScalarArithmetic_AppliesToEveryField(string expression, float x, float y, float height)
        {
            var result = Execute($"v.x=8;v.y=4;v.height=-2;Return {expression};", out var data);
            Assert.That(result.ret.dic.Keys, Is.EquivalentTo(new[] { "x", "y", "height" }));
            Assert.That(result.ret.dic["x"].num, Is.EqualTo(x).Within(0.0001f));
            Assert.That(result.ret.dic["y"].num, Is.EqualTo(y).Within(0.0001f));
            Assert.That(result.ret.dic["height"].num, Is.EqualTo(height).Within(0.0001f));
            Assert.That(data.heap["v"].dic["x"].num, Is.EqualTo(8f));
            Assert.That(data.heap["v"].dic["y"].num, Is.EqualTo(4f));
            Assert.That(data.heap["v"].dic["height"].num, Is.EqualTo(-2f));
        }

        [TestCase("a+b", 10f, 3f)]
        [TestCase("a-b", 6f, 5f)]
        [TestCase("a*b", 16f, -4f)]
        [TestCase("a/b", 4f, -4f)]
        public void DictionaryDictionaryArithmetic_UsesMatchingFields(string expression, float x, float y)
        {
            var result = Execute($"a.x=8;a.y=4;a.height=-2;b.x=2;b.y=-1;b.extra=9;Return {expression};");
            Assert.That(result.ret.dic.Keys, Is.EquivalentTo(new[] { "x", "y" }));
            Assert.That(result.ret.dic["x"].num, Is.EqualTo(x).Within(0.0001f));
            Assert.That(result.ret.dic["y"].num, Is.EqualTo(y).Within(0.0001f));
        }

        [TestCase("2+3", 5f)]
        [TestCase("5-3", 2f)]
        [TestCase("2*3", 6f)]
        [TestCase("6/2", 3f)]
        public void ScalarArithmetic_RemainsNumeric(string expression, float expected)
        {
            Assert.That(Execute($"Return {expression};").ret.num, Is.EqualTo(expected).Within(0.0001f));
        }

        [Test]
        public void LogicalAndOr_ShortCircuitRegisteredRightHandCall()
        {
            var commandName = $"Cancel_{Guid.NewGuid():N}";
            using (new RegistryEntry<CmdBase>(BaseData.cmdDic, commandName, new CancelInterpreterCmd()))
            {
                var andResult = Execute($"Return 0&&{commandName}();", out var andData);
                Assert.That(andResult.ret.num, Is.EqualTo(0f).Within(0.0001f));
                Assert.That(andData.IsCancelled, Is.False, "short-circuit executed the right-hand Cmd");

                var orResult = Execute($"Return 1||{commandName}();", out var orData);
                Assert.That(orResult.ret.num, Is.EqualTo(1f).Within(0.0001f));
                Assert.That(orData.IsCancelled, Is.False, "short-circuit executed the right-hand Cmd");
            }
        }

        [Test]
        public void MethodValidation_AcceptsCommandsAliasesAndRegisteredCustomPrograms()
        {
            var alias = $"LenAlias_{Guid.NewGuid():N}";
            var name = $"Custom_{Guid.NewGuid():N}";
            var function = CompileProgram(name, "Return param1;", "valid custom program");
            using (new RegistryEntry<CmdBase>(BaseData.cmdDic, alias, new LenCmd()))
            using (new RegistryEntry<ProgramDataForm.Data>(ProgramDataForm.DataByName, name, function))
            {
                Compile($"Len();{alias}();Return {name}(1);", out _, out _, out _, "known calls");
                Compile($"Return {name}(param1);", out _, out _, out _, "registered self call");
                Assert.That(Execute($"Return {name}(7);").ret.num, Is.EqualTo(7f));
            }
        }

        [Test]
        public void MethodValidation_ReportsAllUnknownCallsInSourceOrderIncludingUnreachableCalls()
        {
            const string source = "if(0){\r\n  MissingOuter(MissingInner());\r\n}\r\nReturn 0&&MissingRight();";
            var compiler = new Compiler();
            Assert.That(compiler.TryCompile(source, out var code, out _, out _, out _,
                out var codeMap, out var errors), Is.False);
            Assert.That(code, Is.Empty);
            Assert.That(codeMap, Is.Empty);
            Assert.That(errors, Has.Count.EqualTo(3));
            var names = new[] { "MissingOuter", "MissingInner", "MissingRight" };
            for (int i = 0; i < names.Length; i++)
            {
                Assert.That(errors[i].Stage, Is.EqualTo("方法检查"));
                Assert.That(errors[i].Message, Does.Contain(names[i]).And.Contain("不存在"));
                Assert.That(errors[i].StartIndex, Is.EqualTo(source.IndexOf(names[i], StringComparison.Ordinal)));
                Assert.That(errors[i].EndIndex, Is.EqualTo(errors[i].StartIndex + names[i].Length - 1));
            }
            Assert.That(errors[0].LineNumber, Is.EqualTo(2));
            Assert.That(errors[0].ColumnNumber, Is.EqualTo(3));
            Assert.That(errors[2].LineNumber, Is.EqualTo(4));
        }

        [Test]
        public void MethodValidation_RejectsWrongCaseButIgnoresStringsAndMemberNames()
        {
            var name = $"Known_{Guid.NewGuid():N}";
            using (new RegistryEntry<CmdBase>(BaseData.cmdDic, name, new LenCmd()))
            {
                var compiler = new Compiler();
                Assert.That(compiler.TryCompile($"{name.ToLowerInvariant()}();", out _, out _, out _,
                    out _, out _, out var errors), Is.False);
                Assert.That(errors.Single().Message, Does.Contain("不存在"));
                Compile("owner.Missing=1;value=\"Missing()\";Return owner.Missing;", out _, out _, out _,
                    "member names and strings are not calls");
            }
        }

        [Test]
        public void MethodValidation_DoesNotOverwriteProgramOnMissingMethod()
        {
            var program = CompileProgram("SafeMethodApply", "Return param1;", "initial valid program");
            var oldCode = program.code;
            var oldZCode = program.zCode;
            var oldMap = program.zCodeMap;
            Assert.That(program.TryApplyCode("MissingApply();", out _, out var errors), Is.False);
            Assert.That(errors.Single().Message, Does.Contain("MissingApply").And.Contain("不存在"));
            Assert.That(program.code, Is.EqualTo(oldCode));
            Assert.That(program.zCode, Is.SameAs(oldZCode));
            Assert.That(program.zCodeMap, Is.SameAs(oldMap));
            Assert.That(program.paramCount, Is.EqualTo(1));
            Assert.That(program.returnValue, Is.EqualTo("var"));
        }

        [Test]
        public void WhileBreakContinue_ExecuteWithNearestLoopSemantics()
        {
            const string source = @"
i=0;
sum=0;
while(i<6)
{
    i=i+1;
    if(i==2){continue;}
    if(i==5){break;}
    sum=sum+i;
}
Return sum;";

            Assert.That(Execute(source).ret.num, Is.EqualTo(8f).Within(0.0001f));
        }

        [Test]
        public void ForClauses_MayBeEmpty()
        {
            var allEmptyZCode = Compile("for(;;){break;}", out _, out _, out _, "all-empty for clauses");
            Assert.That(allEmptyZCode, Is.Not.Empty);

            const string source = @"
i=0;
for(;i<3;)
{
    i=i+1;
}
Return i;";

            Assert.That(Execute(source).ret.num, Is.EqualTo(3f).Within(0.0001f));
        }

        [Test]
        public void PostIncrementAndAddAssign_WorkInStatementsAndForClauses()
        {
            const string source = @"
i=0;
sum=0;
for(;i<4;i++)
{
    sum+=i;
}
i++;
Return sum+i;";

            Assert.That(Execute(source).ret.num, Is.EqualTo(11f).Within(0.0001f));
        }

        [Test]
        public void PostIncrementAndAddAssign_UpdateMemberAndIndexTargets()
        {
            const string source = @"
owner.value=2;
owner.value++;
owner.value+=3;
items[""key""]=4;
items[""key""]++;
items[""key""]+=2;
Return owner.value+items[""key""];";

            Assert.That(Execute(source).ret.num, Is.EqualTo(13f).Within(0.0001f));
        }

        [Test]
        public void ReturnPostIncrement_ReturnsBeforeIncrement()
        {
            var result = Execute("i=7;Return i++;", out var data);

            Assert.That(result.ret.num, Is.EqualTo(7f).Within(0.0001f));
            Assert.That(data.heap["i"].num, Is.EqualTo(7f).Within(0.0001f));
        }

        [Test]
        public void PostIncrementInOtherExpressions_IsRejectedWithoutExecutableCode()
        {
            var compiler = new Compiler();
            var success = compiler.TryCompile("i=1;Return i++ + 1;", out var zCode, out _,
                out _, out _, out _, out var errors);

            Assert.That(success, Is.False);
            Assert.That(zCode, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
        }

        [Test]
        public void IncrementSyntax_RoundTripsThroughDecompiler()
        {
            const string source = "i=0;for(;i<2;i++){i+=1;}Return i++;";
            var compiler = new Compiler();
            var success = compiler.TryCompile(source, out _, out var syntaxNodes,
                out _, out _, out _, out var errors);

            Assert.That(success, Is.True, string.Join("\n", errors));
            var restored = new Decompiler().Decompile(syntaxNodes);
            Assert.That(restored, Does.Contain("i++"));
            Assert.That(restored, Does.Contain("i += 1"));
            Compile(restored, out _, out _, out _, "increment decompiler round-trip");
        }

        [TestCase("a=1+2*3;", "a = 1 + 2 * 3;\n")]
        [TestCase("a=(1+2)*3;", "a = (1 + 2) * 3;\n")]
        [TestCase("a=10-(3-1);", "a = 10 - (3 - 1);\n")]
        [TestCase("a=10/(5/2);", "a = 10 / (5 / 2);\n")]
        [TestCase("a=1+(2+3);", "a = 1 + (2 + 3);\n")]
        [TestCase("a=-(1+2);", "a = -(1 + 2);\n")]
        [TestCase("a=!!b;", "a = !!b;\n")]
        [TestCase("a=+ +b;", "a = + +b;\n")]
        [TestCase("a=1+ +b;", "a = 1 + +b;\n")]
        [TestCase("a=(1||2)&&3;", "a = (1 || 2) && 3;\n")]
        [TestCase("a=foo.bar+items[1+2];", "a = foo.bar + items[1 + 2];\n")]
        public void Decompiler_UsesOnlyRequiredExpressionParentheses(string source, string expected)
        {
            var compiler = new Compiler();
            Assert.That(compiler.TryCompile(source, out var originalCode, out var syntaxNodes,
                out _, out _, out _, out var errors), Is.True, string.Join("\n", errors));

            var restored = new Decompiler().Decompile(syntaxNodes);
            Assert.That(restored, Is.EqualTo(expected));
            Assert.That(compiler.TryCompile(restored, out var restoredCode, out _,
                out _, out _, out _, out errors), Is.True, string.Join("\n", errors));
            Assert.That(restoredCode, Is.EqualTo(originalCode), "decompilation changed expression evaluation");
        }

        [Test]
        public void Decompiler_LeavesConditionAndArgumentsReadable()
        {
            const string source = "if(a==0||b>2&&c<3){Wait(1+2*3);}";
            var compiler = new Compiler();
            Assert.That(compiler.TryCompile(source, out var originalCode, out var syntaxNodes,
                out _, out _, out _, out var errors), Is.True, string.Join("\n", errors));

            var restored = new Decompiler().Decompile(syntaxNodes);
            Assert.That(restored, Does.Contain("if (a == 0 || b > 2 && c < 3)"));
            Assert.That(restored, Does.Contain("Wait(1 + 2 * 3);"));
            Assert.That(compiler.TryCompile(restored, out var restoredCode, out _,
                out _, out _, out _, out errors), Is.True, string.Join("\n", errors));
            Assert.That(restoredCode, Is.EqualTo(originalCode));
        }

        [Test]
        public void Decompiler_SpacesArgumentsWithoutChangingStringsOrEmptySlots()
        {
            const string source = "ShowDialog(\"a,b + c\",,\"title\",\"text\",);";
            var compiler = new Compiler();
            Assert.That(compiler.TryCompile(source, out var originalCode, out var syntaxNodes,
                out _, out _, out _, out var errors), Is.True, string.Join("\n", errors));

            var restored = new Decompiler().Decompile(syntaxNodes);
            Assert.That(restored, Is.EqualTo("ShowDialog(\"a,b + c\", , \"title\", \"text\", );\n"));
            Assert.That(compiler.TryCompile(restored, out var restoredCode, out _,
                out _, out _, out _, out errors), Is.True, string.Join("\n", errors));
            Assert.That(restoredCode, Is.EqualTo(originalCode));
        }

        [Test]
        public void Decompiler_KeepsCompletelyEmptyForClausesCompact()
        {
            var compiler = new Compiler();
            Assert.That(compiler.TryCompile("for(;;){break;}", out _, out var syntaxNodes,
                out _, out _, out _, out var errors), Is.True, string.Join("\n", errors));

            Assert.That(new Decompiler().Decompile(syntaxNodes),
                Is.EqualTo("for (;;)\n{\n    break;\n}\n"));
        }

        [Test]
        public void Decompiler_IndentsNestedBraceContentsByFourSpaces()
        {
            const string source = "if(a){x=1;while(b){y=2;if(c){z=3;}else{z=4;}}}for(i=0;i<2;i++){Wait(i);}";
            const string expected = "if (a)\n" +
                                    "{\n" +
                                    "    x = 1;\n" +
                                    "    while (b)\n" +
                                    "    {\n" +
                                    "        y = 2;\n" +
                                    "        if (c)\n" +
                                    "        {\n" +
                                    "            z = 3;\n" +
                                    "        }\n" +
                                    "        else\n" +
                                    "        {\n" +
                                    "            z = 4;\n" +
                                    "        }\n" +
                                    "    }\n" +
                                    "}\n" +
                                    "for (i = 0; i < 2; i++)\n" +
                                    "{\n" +
                                    "    Wait(i);\n" +
                                    "}\n";

            var compiler = new Compiler();
            Assert.That(compiler.TryCompile(source, out var originalCode, out var syntaxNodes,
                out _, out _, out _, out var errors), Is.True, string.Join("\n", errors));

            var restored = new Decompiler().Decompile(syntaxNodes);
            Assert.That(restored, Is.EqualTo(expected));
            Assert.That(compiler.TryCompile(restored, out var restoredCode, out _,
                out _, out _, out _, out errors), Is.True, string.Join("\n", errors));
            Assert.That(restoredCode, Is.EqualTo(originalCode));
        }

        [Test]
        public void IfElse_BlocksRemainDistinctFromTheIfNode()
        {
            const string source = "if(a==0){}else{}";
            var compiler = new Compiler();

            var success = compiler.TryCompile(source, out _, out var syntaxNodes,
                out _, out _, out _, out var errors);

            Assert.That(success, Is.True, string.Join("\n", errors.Select(error => error.ToString())));
            Assert.That(syntaxNodes, Has.Count.EqualTo(1));

            var ifNode = syntaxNodes[0];
            Assert.That(ifNode.desc.type, Is.EqualTo(CodeType.Reserved));
            Assert.That(ifNode.desc.code, Is.EqualTo("if"));
            Assert.That(ifNode.subNodes, Has.Count.EqualTo(3));
            Assert.That(ifNode.subNodes[1].desc.type, Is.EqualTo(CodeType.Action));
            Assert.That(ifNode.subNodes[1].desc.code, Is.EqualTo("then"));
            Assert.That(ifNode.subNodes[1].subNodes, Is.Empty);
            Assert.That(ifNode.subNodes[2].desc.type, Is.EqualTo(CodeType.Action));
            Assert.That(ifNode.subNodes[2].desc.code, Is.EqualTo("else"));
            Assert.That(ifNode.subNodes[2].subNodes, Is.Empty);
        }

        [Test]
        public void StringEscapes_AreDecodedExactlyOnce()
        {
            const string source = "Return \"quote:\\\" slash:\\\\ newline:\\n tab:\\t\";";
            const string expected = "quote:\" slash:\\ newline:\n tab:\t";
            Assert.That(Execute(source).ret.str, Is.EqualTo(expected));

            const string singleQuotedSource = "Return 'it\\'s';";
            Assert.That(Execute(singleQuotedSource).ret.str, Is.EqualTo("it's"));
        }

        [Test]
        public void CrLfAndCarriageReturn_AreWhitespaceAndNeverBecomeVariables()
        {
            const string source = "a=1;\r\nb=2;\rReturn a+b;\r\n";
            var zCode = Compile(source, out _, out _, out _, "CRLF source");

            Assert.That(zCode, Does.Not.Contain("\r"));
            Assert.That(Execute(source).ret.num, Is.EqualTo(3f).Within(0.0001f));
        }

        [Test]
        public void SingleCharacterVariableFollowedByPlus_RemainsTwoTokens()
        {
            Assert.That(Execute("a=2;Return a+1;").ret.num, Is.EqualTo(3f).Within(0.0001f));
        }

        [Test]
        public void CustomProgramArguments_PreserveSourceOrder()
        {
            var functionName = $"ArgumentOrder_{Guid.NewGuid():N}";
            var function = CompileProgram(
                functionName,
                "Return param1*100+param2*10+param3;",
                $"custom function {functionName}");

            ProgramDataForm.AddData(function);
            try
            {
                var result = Execute($"Return {functionName}(1,2,3);");
                Assert.That(result.ret.num, Is.EqualTo(123f).Within(0.0001f));
            }
            finally
            {
                ProgramDataForm.RemoveData(function.uid);
            }
        }

        [Test]
        public void EmptyArgumentSlots_CompileAndExecuteAsEmptyStrings()
        {
            var functionName = $"EmptyArguments_{Guid.NewGuid():N}";
            var function = CompileProgram(
                functionName,
                "Return param1+param2+param3;",
                $"empty arguments function {functionName}");

            ProgramDataForm.AddData(function);
            try
            {
                string source = $"Return {functionName}(,\"middle\",);";
                var zCode = Compile(source, out _, out _, out _, "explicit empty argument slots");
                Assert.That(zCode.Count(value => value == string.Empty), Is.EqualTo(2));

                var result = Execute(source);
                Assert.That(result.ret.str, Is.EqualTo("middle"));
            }
            finally
            {
                ProgramDataForm.RemoveData(function.uid);
            }
        }

        [Test]
        public void CommandArguments_OmittedAtEnd_AreEmptyStrings()
        {
            const string commandName = "Len";
            bool hadPrevious = BaseData.cmdDic.TryGetValue(commandName, out var previous);
            BaseData.cmdDic[commandName] = new LenCmd();

            try
            {
                var result = Execute("Return Len();");
                Assert.That(result.ret.num, Is.EqualTo(0f).Within(0.0001f));
            }
            finally
            {
                if (hadPrevious)
                {
                    BaseData.cmdDic[commandName] = previous;
                }
                else
                {
                    BaseData.cmdDic.Remove(commandName);
                }
            }
        }

        [Test]
        public void CancellationDuringCommand_StopsBeforeTheNextOpcode()
        {
            const string commandName = "Len";
            bool hadPrevious = BaseData.cmdDic.TryGetValue(commandName, out var previous);
            BaseData.cmdDic[commandName] = new CancelInterpreterCmd();

            try
            {
                var result = Execute("Len();value=1;Return value;", out var data);

                Assert.That(result.complete, Is.True);
                Assert.That(data.IsCancelled, Is.True);
                Assert.That(data.heap.ContainsKey("value"), Is.False);
            }
            finally
            {
                if (hadPrevious)
                {
                    BaseData.cmdDic[commandName] = previous;
                }
                else
                {
                    BaseData.cmdDic.Remove(commandName);
                }
            }
        }

        [Test]
        public void WaitingCommand_DoesNotRecreateTheCommandOrPrepareArgumentsAgain()
        {
            string alias = $"DelayedProbe_{Guid.NewGuid():N}";
            var state = new DelayedProbeState();
            using (new RegistryEntry<CmdBase>(BaseData.cmdDic, alias, new DelayedProbeCmd(state)))
            {
                var program = CompileProgram("WaitingCommand", $"value=7;Return {alias}(value);", alias);
                var data = CreateInterpret(program);
                var result = data.Interpret();
                Assert.That(result.complete, Is.False);
                Assert.That(result.errors, Is.Empty);
                Assert.That(state.CreateCount, Is.EqualTo(1));
                Assert.That(state.ExecuteCount, Is.EqualTo(1));
                Assert.That(state.Parameters.Single().num, Is.EqualTo(7f));

                // Resolving this reference again would create an observable phantom heap entry.
                // A waiting command must not look at arguments already captured on its first pass.
                const string unexpectedVariable = "MustNotResolveWhileWaiting";
                data.stack.Single(box => box.valName == "value").valName = unexpectedVariable;
                var capturedParameters = state.Parameters;
                int waitingPc = data.p;
                for (int i = 0; i < 32; i++)
                {
                    result = data.Interpret();
                    Assert.That(result.complete, Is.False);
                    Assert.That(result.errors, Is.Empty);
                    Assert.That(data.p, Is.EqualTo(waitingPc));
                }

                Assert.That(state.CreateCount, Is.EqualTo(1));
                Assert.That(state.ExecuteCount, Is.EqualTo(1));
                Assert.That(state.Parameters, Is.SameAs(capturedParameters));
                Assert.That(data.heap.ContainsKey(unexpectedVariable), Is.False);

                state.Task.res = new[] { CodeHelper.CreateBoxByNum(9f) };
                state.Task.Complete();
                result = data.Interpret();
                Assert.That(result.complete, Is.True);
                Assert.That(result.errors, Is.Empty);
                Assert.That(result.ret.num, Is.EqualTo(9f));
                Assert.That(data.stack, Is.Empty);
                Assert.That(data.top, Is.EqualTo(-1));
                Assert.That(state.CreateCount, Is.EqualTo(1), "completion must not prepare the command again");
                Assert.That(data.heap.ContainsKey(unexpectedVariable), Is.False);

                data.Reset();
                result = data.Interpret();
                Assert.That(result.complete, Is.False);
                Assert.That(state.CreateCount, Is.EqualTo(2), "Reset must allow a new independent invocation");
                Assert.That(state.ExecuteCount, Is.EqualTo(2));
                data.Cancel();
            }
        }

        [Test]
        public void WaitingCommand_CompletesWithCapturedCountsAfterMetadataAndRegistryChanges()
        {
            string alias = $"ChangedWaitingProbe_{Guid.NewGuid():N}";
            var state = new DelayedProbeState();
            var metadata = CmdDataForm.DataByName["Len"].Copy();
            using (new RegistryEntry<CmdDataForm.Data>(CmdDataForm.DataByName, "Len", metadata))
            using (new RegistryEntry<CmdBase>(BaseData.cmdDic, alias, new DelayedProbeCmd(state)))
            {
                var program = CompileProgram("ChangedWaitingCommand", $"Return {alias}(7);", "captured command metadata");
                var data = CreateInterpret(program);
                var first = data.Interpret();
                Assert.That(first.complete, Is.False);
                Assert.That(first.errors, Is.Empty);

                metadata.prmNames = new List<string> { "changed1", "changed2" };
                metadata.retNames = new List<string> { "changedResult1", "changedResult2" };
                BaseData.cmdDic.Remove(alias);
                var waiting = data.Interpret();
                Assert.That(waiting.complete, Is.False);
                Assert.That(waiting.errors, Is.Empty, "an active call must not resolve its template again");
                state.Task.res = new[] { CodeHelper.CreateBoxByNum(19f) };
                state.Task.Complete();
                var completed = RunToCompletion(data, program.code);
                Assert.That(completed.ret.num, Is.EqualTo(19f));
                Assert.That(state.CreateCount, Is.EqualTo(1));
                Assert.That(state.ExecuteCount, Is.EqualTo(1));
                Assert.That(data.stack, Is.Empty);
            }
        }

        [Test]
        public void RetInfo_DefaultConstructionStillProvidesIndependentEmptyValues()
        {
            var first = new InterpretDataForm.RetInfo();
            var second = new InterpretDataForm.RetInfo();
            Assert.That(first.ret, Is.Not.Null);
            Assert.That(first.ret.num, Is.EqualTo(0f));
            Assert.That(first.ret.dic, Is.Not.Null.And.Empty);
            Assert.That(first.errors, Is.Not.Null.And.Empty);
            Assert.That(second.ret, Is.Not.SameAs(first.ret));
            Assert.That(second.ret.dic, Is.Not.SameAs(first.ret.dic));
            Assert.That(second.errors, Is.Not.SameAs(first.errors));
            first.ret.num = 5f;
            Assert.That(second.ret.num, Is.EqualTo(0f));
        }

        [Test]
        public void RetInfo_WithProvidedReturnValueDoesNotAllocateADiscardedDefaultBox()
        {
            var interpreter = new Interpreter(CreateInterpret(CompileProgram("RetInfoAllocation", "Return 1;", "ret info")));
            var method = typeof(Interpreter).GetMethod("MakeRetInfo", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            var makeInfo = (Func<bool, BoxDataForm.Data, InterpretDataForm.RetInfo>)Delegate.CreateDelegate(
                typeof(Func<bool, BoxDataForm.Data, InterpretDataForm.RetInfo>), interpreter, method);
            var provided = CodeHelper.CreateBoxByNum(17f);
            var result = makeInfo(true, provided);
            Assert.That(result.ret, Is.SameAs(provided));
            Assert.That(result.complete, Is.True);
            Assert.That(result.errors, Is.Empty);

            // Measure relative allocations, not a Mono/architecture-specific object byte size.
            long boxBytes = MeasureAllocations(() => allocationSink = CodeHelper.CreateBox());
            long defaultInfoBytes = MeasureAllocations(() => allocationSink = new InterpretDataForm.RetInfo());
            long suppliedInfoBytes = MeasureAllocations(() => allocationSink = makeInfo(true, provided));
            if (boxBytes == 0)
                Assert.Ignore("This Unity runtime reports zero even for explicit Box allocations; allocation counters are unavailable.");
            Assert.That(boxBytes, Is.GreaterThan(0));
            Assert.That(suppliedInfoBytes, Is.LessThanOrEqualTo(defaultInfoBytes - boxBytes),
                "a supplied return Box must bypass the public default constructor's Box allocation");
        }

        [Test]
        public void DictionaryAddAssign_ReturnsFreshFieldsWithoutChangingAliasedInput()
        {
            var original = CodeHelper.CreateBox();
            original.dic["x"] = CodeHelper.CreateBoxByNum(1f);
            original.dic["y"] = CodeHelper.CreateBoxByNum(2f);
            var program = CompileProgram("DictionaryAddAssign", "target+=1;target.x=100;Return alias.x;", "dictionary +=");
            var data = CreateInterpret(program);
            data.heap["target"] = original;
            data.heap["alias"] = original;

            var result = RunToCompletion(data, program.code);
            Assert.That(result.ret.num, Is.EqualTo(1f));
            Assert.That(original.dic["x"].num, Is.EqualTo(1f));
            Assert.That(original.dic["y"].num, Is.EqualTo(2f));
            Assert.That(data.heap["target"], Is.Not.SameAs(original));
            Assert.That(data.heap["target"].dic["y"], Is.Not.SameAs(original.dic["y"]));
            Assert.That(data.heap["target"].dic["x"].num, Is.EqualTo(100f));
            Assert.That(data.heap["target"].dic["y"].num, Is.EqualTo(3f));
        }

        [Test]
        public void ScalarBoxes_CreateDictionariesOnlyForMemberAccessOrCommandArguments()
        {
            var number = CodeHelper.CreateBoxByNum(7f);
            var text = CodeHelper.CreateBoxByStr("text");
            var reference = CodeHelper.CreateBoxByVal("value");
            Assert.That(number.dic, Is.Null);
            Assert.That(text.dic, Is.Null);
            Assert.That(reference.dic, Is.Null);
            var copy = number.DeepCopy();
            Assert.That(copy, Is.Not.SameAs(number));
            Assert.That(copy.num, Is.EqualTo(7f));
            Assert.That(copy.dic, Is.Null);

            Execute("value=7;copy=value;copy+=2;Return value;", out var data);
            Assert.That(data.heap["value"].dic, Is.Null);
            Assert.That(data.heap["copy"].dic, Is.Null);
            Assert.That(data.heap["value"].num, Is.EqualTo(7f));
            Assert.That(data.heap["copy"].num, Is.EqualTo(9f));
            Assert.That(Execute("Return Len(7);").ret.num, Is.EqualTo(0f));
        }

        [Test]
        public void ScalarMemberAccess_CreatesIndependentDictionariesOnAssignment()
        {
            var result = Execute("a=7;a.x=2;a[\"y\"]=3;b=a;b.x=9;Return a.x+b.y;", out var data);
            Assert.That(result.ret.num, Is.EqualTo(5f));
            Assert.That(data.heap["a"].num, Is.EqualTo(7f));
            Assert.That(data.heap["a"].dic["x"].num, Is.EqualTo(2f));
            Assert.That(data.heap["b"].dic["x"].num, Is.EqualTo(9f));
            Assert.That(data.heap["a"].dic, Is.Not.SameAs(data.heap["b"].dic));
            Assert.That(data.heap["a"].dic["y"], Is.Not.SameAs(data.heap["b"].dic["y"]));
        }

        [Test]
        public void TemporaryBoxes_ReuseObjectsAcrossLoopsAndKeepSuspendedOperandsIntact()
        {
            var program = CompileProgram("TemporaryLoop", "i=0;while(i<100){10+(i+1)*2;i+=1;}Return i;", "temporary pool");
            var data = CreateInterpret(program);
            var interpreter = new Interpreter(data);
            var observedStackValues = new HashSet<BoxDataForm.Data>();
            InterpretDataForm.RetInfo result = null;
            int steps = 0;
            // Suspend after every instruction, including between operands. Identity
            // reuse proves pooling even on Mono builds with unavailable GC counters.
            while (steps++ < 10000)
            {
                result = InterpretWithBudget(interpreter, 1);
                Assert.That(result.errors, Is.Empty);
                foreach (var box in data.stack)
                {
                    Assert.That(box.dic, Is.Null);
                    observedStackValues.Add(box);
                }
                if (result.complete) break;
            }
            Assert.That(result.complete, Is.True);
            Assert.That(result.ret.num, Is.EqualTo(100f));
            Assert.That(steps, Is.GreaterThan(1000));
            Assert.That(observedStackValues.Count, Is.LessThanOrEqualTo(16),
                "hundreds of scalar operations should reuse a bounded set of stack Boxes");
            Assert.That(data.heap["i"].dic, Is.Null);
            Assert.That(data.stack, Is.Empty);

            var nested = CreateInterpret(CompileProgram("SuspendedOperands", "Return 10+(3*4);", "suspended operands"));
            var nestedInterpreter = new Interpreter(nested);
            do { result = InterpretWithBudget(nestedInterpreter, 1); }
            while (!result.complete);
            Assert.That(result.errors, Is.Empty);
            Assert.That(result.ret.num, Is.EqualTo(22f));
        }

        [TestCase("Return 7+8;", 15f)]
        [TestCase("Return \"old\"+\" value\";", "old value")]
        public void TemporaryReturnValues_SurviveResetAndLaterPoolReuse(string source, object expected)
        {
            var program = CompileProgram("RetainedReturn", source, "retained return");
            var data = CreateInterpret(program);
            var first = RunToCompletion(data, source).ret;
            for (int i = 0; i < 4; i++)
            {
                data.Reset();
                Assert.That(program.TryApplyCode($"for(i=0;i<20;i++){{3+4;}}Return {i};", out _, out var errors),
                    Is.True, string.Join(Environment.NewLine, errors));
                var next = RunToCompletion(data, program.code).ret;
                Assert.That(next, Is.Not.SameAs(first));
                Assert.That(next.num, Is.EqualTo(i));
                if (expected is string text) Assert.That(first.str, Is.EqualTo(text));
                else Assert.That(first.num, Is.EqualTo((float)expected));
            }
        }

        [Test]
        public void SharedTemporaryPool_ReusesBoxesAcrossInterpretersWithoutRetainingTheOldOwner()
        {
            ResetSharedTemporaryPool();
            var firstData = CreateInterpret(CompileProgram("PoolProducer", "11;Return 0;", "shared pool producer"));
            var first = new Interpreter(firstData);
            Assert.That(InterpretWithBudget(first, 1).complete, Is.False);
            var released = firstData.stack.Single();
            Assert.That(released.num, Is.EqualTo(11f));
            Assert.That(InterpretWithBudget(first, 1).complete, Is.False);
            Assert.That(firstData.stack, Is.Empty);

            var ownerField = released.GetType().GetField("owner", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(ownerField, Is.Not.Null);
            Assert.That(ownerField.GetValue(released), Is.Null,
                "the global pool must not keep an old interpreter/scene alive");
            first.Reset();
            first.Cancel();

            var secondData = CreateInterpret(CompileProgram("PoolConsumer", "Return 23;", "shared pool consumer"));
            var second = new Interpreter(secondData);
            Assert.That(InterpretWithBudget(second, 1).complete, Is.False);
            Assert.That(secondData.stack.Single(), Is.SameAs(released),
                "a new interpreter should reuse another interpreter's free Box, even after its Reset/Cancel");
            Assert.That(released.num, Is.EqualTo(23f));
            Assert.That(ownerField.GetValue(released), Is.SameAs(second));
            var result = InterpretWithBudget(second, 1);
            Assert.That(result.complete, Is.True);
            Assert.That(result.errors, Is.Empty);
            Assert.That(result.ret.num, Is.EqualTo(23f));
            Assert.That(ownerField.GetValue(result.ret), Is.Null);
            Execute("for(i=0;i<100;i++){3+4;}Return 9;");
            Assert.That(result.ret.num, Is.EqualTo(23f), "an exported return must not reenter the shared pool");
        }

        [Test]
        public void SharedTemporaryPool_InterleavedInterpretersKeepTheirLiveStackValuesSeparate()
        {
            ResetSharedTemporaryPool();
            var firstData = CreateInterpret(CompileProgram("InterleavedA", "Return 10+(3*4);", "first suspended stack"));
            var secondData = CreateInterpret(CompileProgram("InterleavedB", "Return 20+(5*6);", "second suspended stack"));
            var first = new Interpreter(firstData);
            var second = new Interpreter(secondData);
            InterpretDataForm.RetInfo firstResult = null, secondResult = null;
            for (int step = 0; step < 32; step++)
            {
                if (firstResult == null || !firstResult.complete) firstResult = InterpretWithBudget(first, 1);
                if (secondResult == null || !secondResult.complete) secondResult = InterpretWithBudget(second, 1);
                Assert.That(firstResult.errors, Is.Empty);
                Assert.That(secondResult.errors, Is.Empty);
                Assert.That(firstData.stack.Intersect(secondData.stack), Is.Empty,
                    "only free Boxes may be shared, never suspended operands");
                if (firstResult.complete && secondResult.complete) break;
            }
            Assert.That(firstResult.complete, Is.True);
            Assert.That(secondResult.complete, Is.True);
            Assert.That(firstResult.ret.num, Is.EqualTo(22f));
            Assert.That(secondResult.ret.num, Is.EqualTo(50f));
        }

        [Test]
        public void SharedTemporaryPool_ClearingFreeEntriesPreservesSuspendedValues()
        {
            var data = CreateInterpret(CompileProgram("PoolClear", "Return 10+(3*4);", "pool clear"));
            var interpreter = new Interpreter(data);
            Assert.That(InterpretWithBudget(interpreter, 1).complete, Is.False);
            var suspended = data.stack.Single();
            float before = suspended.num;
            Execute("for(i=0;i<20;i++){3+4;}Return 9;");
            ResetSharedTemporaryPool();
            var other = Execute("Return 19;");
            Assert.That(other.ret, Is.Not.SameAs(suspended));
            Assert.That(data.stack.Single(), Is.SameAs(suspended));
            Assert.That(suspended.num, Is.EqualTo(before));
            var result = InterpretWithBudget(interpreter, 32);
            Assert.That(result.complete, Is.True);
            Assert.That(result.errors, Is.Empty);
            Assert.That(result.ret.num, Is.EqualTo(22f));
        }

        [TestCase("12", 12f)]
        [TestCase("10+2", 12f)]
        public void TemporaryCommandArguments_SurviveWaitingCompletionAndReset(string expression, float expected)
        {
            string alias = $"RetainingProbe_{Guid.NewGuid():N}";
            var state = new DelayedProbeState();
            using (new RegistryEntry<CmdBase>(BaseData.cmdDic, alias, new DelayedProbeCmd(state)))
            {
                var program = CompileProgram("RetainedArgument",
                    $"result={alias}({expression});for(i=0;i<100;i++){{i+2;}}Return result;", "retained argument");
                var data = CreateInterpret(program);
                Assert.That(data.Interpret().complete, Is.False);
                var retained = state.Parameters[0];
                Assert.That(retained.num, Is.EqualTo(expected));
                Assert.That(retained.dic, Is.Not.Null.And.Empty, "commands retain their writable dictionary API");
                for (int i = 0; i < 4; i++)
                {
                    Assert.That(data.Interpret().complete, Is.False);
                    Assert.That(retained.num, Is.EqualTo(expected));
                }
                Execute("for(i=0;i<100;i++){3+4;}Return 9;");
                Assert.That(retained.num, Is.EqualTo(expected),
                    "another interpreter must not reuse an asynchronous command's captured argument");
                retained.num = 19f;
                state.Task.res = state.Parameters;
                state.Task.Complete();
                Assert.That(RunToCompletion(data, program.code).ret.num, Is.EqualTo(19f));
                Assert.That(retained.num, Is.EqualTo(19f), "completed commands may still retain their parameters");

                data.Reset();
                Assert.That(data.Interpret().complete, Is.False);
                Assert.That(state.Parameters[0], Is.Not.SameAs(retained));
                data.Cancel();
                Assert.That(retained.num, Is.EqualTo(19f));
            }
        }

        [Test]
        public void TemporaryWaitValue_SurvivesRepushAndCancellation()
        {
            var program = CompileProgram("PooledWait", "Wait(1);Return 7;", "pooled wait");
            var data = CreateInterpret(program);
            Assert.That(data.Interpret().complete, Is.False);
            var countdown = data.stack.Single();
            float remaining = countdown.num;
            for (int i = 0; i < 4; i++)
            {
                Assert.That(data.Interpret().complete, Is.False);
                Assert.That(data.stack.Single(), Is.SameAs(countdown));
                Assert.That(countdown.num, Is.GreaterThan(0f).And.LessThanOrEqualTo(remaining));
                remaining = countdown.num;
            }
            countdown.num = 0f;
            Assert.That(RunToCompletion(data, program.code).ret.num, Is.EqualTo(7f));

            data.Reset();
            Assert.That(data.Interpret().complete, Is.False);
            var cancelledCountdown = data.stack.Single();
            float cancelledRemaining = cancelledCountdown.num;
            data.Cancel();
            data.Reset();
            Assert.That(data.Interpret().complete, Is.False);
            Assert.That(data.stack.Single(), Is.Not.SameAs(cancelledCountdown));
            Assert.That(cancelledCountdown.num, Is.EqualTo(cancelledRemaining));
            data.Cancel();
        }

        [Test]
        public void TemporarySubprogramArgument_CopyOutDoesNotClearTheReturnedDictionary()
        {
            string name = $"DictionaryCopyOut_{Guid.NewGuid():N}";
            var function = CompileProgram(name, "param1.x=7;Return param1;", "dictionary copy-out");
            using (new RegistryEntry<ProgramDataForm.Data>(ProgramDataForm.DataByName, name, function))
            {
                var result = Execute($"value={name}(1);for(i=0;i<100;i++){{3+4;}}Return value;", out var data);
                Assert.That(result.ret.num, Is.EqualTo(1f));
                Assert.That(result.ret.dic["x"].num, Is.EqualTo(7f));
                Assert.That(data.stack, Is.Empty);
            }
        }

        [Test]
        public void TemporaryFastFields_PreserveRegisteredCommandArgumentNotifications()
        {
            string alias = $"MutatingBoxProbe_{Guid.NewGuid():N}";
            var registered = CodeHelper.CreateBoxByNum(7f);
            int uid = BoxDataForm.AddData(registered);
            int notifications = 0;
            Action<BoxDataForm.Data, float, float> changed = (box, previous, next) =>
            {
                if (ReferenceEquals(box, registered)) notifications++;
            };
            BoxDataForm.changeNumAction += changed;
            try
            {
                using (new RegistryEntry<CmdBase>(BaseData.cmdDic, alias, new MutatingBoxProbeCmd()))
                {
                    var program = CompileProgram("RegisteredBox",
                        $"{alias}(value);for(i=0;i<20;i++){{3+4;}}Return value;", "registered Box notifications");
                    var data = CreateInterpret(program);
                    data.heap["value"] = registered;
                    var result = RunToCompletion(data, program.code);
                    Assert.That(result.ret, Is.SameAs(registered));
                    Assert.That(registered.uid, Is.EqualTo(uid));
                    Assert.That(registered.num, Is.EqualTo(8f));
                    Assert.That(BoxDataForm.DataByUid[uid], Is.SameAs(registered));
                    Assert.That(notifications, Is.EqualTo(1));
                }
            }
            finally
            {
                BoxDataForm.changeNumAction -= changed;
                BoxDataForm.RemoveData(uid);
            }
        }

        [Test]
        public void TemporaryBoxes_ClearTextReferenceAndDictionaryStateBeforeReuse()
        {
            ResetSharedTemporaryPool();
            var program = CompileProgram("TemporaryFields", "\"old text\";value;7;Return 0;", "temporary field cleanup");
            var data = CreateInterpret(program);
            var interpreter = new Interpreter(data);
            InterpretWithBudget(interpreter, 1);
            var temporary = data.stack.Single();
            Assert.That(temporary.str, Is.EqualTo("old text"));
            InterpretWithBudget(interpreter, 1); // Discard text.
            InterpretWithBudget(interpreter, 1); // Get variable reference.
            Assert.That(data.stack.Single(), Is.SameAs(temporary));
            Assert.That(temporary.str, Is.Null);
            Assert.That(temporary.valName, Is.EqualTo("value"));
            InterpretWithBudget(interpreter, 1); // Discard reference.
            InterpretWithBudget(interpreter, 1); // Push number.
            Assert.That(data.stack.Single(), Is.SameAs(temporary));
            Assert.That(temporary.num, Is.EqualTo(7f));
            Assert.That(temporary.uid, Is.EqualTo(-1));
            Assert.That(temporary.str, Is.Null);
            Assert.That(temporary.valName, Is.Null);
            Assert.That(temporary.dic, Is.Null);
            var result = InterpretWithBudget(interpreter, 32);
            Assert.That(result.complete, Is.True);
            Assert.That(result.errors, Is.Empty);
            Assert.That(result.ret.num, Is.EqualTo(0f));
        }

        [TestCase(Op.Plus)]
        [TestCase(Op.Less)]
        [TestCase(Op.Assign)]
        [TestCase(Op.AddAssign)]
        [TestCase(Op.Take)]
        [TestCase(Op.Sub)]
        public void BinaryInstructions_ReportStackUnderflowBeforeRemovingAnOperand(Op operation)
        {
            var program = new ProgramDataForm.Data(-1, "BinaryUnderflow", string.Empty,
                new List<string> { ((int)operation).ToString() }, new List<int> { 0 }, 0, "void");
            var data = CreateInterpret(program);
            var operand = CodeHelper.CreateBoxByNum(7f);
            data.stack.Add(operand);
            data.top = 0;
            var result = data.Interpret();
            Assert.That(result.complete, Is.True);
            Assert.That(result.errors, Has.Count.EqualTo(1));
            Assert.That(result.errors[0].Message, Does.Contain("运行栈不足"));
            Assert.That(result.errors[0].OpCode, Is.EqualTo(operation));
            Assert.That(data.stack, Has.Count.EqualTo(1));
            Assert.That(data.stack[0], Is.SameAs(operand));
            Assert.That(data.top, Is.EqualTo(0));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void BinaryInstructions_ReportNullOperandsWithoutPartiallyPopping(int emptyIndex)
        {
            var program = new ProgramDataForm.Data(-1, "BinaryNull", string.Empty,
                new List<string> { ((int)Op.Plus).ToString() }, new List<int> { 0 }, 0, "void");
            var data = CreateInterpret(program);
            data.stack.Add(CodeHelper.CreateBoxByNum(7f));
            data.stack.Add(CodeHelper.CreateBoxByNum(3f));
            data.stack[emptyIndex] = null;
            data.top = 1;
            var result = data.Interpret();
            Assert.That(result.complete, Is.True);
            Assert.That(result.errors, Has.Count.EqualTo(1));
            Assert.That(result.errors[0].Message, Does.Contain($"栈位置 {emptyIndex} 的值为空"));
            Assert.That(data.stack, Has.Count.EqualTo(2));
            Assert.That(data.top, Is.EqualTo(1));
        }

        [TestCase("left-right", 5f)]
        [TestCase("left/right", 4f)]
        [TestCase("left%right", 2f)]
        [TestCase("left>right", 1f)]
        [TestCase("left<right", 0f)]
        [TestCase("left>=right", 1f)]
        [TestCase("left<=right", 0f)]
        [TestCase("left==right", 0f)]
        [TestCase("left!=right", 1f)]
        [TestCase("\"8\"==left", 1f)]
        [TestCase("left!=\"8\"", 0f)]
        public void BinaryInstructions_PreserveResolvedOperandOrderAndComparisonSemantics(string expression, float expected)
        {
            // right is 3 except for division, where exact integer output is easier
            // to inspect. Both operands arrive as variable references.
            float right = expression == "left/right" ? 2f : 3f;
            var result = Execute($"left=8;right={right};Return {expression};", out var data);
            Assert.That(result.ret.num, Is.EqualTo(expected));
            Assert.That(data.stack, Is.Empty);
            Assert.That(data.top, Is.EqualTo(-1));
            Assert.That(data.heap["left"].num, Is.EqualTo(8f));
            Assert.That(data.heap["right"].num, Is.EqualTo(right));
        }

        [Test]
        public void NestedDictionaryArithmetic_ResolvesFieldsWithoutMutatingItsInputs()
        {
            var vector = CodeHelper.CreateBox();
            vector.dic["x"] = CodeHelper.CreateBoxByNum(8f);
            vector.dic["y"] = CodeHelper.CreateBoxByNum(-4f);
            var original = CodeHelper.CreateBox();
            original.dic["vector"] = vector;
            var program = CompileProgram("NestedArithmetic", "result=source/2;result+=source;Return result;", "nested arithmetic");
            var data = CreateInterpret(program);
            data.heap["source"] = original;
            var result = RunToCompletion(data, program.code);
            var returnedVector = result.ret.dic["vector"];
            Assert.That(returnedVector, Is.Not.SameAs(vector));
            Assert.That(returnedVector.dic["x"].num, Is.EqualTo(12f));
            Assert.That(returnedVector.dic["y"].num, Is.EqualTo(-6f));
            Assert.That(vector.dic["x"].num, Is.EqualTo(8f));
            Assert.That(vector.dic["y"].num, Is.EqualTo(-4f));
            Assert.That(original.dic["vector"], Is.SameAs(vector));
        }

        [Test]
        public void TemporaryOperands_AreSafeToReuseAfterAnInstructionThrows()
        {
            var program = CompileProgram("PooledError", "Return 8/0;", "pooled error");
            var data = CreateInterpret(program);
            var failed = data.Interpret();
            Assert.That(failed.complete, Is.True);
            Assert.That(failed.errors, Has.Count.EqualTo(1));
            data.Reset();
            Assert.That(program.TryApplyCode("Return 3+4;", out _, out var errors), Is.True,
                string.Join(Environment.NewLine, errors));
            Assert.That(RunToCompletion(data, program.code).ret.num, Is.EqualTo(7f));
            Assert.That(failed.ret.num, Is.EqualTo(0f));
            Assert.That(failed.errors, Has.Count.EqualTo(1));
        }

        [Test]
        public void SharedParseCache_DoesNotShareInterpreterStateAndSurvivesReset()
        {
            var program = CompileProgram("SharedParseCache", "value=param1+1;Return value;", "shared parse cache");
            var first = CreateInterpret(program);
            var second = CreateInterpret(program);
            first.heap["param1"] = CodeHelper.CreateBoxByNum(2f);
            second.heap["param1"] = CodeHelper.CreateBoxByNum(8f);
            Assert.That(RunToCompletion(first, program.code).ret.num, Is.EqualTo(3f));
            Assert.That(RunToCompletion(second, program.code).ret.num, Is.EqualTo(9f));
            Assert.That(first.heap, Is.Not.SameAs(second.heap));
            Assert.That(first.heap["value"], Is.Not.SameAs(second.heap["value"]));
            Assert.That(first.stack, Is.Not.SameAs(second.stack));
            AssertSharedParseArrays(first, second);

            first.Cancel();
            Assert.That(second.IsCancelled, Is.False);
            first.Reset();
            first.heap["param1"] = CodeHelper.CreateBoxByNum(20f);
            Assert.That(RunToCompletion(first, program.code).ret.num, Is.EqualTo(21f));
            Assert.That(second.heap["value"].num, Is.EqualTo(9f));
            AssertSharedParseArrays(first, second);
        }

        [Test]
        public void CompilerReuse_DoesNotMutatePreviouslyPublishedLists()
        {
            var compiler = new Compiler();
            Assert.That(compiler.TryCompile("Return 1;", out var firstCode, out _, out _, out _,
                out var firstMap, out var firstErrors), Is.True, string.Join(Environment.NewLine, firstErrors));
            var retainedCode = firstCode.ToArray();
            var retainedMap = firstMap.ToArray();
            Assert.That(compiler.TryCompile("value=23;Return value+4;", out var nextCode, out _, out _, out _,
                out var nextMap, out var nextErrors), Is.True, string.Join(Environment.NewLine, nextErrors));

            Assert.That(nextCode, Is.Not.SameAs(firstCode));
            Assert.That(nextMap, Is.Not.SameAs(firstMap));
            Assert.That(firstCode, Is.EqualTo(retainedCode));
            Assert.That(firstMap, Is.EqualTo(retainedMap));
        }

        [Test]
        public void SharedParseCache_ReplacingSameLengthCompiledCodeInvalidatesParsedNumbers()
        {
            var program = CompileProgram("ReplaceSharedParseCache", "Return 1;", "initial parse cache");
            var first = CreateInterpret(program);
            Assert.That(RunToCompletion(first, program.code).ret.num, Is.EqualTo(1f));
            var previousArrays = ParseArrays(first);
            var previousCode = program.zCode;

            Assert.That(program.TryApplyCode("Return 9;", out _, out var errors), Is.True,
                string.Join(Environment.NewLine, errors));
            Assert.That(program.zCode, Has.Count.EqualTo(previousCode.Count));
            Assert.That(program.zCode, Is.Not.SameAs(previousCode));
            first.Reset();
            Assert.That(RunToCompletion(first, program.code).ret.num, Is.EqualTo(9f));
            var revisedArrays = ParseArrays(first);
            for (int i = 0; i < previousArrays.Length; i++)
                Assert.That(revisedArrays[i], Is.Not.SameAs(previousArrays[i]), "replaced source retained stale parse arrays");

            var second = CreateInterpret(program);
            Assert.That(RunToCompletion(second, program.code).ret.num, Is.EqualTo(9f));
            AssertSharedParseArrays(first, second);
        }

        [Test]
        public void SharedParseCache_ReplacingRawCodeDoesNotReuseAnOldToken()
        {
            var program = CompileProgram("MutatedRawParseCache", "Return 1;", "raw parse cache");
            var first = CreateInterpret(program);
            Assert.That(RunToCompletion(first, program.code).ret.num, Is.EqualTo(1f));
            var originalCode = program.zCode;
            int operandIndex = program.zCode.FindIndex(token => token == ((int)Op.PushNum).ToString());
            Assert.That(operandIndex, Is.GreaterThanOrEqualTo(0));
            var replacementCode = new List<string>(originalCode);
            replacementCode[operandIndex + 1] = "23";
            program.zCode = replacementCode;

            first.Reset();
            Assert.That(RunToCompletion(first, program.code).ret.num, Is.EqualTo(23f));
            Assert.That(program.zCode, Is.Not.SameAs(originalCode));
            Assert.That(originalCode[operandIndex + 1], Is.EqualTo("1"));
            var second = CreateInterpret(program);
            Assert.That(RunToCompletion(second, program.code).ret.num, Is.EqualTo(23f));
            AssertSharedParseArrays(first, second);
        }

        [Test]
        public void RuntimeSnapshot_SharesOnlyPrivateCompiledListsUntilTheSourceChanges()
        {
            var program = CompileProgram("RuntimeSnapshot", "Return 1;", "initial runtime snapshot");
            var first = program.CreateRuntimeSnapshot();
            var second = program.CreateRuntimeSnapshot();
            Assert.That(first, Is.Not.SameAs(second));
            Assert.That(first.zCode, Is.SameAs(second.zCode));
            Assert.That(first.zCodeMap, Is.SameAs(second.zCodeMap));
            Assert.That(first.zCode, Is.Not.SameAs(program.zCode));
            Assert.That(first.zCodeMap, Is.Not.SameAs(program.zCodeMap));
            first.name = "ChangedSnapshotMetadata";
            Assert.That(second.name, Is.EqualTo("RuntimeSnapshot"));

            Assert.That(program.TryApplyCode("Return 9;", out _, out var errors), Is.True,
                string.Join(Environment.NewLine, errors));
            var revised = program.CreateRuntimeSnapshot();
            Assert.That(revised.zCode, Is.Not.SameAs(first.zCode));
            Assert.That(revised.zCodeMap, Is.Not.SameAs(first.zCodeMap));
            Assert.That(RunToCompletion(CreateInterpret(first), first.code).ret.num, Is.EqualTo(1f));
            Assert.That(RunToCompletion(CreateInterpret(revised), revised.code).ret.num, Is.EqualTo(9f));
        }

        [Test]
        public void RuntimeSnapshot_PreservesDerivedProgramTypeAndMetadataWithoutSharingTheWrapper()
        {
            var source = new DerivedSnapshotProgram(
                CompileProgram("DerivedSnapshot", "Return 6;", "derived snapshot"), 29);
            var snapshot = source.CreateRuntimeSnapshot();
            Assert.That(snapshot, Is.TypeOf<DerivedSnapshotProgram>());
            Assert.That(snapshot, Is.Not.SameAs(source));
            Assert.That(((DerivedSnapshotProgram)snapshot).Marker, Is.EqualTo(29));
            Assert.That(snapshot.uid, Is.EqualTo(source.uid));
            Assert.That(snapshot.paramCount, Is.EqualTo(source.paramCount));
            Assert.That(snapshot.returnValue, Is.EqualTo(source.returnValue));
            Assert.That(snapshot.zCode, Is.Not.SameAs(source.zCode));
            Assert.That(RunToCompletion(CreateInterpret(snapshot), snapshot.code).ret.num, Is.EqualTo(6f));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RuntimeSnapshot_DetectsReplacementOfEitherCompiledList(bool replaceCode)
        {
            var program = CompileProgram("ReplacedRuntimeSnapshot", "Return 1;", "immutable runtime snapshot");
            var first = program.CreateRuntimeSnapshot();
            if (replaceCode)
                program.zCode = new List<string>(program.zCode);
            else
                program.zCodeMap = new List<int>(program.zCodeMap);

            var revised = program.CreateRuntimeSnapshot();
            Assert.That(revised.zCode, Is.Not.SameAs(first.zCode));
            Assert.That(revised.zCodeMap, Is.Not.SameAs(first.zCodeMap));
            Assert.That(revised.zCode, Is.EqualTo(first.zCode));
            Assert.That(revised.zCodeMap, Is.EqualTo(first.zCodeMap));
            Assert.That(RunToCompletion(CreateInterpret(first), first.code).ret.num, Is.EqualTo(1f));
            Assert.That(RunToCompletion(CreateInterpret(revised), revised.code).ret.num, Is.EqualTo(1f));
            var unchanged = program.CreateRuntimeSnapshot();
            Assert.That(unchanged.zCode, Is.SameAs(revised.zCode));
            Assert.That(unchanged.zCodeMap, Is.SameAs(revised.zCodeMap));
        }

        [Test]
        public void RuntimeSnapshot_KeepsRunningCommandAndFollowingOpcodesOnItsOriginalRevision()
        {
            string alias = $"SnapshotProbe_{Guid.NewGuid():N}";
            var state = new DelayedProbeState();
            using (new RegistryEntry<CmdBase>(BaseData.cmdDic, alias, new DelayedProbeCmd(state)))
            {
                var source = CompileProgram("RunningSnapshot", $"{alias}(1);Return 3;", "running runtime snapshot");
                var running = CreateInterpret(source.CreateRuntimeSnapshot());
                var first = running.Interpret();
                Assert.That(first.complete, Is.False);
                Assert.That(first.errors, Is.Empty);
                Assert.That(source.TryApplyCode($"{alias}(1);Return 8;", out _, out var errors), Is.True,
                    string.Join(Environment.NewLine, errors));
                var nextRevision = source.CreateRuntimeSnapshot();

                state.Task.res = new[] { CodeHelper.CreateBoxByNum(0f) };
                state.Task.Complete();
                Assert.That(RunToCompletion(running, running.program.code).ret.num, Is.EqualTo(3f));
                var next = CreateInterpret(nextRevision);
                Assert.That(next.Interpret().complete, Is.False);
                Assert.That(state.ExecuteCount, Is.EqualTo(2));
                state.Task.res = new[] { CodeHelper.CreateBoxByNum(0f) };
                state.Task.Complete();
                Assert.That(RunToCompletion(next, nextRevision.code).ret.num, Is.EqualTo(8f));
            }
        }

        [Test]
        public void WaitingSubProgram_ResumesItsSnapshotAfterSourceSignatureAndRegistryChanges()
        {
            string alias = $"ChildWaitingProbe_{Guid.NewGuid():N}";
            string functionName = $"ChangedWaitingChild_{Guid.NewGuid():N}";
            var state = new DelayedProbeState();
            using (new RegistryEntry<CmdBase>(BaseData.cmdDic, alias, new DelayedProbeCmd(state)))
            {
                var function = CompileProgram(functionName, $"{alias}(param1);Return param1;", "waiting child snapshot");
                using (new RegistryEntry<ProgramDataForm.Data>(ProgramDataForm.DataByName, functionName, function))
                {
                    var program = CompileProgram("ChangedWaitingChildParent", $"Return {functionName}(7);", "waiting child parent");
                    var data = CreateInterpret(program);
                    var first = data.Interpret();
                    Assert.That(first.complete, Is.False);
                    Assert.That(first.errors, Is.Empty);
                    var capturedProgram = data.subInterpret.program;
                    Assert.That(capturedProgram.paramCount, Is.EqualTo(1));
                    Assert.That(function.TryApplyCode("Return param1+param2;", out _, out var errors), Is.True,
                        string.Join(Environment.NewLine, errors));
                    Assert.That(function.paramCount, Is.EqualTo(2));
                    ProgramDataForm.DataByName.Remove(functionName);

                    var waiting = data.Interpret();
                    Assert.That(waiting.complete, Is.False);
                    Assert.That(waiting.errors, Is.Empty);
                    Assert.That(data.subInterpret.program, Is.SameAs(capturedProgram));
                    state.Task.res = new[] { CodeHelper.CreateBoxByNum(0f) };
                    state.Task.Complete();
                    var completed = RunToCompletion(data, program.code);
                    Assert.That(completed.ret.num, Is.EqualTo(7f));
                    Assert.That(completed.errors, Is.Empty);
                    Assert.That(data.subInterpret, Is.Null);
                    Assert.That(data.stack, Is.Empty);
                    Assert.That(state.ExecuteCount, Is.EqualTo(1));
                }
            }
        }

        [Test]
        public void CustomProgram_BudgetIsSharedAcrossParentAndChildAndResumesWithoutLosingState()
        {
            string name = $"SharedBudget_{Guid.NewGuid():N}";
            var function = CompileProgram(name, "for(i=0;i<1000;i++){param1+=1;}Return param1;", "child instruction budget");
            using (new RegistryEntry<ProgramDataForm.Data>(ProgramDataForm.DataByName, name, function))
            {
                var program = CompileProgram("ParentBudget", $"value={name}(0);Return value;", "parent instruction budget");
                var data = CreateInterpret(program);
                var first = data.Interpret();
                Assert.That(first.errors, Is.Empty);
                Assert.That(first.complete, Is.False, "the child must not receive a fresh unbounded instruction budget");
                Assert.That(data.subInterpret, Is.Not.Null);
                Assert.That(data.subInterpret.heap["param1"].num, Is.GreaterThan(0f).And.LessThan(1000f));
                Assert.That(RunToCompletion(data, program.code).ret.num, Is.EqualTo(1000f));
                Assert.That(data.subInterpret, Is.Null);
                Assert.That(data.stack, Is.Empty);
            }
        }

        [Test]
        public void RecursiveProgram_WithSharedParseCacheStillHonorsCallDepthLimit()
        {
            string name = $"RecursiveDepth_{Guid.NewGuid():N}";
            var function = new ProgramDataForm.Data(-1, name, string.Empty,
                new List<string>(), new List<int>(), 1, "var");
            using (new RegistryEntry<ProgramDataForm.Data>(ProgramDataForm.DataByName, name, function))
            {
                Assert.That(function.TryApplyCode($"Return {name}(param1);", out _, out var compileErrors), Is.True,
                    string.Join(Environment.NewLine, compileErrors));
                var data = CreateInterpret(function);
                data.heap["param1"] = CodeHelper.CreateBoxByNum(1f);
                var result = data.Interpret();
                Assert.That(result.complete, Is.True);
                Assert.That(result.errors, Has.Count.EqualTo(1));
                Assert.That(result.errors[0].Message, Does.Contain("调用层级超过限制"));
            }
        }

        [Test]
        public void TryApplyCode_DoesNotOverwriteLastValidProgramOnCompileFailure()
        {
            var program = CompileProgram("SafeApply", "Return 1;", "initial valid program");
            var originalCode = program.code;
            var originalZCode = new List<string>(program.zCode);

            var applied = program.TryApplyCode("value=;", out _, out var errors);

            Assert.That(applied, Is.False);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(program.code, Is.EqualTo(originalCode));
            Assert.That(program.zCode, Is.EqualTo(originalZCode));
        }

        [Test]
        public void ExcessiveNesting_ReturnsCompileErrorsWithoutThrowing()
        {
            int count = SyntaxAnalysis.MaxNestingDepth + 32;
            var sources = new[]
            {
                "Return " + new string('!', count) + "0;",
                new string('(', count) + "1" + new string(')', count) + ";",
                string.Concat(Enumerable.Repeat("while(1){", count)) +
                "break;" + new string('}', count)
            };

            foreach (string source in sources)
            {
                var compiler = new Compiler();
                bool success = true;
                List<string> zCode = null;
                List<int> zCodeMap = null;
                List<CompileError> errors = null;

                Assert.DoesNotThrow(() =>
                {
                    success = compiler.TryCompile(source, out zCode, out _, out _, out _,
                        out zCodeMap, out errors);
                });
                Assert.That(success, Is.False);
                Assert.That(errors, Is.Not.Null.And.Not.Empty);
                Assert.That(zCode, Is.Empty);
                Assert.That(zCodeMap, Is.Empty);
            }
        }

        [Test]
        public void ExpressionStatements_DoNotGrowTheRuntimeStackAcrossLoops()
        {
            var functionName = $"DiscardedCall_{Guid.NewGuid():N}";
            var function = CompileProgram(functionName, "value=1;", "discarded custom call");
            ProgramDataForm.AddData(function);
            try
            {
                var result = Execute($@"
i=0;
while(i<200)
{{
    1+1;
    {functionName}();
    i=i+1;
}}
Return i;", out var data);

                Assert.That(result.ret.num, Is.EqualTo(200f).Within(0.0001f));
                Assert.That(data.stack, Is.Empty);
                Assert.That(data.top, Is.EqualTo(-1));
            }
            finally
            {
                ProgramDataForm.RemoveData(function.uid);
            }
        }

        private static ProgramDataForm.Data CompileProgram(string name, string source, string context)
        {
            var zCode = Compile(source, out var zCodeMap, out var paramCount, out var returnValue, context);
            return new ProgramDataForm.Data(-1, name, source, zCode, zCodeMap, paramCount, returnValue);
        }

        private static List<string> Compile(
            string source,
            out List<int> zCodeMap,
            out int paramCount,
            out string returnValue,
            string context)
        {
            var compiler = new Compiler();
            var zCode = compiler.Compile(
                source,
                out _,
                out paramCount,
                out returnValue,
                out zCodeMap,
                out var errors);

            Assert.That(errors, Is.Empty, $"{context}: {string.Join(Environment.NewLine, errors)}");
            Assert.That(zCode, Is.Not.Null.And.Not.Empty, $"{context}: compiler returned no code");
            return zCode;
        }

        private static InterpretDataForm.RetInfo Execute(string source)
        {
            return Execute(source, out _);
        }

        private static InterpretDataForm.RetInfo Execute(string source, out InterpretDataForm.Data data)
        {
            var program = CompileProgram($"Test_{Guid.NewGuid():N}", source, source);
            data = CreateInterpret(program);
            return RunToCompletion(data, source);
        }

        private static InterpretDataForm.Data CreateInterpret(ProgramDataForm.Data program)
        {
            return new InterpretDataForm.Data(
                -1,
                new List<BoxDataForm.Data>(),
                new Dictionary<string, BoxDataForm.Data>(),
                program,
                0,
                -1,
                0,
                null,
                new List<BoxDataForm.Data>(),
                0);
        }

        private static InterpretDataForm.RetInfo RunToCompletion(InterpretDataForm.Data data, string source)
        {
            InterpretDataForm.RetInfo result = null;
            for (var guard = 0; guard < 32; guard++)
            {
                result = data.Interpret();
                if (result.complete)
                {
                    break;
                }
            }

            Assert.That(result, Is.Not.Null);
            Assert.That(result.complete, Is.True, $"Interpreter did not complete: {source}");
            Assert.That(result.errors, Is.Empty,
                $"Interpreter errors for {source}:{Environment.NewLine}{string.Join(Environment.NewLine, result.errors)}");
            return result;
        }

        private static object[] ParseArrays(InterpretDataForm.Data data)
        {
            var interpreterField = typeof(InterpretDataForm.Data).GetField("_interpreter",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(interpreterField, Is.Not.Null);
            var interpreter = interpreterField.GetValue(data);
            Assert.That(interpreter, Is.Not.Null);
            string[] fields = { "opcodeCache", "opcodeCacheValid", "numberCache", "numberCacheValid",
                "integerCache", "integerCacheValid" };
            return fields.Select(name =>
            {
                var field = typeof(Interpreter).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(field, Is.Not.Null, $"Missing parse cache field: {name}");
                var value = field.GetValue(interpreter);
                Assert.That(value, Is.Not.Null, $"Uninitialized parse cache field: {name}");
                return value;
            }).ToArray();
        }

        private static void AssertSharedParseArrays(InterpretDataForm.Data first, InterpretDataForm.Data second)
        {
            var firstArrays = ParseArrays(first);
            var secondArrays = ParseArrays(second);
            for (int i = 0; i < firstArrays.Length; i++)
                Assert.That(firstArrays[i], Is.SameAs(secondArrays[i]), "same program allocated per-interpreter parse arrays");
        }

        private static void ResetSharedTemporaryPool()
        {
            var reset = typeof(Interpreter).GetMethod("ResetTemporaryBoxPool", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(reset, Is.Not.Null);
            reset.Invoke(null, null);
        }

        private static InterpretDataForm.RetInfo InterpretWithBudget(Interpreter interpreter, int instructionCount)
        {
            var budgetType = typeof(Interpreter).Assembly.GetType("Z_Code.InterpretBudget", true);
            var budget = Activator.CreateInstance(budgetType, instructionCount);
            var method = typeof(Interpreter).GetMethod("Interpret", BindingFlags.Instance | BindingFlags.NonPublic,
                null, new[] { budgetType }, null);
            return (InterpretDataForm.RetInfo)method.Invoke(interpreter, new[] { budget });
        }

        private static long MeasureAllocations(Action action)
        {
            const int iterations = 256;
            action();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < iterations; i++) action();
            return GC.GetAllocatedBytesForCurrentThread() - before;
        }
    }
}
