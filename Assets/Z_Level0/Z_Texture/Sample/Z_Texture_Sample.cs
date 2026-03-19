using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Z_Texture;
public class Z_Texture_Sample :MonoBehaviour
{
    public Sprite[] members;
    public RawImage target;

    // Update is called once per frame
    void Start()
    {
        /*List<Texture2D> texList = new List<Texture2D>();
        foreach (var member in members)
        {
            texList.Add(member.texture);
        }
        TextureTransform.GetTargetSize(texList.ToArray(), 50, 50);
        var newTex = TextureCombine.FillTexture2DsToTexture2D(texList.ToArray(), 2, 2, 10);
        target.texture = newTex;
        File.WriteAllBytes(Application.dataPath+"/test.png",TextureHelper.GetTextureByte(TextureTransform.GetTargetSize(texList[0],300,100)));*/
        var bytes=TextureHelper.GetPNGWithExtraInfo(members[0].texture,new byte[] {1,2,3,4,5,6 });
        File.WriteAllBytes("D:\\jiba.png", bytes);
        foreach(var b in TextureHelper.GetExtraInfoByPNG(File.ReadAllBytes("D:\\jiba.png")))
        {
            Debug.Log(b);
        }
    }
}
