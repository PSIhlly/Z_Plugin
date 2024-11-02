using Z_Ui.Form.Dialog;
using Z_Ui.Form.Img;
using Z_Ui.Form.Npc;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Texture;
using Z_Ui.Dialog;
public class Z_Ui_Sample : MonoBehaviour
{
    private void Start()
    {
        var contentLst=new List<string>();
        var nameLst=new List<string>();
        var bgLst=new List<Sprite>();
        var avatarLst=new List<Sprite>();

        foreach (var v in Dialog.Datas.Values)
        {
            if(v.groupId==1)
            {
                contentLst.Add(v.text);
                nameLst.Add(Npc.Datas[v.speaker_npcId].name);
                bgLst.Add(TextureHelper.GetSpriteByPath(Img.Datas[v.background_imgId].path));
                avatarLst.Add(TextureHelper.GetSpriteByPath(Img.Datas[Npc.Datas[v.speaker_npcId].avatar_imgId].path));
            }
        }
        DialogUiBaseManager.instance.Begin(nameLst, contentLst,bgLst, avatarLst, OnComplete);
    }
    private void OnComplete()
    {
        Debug.Log("End");
    }


}
