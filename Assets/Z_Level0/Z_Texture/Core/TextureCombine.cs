using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_Texture
{
public class TextureCombine
{
    //(0,0)is left down
    public static Texture2D CombineTexture2DsToTexture2D(Texture2D[] tex,int columns,int rows,int compressScale=1)
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
   
}


}