using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Z_Code.Tests;

// Run the real NUnit assertions on Unity's Mono in an isolated project without
// installing a Test Runner package or touching the production scene/saves.
public static class CodeInterpreterRuntimeRegression
{
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
