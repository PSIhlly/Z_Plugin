using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Texture;

public class GameCharacterController : Z_Controller<GameManager>, IZ_Listener<CharacterEvent>
{
    public GameCharacterController(GameManager super) : base(super)
    {
        Z_EventHelper.Register(this);
    }

    public void LoadModel(CharacterInstance ins)
    {

    }

    public void UpdateAnim(CharacterInstance ins)
    {
        
    }
    

   
    public void OnEvent(CharacterEvent evt)
    {
        switch (evt.type)
        {
            case MapEventType.Show:
                break;
            case MapEventType.AfterUpdate:
                UpdateAnim((CharacterInstance)evt.unit.ins);
                break;
        }
    }

}
