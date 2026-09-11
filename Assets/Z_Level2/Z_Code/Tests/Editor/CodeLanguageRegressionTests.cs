using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Z_Code.Form;

namespace Z_Code.Tests
{
    public class CodeLanguageRegressionTests
    {
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

        [Test]
        public void ExistingEfPrograms_CompileWithoutErrorsOrCarriageReturnOperands()
        {
            var efPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExtraAssets/2/Core/ef"));
            Assert.That(File.Exists(efPath), Is.True, $"Missing legacy ef fixture: {efPath}");

            var json = File.ReadAllText(efPath);
            var envelope = JsonUtility.FromJson<EfEnvelope>($"{{\"records\":{json}}}");
            Assert.That(envelope, Is.Not.Null);
            Assert.That(envelope.records, Is.Not.Null.And.Length.GreaterThan(0));

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

        [Test]
        public void ModuloAndLogicalNot_ExecuteWithNumericBooleanSemantics()
        {
            Assert.That(Execute("Return 7%4;").ret.num, Is.EqualTo(3f).Within(0.0001f));
            Assert.That(Execute("Return !0;").ret.num, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(Execute("Return !2;").ret.num, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void LogicalAndOr_ShortCircuitUnknownRightHandCall()
        {
            var missing = $"Missing_{Guid.NewGuid():N}";

            var andResult = Execute($"Return 0&&{missing}();");
            Assert.That(andResult.ret.num, Is.EqualTo(0f).Within(0.0001f));

            var orResult = Execute($"Return 1||{missing}();");
            Assert.That(orResult.ret.num, Is.EqualTo(1f).Within(0.0001f));
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
            data = new InterpretDataForm.Data(
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
    }
}
