using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Form;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Z_DataSystem.Form;
using Z_Map;
using Z_Map.Form;

public enum ModCmdScope
{
    Story,
    Scene
}

public partial class ModCmd : MonoBehaviour
{
    /// <summary>
    /// Executes a form command and returns either its success message or its error message.
    /// </summary>
    public string Execute(string command)
    {
        TryExecute(command, out var result);
        return result;
    }

    /// <summary>
    /// Executes Add/Del/Copy/Set commands for supported forms.
    /// </summary>
    public bool Execute(string command, out string result)
    {
        return TryExecute(command, out result);
    }

    public bool TryExecute(string command, out string result)
    {
        return TryExecuteInternal(command, null, out result);
    }

    public static bool TryExecute(string command, ModCmdScope scope, out string result)
    {
        return TryExecuteInternal(command, scope, out result);
    }

    private static bool TryExecuteInternal(string command, ModCmdScope? scope, out string result)
    {
        try
        {
            bool sceneCommand = IsSceneCommand(command);
            if (scope == ModCmdScope.Story && sceneCommand)
            {
                result = "Scene commands can only be executed from ModScene.";
                return false;
            }

            if (scope == ModCmdScope.Scene && !sceneCommand && !string.IsNullOrWhiteSpace(command))
            {
                result = "Story data commands can only be executed from ModStory. ModScene only accepts AddSceneTile, AddSceneItem, AddSceneObject, and AddSceneCharacter.";
                return false;
            }

            if (TryExecuteSceneCommand(command, out bool isSceneCommand, out result))
                return true;
            if (isSceneCommand)
                return false;

            if (!TryParse(command, out var operation, out var formName, out var dataName, out var patch, out result))
                return false;

            var formApis = GetForms();
            if (!formApis.TryGetValue(formName, out var form))
            {
                result = $"Unknown Story form '{formName}'. Available forms: {string.Join(", ", formApis.Keys.OrderBy(key => key).Select(key => "Story" + key))}.";
                return false;
            }

            switch (operation)
            {
                case Operation.Add:
                    return Add(form, dataName, out result);
                case Operation.Del:
                    return Delete(form, dataName, out result);
                case Operation.Copy:
                    return Copy(form, dataName, out result);
                case Operation.Set:
                    return Set(form, dataName, patch, out result);
                default:
                    result = "Unsupported operation.";
                    return false;
            }
        }
        catch (Exception exception)
        {
            var actual = exception is TargetInvocationException invocation && invocation.InnerException != null
                ? invocation.InnerException
                : exception;
            result = actual.Message;
            return false;
        }
    }

    private static bool TryReadArgument(string text, ref int index, out string value, out string error)
    {
        SkipWhiteSpace(text, ref index);
        value = null;
        error = null;
        if (index >= text.Length)
        {
            error = "Missing data name.";
            return false;
        }

        if (text[index] != '"')
        {
            var start = index;
            while (index < text.Length && !char.IsWhiteSpace(text[index]))
                index++;
            value = text.Substring(start, index - start);
            return true;
        }

        index++;
        var chars = new List<char>();
        while (index < text.Length)
        {
            var character = text[index++];
            if (character == '"')
            {
                value = new string(chars.ToArray());
                return true;
            }

            if (character != '\\' || index >= text.Length)
            {
                chars.Add(character);
                continue;
            }

            var escaped = text[index++];
            switch (escaped)
            {
                case '"': chars.Add('"'); break;
                case '\\': chars.Add('\\'); break;
                case 'n': chars.Add('\n'); break;
                case 'r': chars.Add('\r'); break;
                case 't': chars.Add('\t'); break;
                default:
                    chars.Add(escaped);
                    break;
            }
        }

        error = "Unterminated quoted data name.";
        return false;
    }

    private static void SkipWhiteSpace(string text, ref int index)
    {
        while (index < text.Length && char.IsWhiteSpace(text[index]))
            index++;
    }
}
