using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Z_Code.Tests;
using Z_Code;
using Z_Code.Form;
using Stopwatch = System.Diagnostics.Stopwatch;

// Run the real NUnit assertions on Unity's Mono in an isolated project without
// installing a Test Runner package or touching the production scene/saves.
public static class CodeInterpreterRuntimeRegression
{
    // Stopwatch samples with Deep Profile/recording disabled, including real
    // compiler output, pooled child calls and root Reset work. No game/save load.
    public static void RunBenchmark()
    {
        var fixture = new CodeLanguageRegressionTests();
        bool recording = UnityEngine.Profiling.Profiler.enabled;
        try
        {
            UnityEngine.Profiling.Profiler.enabled = false;
            fixture.RegisterCommandsForEditMode();
            ProgramDataForm.InitInternal();
            var child = BenchmarkProgram("BenchScalarChild", "Return param1*param2+1;");
            ProgramDataForm.DataByName[child.name] = child;
            var middle = BenchmarkProgram("BenchScalarMiddle", "Return BenchScalarChild(param1,param2)+BenchScalarChild(param2,param1);");
            ProgramDataForm.DataByName[middle.name] = middle;
            var dictionaryChild = BenchmarkProgram("BenchDictionaryChild", "param1.x=param1.x+param2;Return param1;");
            ProgramDataForm.DataByName[dictionaryChild.name] = dictionaryChild;
            Benchmark("scalar-loop", "sum=0;for(i=0;i<8;i=i+1){if(i<4){sum=sum+i;}else{sum=sum-i;}}Return sum;");
            Benchmark("nested-calls", "Return BenchScalarMiddle(3,2);");
            Benchmark("dictionary-calls", "value.x=3;value.y=2;copy=BenchDictionaryChild(value,2);Return copy.x+value.x;");
            Benchmark("commands", "list[0]=1;list[1]=2;sum=0;for(i=0;i<4;i=i+1){sum=sum+Len(list);}Return sum;");
            Debug.Log("INTERPRETER_HOT_PATH_BENCHMARK_PASS");
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
        finally
        {
            fixture.RestoreCommandsAfterEditMode();
            UnityEngine.Profiling.Profiler.enabled = recording;
        }
    }

    private static ProgramDataForm.Data BenchmarkProgram(string name, string source)
    {
        var code = new Compiler().Compile(source, out _, out int parameters, out string returns,
            out var sourceMap, out var errors);
        if (errors.Count != 0) throw new InvalidOperationException(name + ": " + string.Join(";", errors));
        return new ProgramDataForm.Data(-1, name, source, code, sourceMap, parameters, returns);
    }

    private static void Benchmark(string name, string source)
    {
        const int owners = 40, frames = 100, samples = 7;
        var program = BenchmarkProgram("Benchmark_" + name, source);
        var calls = new InterpretDataForm.Data[owners];
        for (int i = 0; i < owners; i++)
            calls[i] = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(),
                new Dictionary<string, BoxDataForm.Data>(), program.CreateRuntimeSnapshot(), 0, -1, 0, null,
                new List<BoxDataForm.Data>(), 0);
        void Frame()
        {
            foreach (var call in calls)
            {
                call.Reset();
                var result = call.Interpret();
                if (!result.complete || result.errors.Count != 0)
                    throw new InvalidOperationException(name + " did not complete: " + string.Join(";", result.errors));
            }
        }
        for (int i = 0; i < 20; i++) Frame();
        var times = new double[samples];
        var watch = new Stopwatch();
        for (int i = 0; i < samples; i++)
        {
            GC.Collect();
            watch.Restart();
            for (int frame = 0; frame < frames; frame++) Frame();
            watch.Stop();
            times[i] = watch.Elapsed.TotalMilliseconds / frames;
        }
        Array.Sort(times);
        Debug.Log("INTERPRETER_BENCHMARK " + name + " owners=" + owners
            + " medianMs=" + times[samples/2].ToString("F4", System.Globalization.CultureInfo.InvariantCulture)
            + " minMs=" + times[0].ToString("F4", System.Globalization.CultureInfo.InvariantCulture)
            + " maxMs=" + times[samples-1].ToString("F4", System.Globalization.CultureInfo.InvariantCulture));
    }

    public static void Run()
    {
        var fixture = new CodeLanguageRegressionTests();
        int passed = 0;
        int skipped = 0;
        bool profilerEnabled = UnityEngine.Profiling.Profiler.enabled;
        try
        {
            UnityEngine.Profiling.Profiler.enabled = true;
            fixture.RegisterCommandsForEditMode();
            foreach (var method in typeof(CodeLanguageRegressionTests).GetMethods()
                .OrderBy(method => method.Name, StringComparer.Ordinal))
            {
                var cases = method.GetCustomAttributes(typeof(TestCaseAttribute), true)
                    .Cast<TestCaseAttribute>().ToArray();
                if (cases.Length > 0)
                {
                    foreach (var testCase in cases)
                    {
                        if (Invoke(fixture, method, testCase.Arguments)) passed++;
                        else skipped++;
                    }
                }
                else if (method.IsDefined(typeof(TestAttribute), true))
                {
                    if (Invoke(fixture, method, null)) passed++;
                    else skipped++;
                }
            }
            Debug.Log("CODE_INTERPRETER_RUNTIME_REGRESSION_PASS cases=" + passed + " skipped=" + skipped);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
        finally
        {
            fixture.RestoreCommandsAfterEditMode();
            UnityEngine.Profiling.Profiler.enabled = profilerEnabled;
        }
    }

    private static bool Invoke(object fixture, MethodInfo method, object[] arguments)
    {
        try
        {
            method.Invoke(fixture, arguments);
            Debug.Log("CODE_INTERPRETER_CASE_PASS " + method.Name);
            return true;
        }
        catch (TargetInvocationException error)
        {
            if (error.InnerException is IgnoreException)
            {
                Debug.LogWarning("CODE_INTERPRETER_CASE_SKIPPED " + method.Name + ": " + error.InnerException.Message);
                return false;
            }
            throw new InvalidOperationException("Interpreter regression failed: " + method.Name,
                error.InnerException ?? error);
        }
    }
}
