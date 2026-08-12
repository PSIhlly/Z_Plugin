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

public class ModCmd : MonoBehaviour
{
    private enum Operation
    {
        Add,
        Del,
        Copy,
        Set
    }

    private sealed class FormApi
    {
        public string commandName;
        public string keyName;
        public PropertyInfo key;
        public PropertyInfo name;
        public PropertyInfo protoUid;
        public PropertyInfo defaultData;
        public PropertyInfo dataByKey;
        public MethodInfo addData;
        public MethodInfo removeData;
        public MethodInfo getJoByData;
        public MethodInfo getDataByJo;
        public MethodInfo copy;
        public MethodInfo reset;
    }

    private static Dictionary<string, FormApi> forms;

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
                result = "Story data commands can only be executed from ModStory. ModScene only accepts SceneAddTile, SceneAddItem, SceneAddObject, and SceneAddCharacter.";
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
                result = $"Unknown form '{formName}'. Available forms: {string.Join(", ", formApis.Keys.OrderBy(key => key))}.";
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

    private static bool IsSceneCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return false;

        int index = 0;
        SkipWhiteSpace(command, ref index);
        int commandStart = index;
        while (index < command.Length && !char.IsWhiteSpace(command[index]))
            index++;
        string commandName = command.Substring(commandStart, index - commandStart);
        return IsSceneCommandName(commandName);
    }

    private static bool IsSceneCommandName(string commandName)
    {
        return commandName.Equals("SceneAddTile", StringComparison.OrdinalIgnoreCase)
               || commandName.Equals("SceneAddItem", StringComparison.OrdinalIgnoreCase)
               || commandName.Equals("SceneAddObject", StringComparison.OrdinalIgnoreCase)
               || commandName.Equals("SceneAddCharacter", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryExecuteSceneCommand(string command, out bool isSceneCommand, out string result)
    {
        isSceneCommand = false;
        result = null;
        if (string.IsNullOrWhiteSpace(command))
            return false;

        int index = 0;
        SkipWhiteSpace(command, ref index);
        int commandStart = index;
        while (index < command.Length && !char.IsWhiteSpace(command[index]))
            index++;
        string commandName = command.Substring(commandStart, index - commandStart);

        bool isAddTile = commandName.Equals("SceneAddTile", StringComparison.OrdinalIgnoreCase);
        bool isAddItem = commandName.Equals("SceneAddItem", StringComparison.OrdinalIgnoreCase);
        bool isAddObject = commandName.Equals("SceneAddObject", StringComparison.OrdinalIgnoreCase);
        bool isAddCharacter = commandName.Equals("SceneAddCharacter", StringComparison.OrdinalIgnoreCase);
        isSceneCommand = IsSceneCommandName(commandName);
        if (!isSceneCommand)
            return false;

        if (!TryReadSceneArguments(command, ref index, out List<string> arguments, out result))
            return false;

        ModManager modManager = ModManager.instance;
        if (modManager.assetCtrl == null || modManager.sceneCtrl == null || MapManager.instance.data == null)
        {
            result = "A Mod scene must be active before executing a SceneAdd command.";
            return false;
        }

        if (isAddTile)
        {
            if (arguments.Count != 3
                || !int.TryParse(arguments[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int x)
                || !int.TryParse(arguments[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int y)
                || !int.TryParse(arguments[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int height))
            {
                result = "Usage: SceneAddTile {x} {y} {height}";
                return false;
            }

            TileUnitForm.Data tile = modManager.assetCtrl.AddTile(x, y, height);
            if (tile == null)
            {
                result = $"Failed to add a tile at ({x}, {y}, height {height}); the position is invalid or already occupied.";
                return false;
            }

            modManager.sceneCtrl.ForceUpdate();
            result = $"Added tile at ({x}, {y}, height {height}).";
            return true;
        }

        if (arguments.Count != 1 || string.IsNullOrWhiteSpace(arguments[0]))
        {
            result = $"Usage: {commandName} {{name}}. Quote names that contain spaces.";
            return false;
        }

        string name = arguments[0];
        Vector3 position = CameraInstance.instance.tarTrs.position;
        float angle = modManager.sceneCtrl.angle;

        if (isAddItem)
        {
            if (!ItemProductForm.DataByNameProtouid.TryGetValue((name, 0), out ItemProductForm.Data data))
            {
                result = $"ItemProduct '{name}' was not found.";
                return false;
            }

            if (modManager.assetCtrl.AddItem(data, position, angle) == null)
            {
                result = $"Failed to add ItemProduct '{name}' at the Mod scene camera target.";
                return false;
            }
        }
        else if (isAddObject)
        {
            if (!MapObjectForm.DataByName.TryGetValue(name, out MapObjectForm.Data data))
            {
                result = $"MapObject '{name}' was not found.";
                return false;
            }

            if (modManager.assetCtrl.AddObject(data, position, angle) == null)
            {
                result = $"Failed to add MapObject '{name}' at the Mod scene camera target.";
                return false;
            }
        }
        else
        {
            if (!CharacterProductForm.DataByNameProtouid.TryGetValue((name, 0), out CharacterProductForm.Data data))
            {
                result = $"CharacterProduct '{name}' was not found.";
                return false;
            }

            if (modManager.assetCtrl.AddCharacter(data, position, angle) == null)
            {
                result = $"Failed to add CharacterProduct '{name}' at the Mod scene camera target.";
                return false;
            }
        }

        modManager.sceneCtrl.ForceUpdate();
        result = $"Added '{name}' at the Mod scene camera target.";
        return true;
    }

    private static bool TryReadSceneArguments(
        string command,
        ref int index,
        out List<string> arguments,
        out string error)
    {
        arguments = new List<string>();
        error = null;
        SkipWhiteSpace(command, ref index);
        while (index < command.Length)
        {
            if (!TryReadArgument(command, ref index, out string argument, out error))
                return false;
            arguments.Add(argument);
            SkipWhiteSpace(command, ref index);
        }
        return true;
    }

    private static bool Add(FormApi form, string name, out string result)
    {
        if (Find(form, name) != null)
        {
            result = $"{form.commandName} '{name}' already exists.";
            return false;
        }

        var data = form.defaultData.GetValue(null);
        form.key.SetValue(data, -1);
        form.name.SetValue(data, name);
        form.protoUid?.SetValue(data, 0);

        var uid = (int)form.addData.Invoke(null, new object[] { data });
        if (uid < 0)
        {
            result = $"Failed to add {form.commandName} '{name}'.";
            return false;
        }

        result = $"Added {form.commandName} '{name}' ({form.keyName} {uid}).";
        return true;
    }

    private static bool Delete(FormApi form, string name, out string result)
    {
        var data = Find(form, name);
        if (data == null)
        {
            result = $"{form.commandName} '{name}' was not found.";
            return false;
        }

        form.removeData.Invoke(null, new[] { form.key.GetValue(data) });
        result = $"Deleted {form.commandName} '{name}'.";
        return true;
    }

    private static bool Copy(FormApi form, string name, out string result)
    {
        var source = Find(form, name);
        if (source == null)
        {
            result = $"{form.commandName} '{name}' was not found.";
            return false;
        }

        var copy = form.copy.Invoke(source, new object[] { false });
        var copyName = GetCopyName(form, GetName(form, source), GetProtoUid(form, source));
        form.name.SetValue(copy, copyName);

        var uid = (int)form.addData.Invoke(null, new object[] { copy });
        if (uid < 0)
        {
            result = $"Failed to copy {form.commandName} '{name}'.";
            return false;
        }

        result = $"Copied {form.commandName} '{name}' to '{copyName}' ({form.keyName} {uid}).";
        return true;
    }

    private static bool Set(FormApi form, string name, JObject patch, out string result)
    {
        var target = Find(form, name);
        if (target == null)
        {
            result = $"{form.commandName} '{name}' was not found.";
            return false;
        }

        var merged = (JObject)form.getJoByData.Invoke(null, new object[] { target });
        MergePreservingMissing(merged, patch);

        // The command name identifies the existing row. Its key must remain stable even if JSON contains it.
        var key = (int)form.key.GetValue(target);
        merged[form.keyName] = key;
        var replacement = form.getDataByJo.Invoke(null, new object[] { merged });
        var replacementName = GetName(form, replacement);
        var replacementProtoUid = GetProtoUid(form, replacement);

        if (HasNameConflict(form, replacementName, replacementProtoUid, key))
        {
            result = form.protoUid == null
                ? $"{form.commandName} '{replacementName}' already exists."
                : $"{form.commandName} '{replacementName}' already exists for protoUid {replacementProtoUid}.";
            return false;
        }

        form.reset.Invoke(target, new object[] { replacement });
        result = $"Set {form.commandName} '{name}' ({form.keyName} {key}).";
        return true;
    }

    private static void MergePreservingMissing(JObject current, JObject patch)
    {
        foreach (var property in patch.Properties())
        {
            if (property.Value is JObject patchObject && current[property.Name] is JObject currentObject)
            {
                MergePreservingMissing(currentObject, patchObject);
            }
            else
            {
                current[property.Name] = property.Value.DeepClone();
            }
        }
    }

    private static object Find(FormApi form, string name)
    {
        return GetData(form)
            .FirstOrDefault(data =>
                (form.protoUid == null || GetProtoUid(form, data) == 0) &&
                string.Equals(GetName(form, data), name, StringComparison.Ordinal));
    }

    private static bool HasNameConflict(FormApi form, string name, int protoUid, int exceptKey)
    {
        return GetData(form).Any(data =>
            (int)form.key.GetValue(data) != exceptKey &&
            (form.protoUid == null || GetProtoUid(form, data) == protoUid) &&
            string.Equals(GetName(form, data), name, StringComparison.Ordinal));
    }

    private static IEnumerable<object> GetData(FormApi form)
    {
        var dictionary = (IDictionary)form.dataByKey.GetValue(null);
        foreach (DictionaryEntry entry in dictionary)
            yield return entry.Value;
    }

    private static string GetName(FormApi form, object data)
    {
        return (string)form.name.GetValue(data);
    }

    private static int GetProtoUid(FormApi form, object data)
    {
        return form.protoUid == null ? 0 : (int)form.protoUid.GetValue(data);
    }

    private static string GetCopyName(FormApi form, string sourceName, int protoUid)
    {
        var baseName = sourceName + " Copy";
        var candidate = baseName;
        var suffix = 2;
        while (GetData(form).Any(data =>
                   (form.protoUid == null || GetProtoUid(form, data) == protoUid) &&
                   string.Equals(GetName(form, data), candidate, StringComparison.Ordinal)))
        {
            candidate = baseName + " " + suffix;
            suffix++;
        }
        return candidate;
    }

    private static Dictionary<string, FormApi> GetForms()
    {
        if (forms != null)
            return forms;

        forms = new Dictionary<string, FormApi>(StringComparer.OrdinalIgnoreCase);
        foreach (var type in GetLoadableTypes())
        {
            if (!type.IsAbstract || !type.IsSealed || !type.Name.EndsWith("ProductForm", StringComparison.Ordinal))
                continue;

            var commandName = type.Name.Substring(0, type.Name.Length - "ProductForm".Length);
            if (commandName.Length == 0)
                continue;

            var dataType = type.GetNestedType("Data", BindingFlags.Public | BindingFlags.NonPublic);
            if (dataType == null || dataType == typeof(ProductForm.Data) || !typeof(ProductForm.Data).IsAssignableFrom(dataType))
                continue;

            var api = CreateApi(type, dataType, commandName);
            if (api != null && !forms.ContainsKey(commandName))
                forms.Add(commandName, api);
        }

        RegisterAlias("SceneObject", typeof(MapObjectForm));
        RegisterAlias("Event", typeof(EventProgramDataForm));
        RegisterAlias("Mission", typeof(MissionForm));
        RegisterAlias("Tile", typeof(MapTextureForm));

        return forms;
    }

    private static void RegisterAlias(string commandName, Type formType)
    {
        var dataType = formType.GetNestedType("Data", BindingFlags.Public | BindingFlags.NonPublic);
        var api = dataType == null ? null : CreateApi(formType, dataType, commandName);
        if (api != null)
            forms[commandName] = api;
    }

    private static FormApi CreateApi(Type formType, Type dataType, string commandName)
    {
        const BindingFlags staticFlags = BindingFlags.Public | BindingFlags.Static;
        const BindingFlags instanceFlags = BindingFlags.Public | BindingFlags.Instance;
        var dataByKey = formType.GetProperty("DataByUid", staticFlags) ?? formType.GetProperty("DataById", staticFlags);
        var keyName = dataByKey?.Name == "DataByUid" ? "uid" : "id";

        var api = new FormApi
        {
            commandName = commandName,
            keyName = keyName,
            key = dataType.GetProperty(keyName, instanceFlags),
            name = dataType.GetProperty("name", instanceFlags),
            protoUid = dataType.GetProperty("protoUid", instanceFlags),
            defaultData = formType.GetProperty("defaultData", staticFlags),
            dataByKey = dataByKey,
            addData = formType.GetMethod("AddData", staticFlags, null, new[] { dataType }, null),
            removeData = formType.GetMethod("RemoveData", staticFlags, null, new[] { typeof(int) }, null),
            getJoByData = formType.GetMethod("GetJoByData", staticFlags, null, new[] { dataType }, null),
            getDataByJo = formType.GetMethod("GetDataByJo", staticFlags, null, new[] { typeof(JObject) }, null),
            copy = dataType.GetMethod("Copy", instanceFlags, null, new[] { typeof(bool) }, null),
            reset = dataType.GetMethod("Reset", instanceFlags, null, new[] { dataType }, null)
        };

        return api.key == null || api.name == null || api.defaultData == null || api.dataByKey == null ||
               api.addData == null || api.removeData == null || api.getJoByData == null ||
               api.getDataByJo == null || api.copy == null || api.reset == null
            ? null
            : api;
    }

    private static IEnumerable<Type> GetLoadableTypes()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                types = exception.Types;
            }

            foreach (var type in types)
            {
                if (type != null)
                    yield return type;
            }
        }
    }

    private static bool TryParse(
        string command,
        out Operation operation,
        out string formName,
        out string dataName,
        out JObject patch,
        out string error)
    {
        operation = default;
        formName = null;
        dataName = null;
        patch = null;
        error = null;

        if (string.IsNullOrWhiteSpace(command))
        {
            error = "Command is empty.";
            return false;
        }

        var index = 0;
        SkipWhiteSpace(command, ref index);
        var commandStart = index;
        while (index < command.Length && !char.IsWhiteSpace(command[index]))
            index++;
        var commandName = command.Substring(commandStart, index - commandStart);

        if (!TrySplitCommandName(commandName, out operation, out formName))
        {
            error = $"Unknown command '{commandName}'. Expected Add/Del/Copy/Set followed by a supported form name.";
            return false;
        }

        if (!TryReadArgument(command, ref index, out dataName, out error) || string.IsNullOrWhiteSpace(dataName))
        {
            if (error == null)
                error = "Data name is empty.";
            return false;
        }

        SkipWhiteSpace(command, ref index);
        if (operation != Operation.Set)
        {
            if (index != command.Length)
            {
                error = "Unexpected text after the data name.";
                return false;
            }
            return true;
        }

        if (index >= command.Length)
        {
            error = "Set command requires a JSON object.";
            return false;
        }

        var json = command.Substring(index).Trim();
        if (json.Length >= 2 && json[0] == '"' && json[json.Length - 1] == '"')
        {
            try
            {
                json = JsonConvert.DeserializeObject<string>(json);
            }
            catch (JsonException)
            {
                // Also accept the documented form: "{"field":value}" without escaped inner quotes.
                json = json.Substring(1, json.Length - 2).Replace("\\\"", "\"");
            }
        }

        try
        {
            patch = JObject.Parse(json);
            return true;
        }
        catch (JsonException exception)
        {
            error = "Invalid JSON: " + exception.Message;
            return false;
        }
    }

    private static bool TrySplitCommandName(string commandName, out Operation operation, out string formName)
    {
        foreach (Operation candidate in Enum.GetValues(typeof(Operation)))
        {
            var prefix = candidate.ToString();
            if (commandName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && commandName.Length > prefix.Length)
            {
                operation = candidate;
                formName = commandName.Substring(prefix.Length);
                return true;
            }
        }

        operation = default;
        formName = null;
        return false;
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
