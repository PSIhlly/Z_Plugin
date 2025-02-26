using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_Texture
{
public class TextureCombine
{
    //(0,0)is left down
    public static Texture2D FillTexture2DsToTexture2D(Texture2D[] tex,int columns,int rows,int compressScale=1)
    {
        int unitWidthNew = tex[0].width / compressScale + (tex[0].width % compressScale == 0 ? 0 : 1);
        int unitHeightNew = tex[0].height / compressScale + (tex[0].height % compressScale == 0 ? 0 : 1);
        Texture2D resTex = new Texture2D(unitWidthNew * columns, unitHeightNew * rows);
            
        for (int i=0;i< rows; i++)
        {
            for(int j=0;j<columns;j++)
            {
                int id = i * columns + j;
                    int x=0, y=0;
                for (y = 0; y* compressScale < tex[id].height; y++)
                    for (x= 0; x* compressScale < tex[id].width;x++)
                {
                        resTex.SetPixel(unitWidthNew*j+x, unitHeightNew * i+y, tex[id].GetPixel(x * compressScale, y * compressScale));
                            
                }
                }
        }
            resTex.Apply();
           
        return resTex;
    }
        public static Texture2D OverlayTexture2DsToTexture2DByMinR(Texture2D[] texs)
        {
            if (texs == null || texs.Length == 0 || texs[0] == null)
                return null;
            int width = texs[0].width;
            int height = texs[0].height;
            for(int i=0;i<texs.Length;i++)
            {
                if(texs[i].width!=width||texs[i].height!=height)
                {
                    TextureTransform.GetTargetSize(texs,width, height);
                    break;
                }
            }
            Texture2D newTex= new Texture2D(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float max = 0;
                    for(int i=0;i<texs.Length;i++)
                    {
                        if(texs[i].GetPixel(x, y).r>0.1f)
                        max = Mathf.Max(texs[i].GetPixel(x,y).r, max);
                    }
                    newTex.SetPixel(x,y, new Color(max, 0,0));
                }
            }
            newTex.Apply();

            return newTex;
        }

    }


}