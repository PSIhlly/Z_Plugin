using System.Collections;
using System.Collections.Generic;
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
        List<Texture2D> texList = new List<Texture2D>();
        foreach (var member in members)
        {
            texList.Add(member.texture);
        }
        TextureTransform.GetTargetSize(texList.ToArray(), 500, 500);
        var newTex = TextureCombine.FillTexture2DsToTexture2D(texList.ToArray(), 2, 2, 10);
        target.texture = newTex;
    }
}
