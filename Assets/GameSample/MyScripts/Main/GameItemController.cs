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
namespace Form
{

    public static partial class ItemProductForm
    {
        public partial class Data
        {
            public bool CanShow(string prmName)
            {
                if (!paramDic.ContainsKey(prmName) || !ItemParamForm.DataByName.ContainsKey(prmName))
                {
                    return false;
                }
                var protoPrm = ItemParamForm.DataByName[prmName];
                switch (protoPrm.showType)
                {
                    case ParamShowType.Always:
                    case ParamShowType.AlwaysWithPanel:
                    case ParamShowType.AlwaysWithPanelAndScene:
                    case ParamShowType.AlwaysWithPanelAndSceneWithoutPlayer:
                        return true;
                    case ParamShowType.OnlyNotZero:
                        return paramDic[prmName].GetValue().num != 0;
                    case ParamShowType.Hide:
                        return false;
                }
                return false;
            }
        }
    }
}
public class GameItemController : Z_Controller<GameManager>
{
    public GameItemController(GameManager super) : base(super)
    {
    }
}
