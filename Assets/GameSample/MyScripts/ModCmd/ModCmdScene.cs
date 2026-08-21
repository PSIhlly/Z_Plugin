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

public partial class ModCmd
{
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
        return commandName.Equals("AddSceneTile", StringComparison.OrdinalIgnoreCase)
               || commandName.Equals("AddSceneItem", StringComparison.OrdinalIgnoreCase)
               || commandName.Equals("AddSceneObject", StringComparison.OrdinalIgnoreCase)
               || commandName.Equals("AddSceneCharacter", StringComparison.OrdinalIgnoreCase);
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

        bool isAddTile = commandName.Equals("AddSceneTile", StringComparison.OrdinalIgnoreCase);
        bool isAddItem = commandName.Equals("AddSceneItem", StringComparison.OrdinalIgnoreCase);
        bool isAddObject = commandName.Equals("AddSceneObject", StringComparison.OrdinalIgnoreCase);
        bool isAddCharacter = commandName.Equals("AddSceneCharacter", StringComparison.OrdinalIgnoreCase);
        isSceneCommand = IsSceneCommandName(commandName);
        if (!isSceneCommand)
            return false;

        if (!TryReadSceneArguments(command, ref index, out List<string> arguments, out result))
            return false;

        ModManager modManager = ModManager.instance;
        if (modManager.assetCtrl == null || modManager.sceneCtrl == null || MapManager.instance.data == null)
        {
            result = "A Mod scene must be active before executing an AddScene command.";
            return false;
        }

        if (isAddTile)
        {
            if (arguments.Count != 3
                || !int.TryParse(arguments[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int x)
                || !int.TryParse(arguments[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int y)
                || !int.TryParse(arguments[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int height))
            {
                result = "Usage: AddSceneTile {x} {y} {height}";
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
}
