using Form;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Texture;
using Z_Time;


public enum ItemStyle
{
    verticalView = 0,
    leftView = 1,
    frontView = 2,
}

public class GameItemController : Z_Controller<GameManager>
{
    public GameItemController(GameManager super) : base(super)
    {
    }
}
