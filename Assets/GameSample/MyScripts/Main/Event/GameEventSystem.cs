using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_UnitSystem;

public class GameEventSystem:IZ_Listener<CollideEvent>
{
    public static CmdBase GetCmd(string name)
    {
        switch (name)
        {
            case "dialog":
                return new DialogCmd();
            default:
                return new TipsCmd();
        }
    }

    public void OnEvent(CollideEvent evt)
    {
        
    }
}
