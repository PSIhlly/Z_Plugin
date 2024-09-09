using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_Texture
{
    public class TextureTransform
    {
       
        public static Texture2D GetTargetSize(Texture2D tex, int width, int height)
        {
            var texNew = new Texture2D(width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    texNew.SetPixel(x, y,CalculatePixel(tex,x,y,width,height));
                }
            texNew.Apply();
            return texNew;
        }
        public static Texture2D Copy(Texture2D tex)
        {
            var texNew = new Texture2D(tex.width, tex.height);
            for(int y=0;y< tex.height;y++)
                for(int x=0;x< tex.width;x++)
                {
                    texNew.SetPixel(x, y, tex.GetPixel(x, y));
                }
            return texNew;
        }
        public static void DebugLogColorCount(Texture2D tex)
        {
            Dictionary<Color, bool> zhonglei = new Dictionary<Color, bool>();
            int count = 0;
            for (int y=0;y< tex.height;y++)
                for (int x = 0; x < tex.width; x++)
                {
                    count += zhonglei.ContainsKey(tex.GetPixel(x, y)) ? 0 : 1;
                    zhonglei[tex.GetPixel(x, y)] = true;

                }
            Debug.Log(count);
        }
        public static void DebugLogFirstRowColor(Texture2D tex)
        {
            for (int x = 0; x < tex.width; x++)
                Debug.Log(x+" "+ tex.GetPixel(x,0));
        }
       public static Color CalculatePixel(Texture2D tex,int targetX,int targetY,int width,int height)
        {
            Vector2 targetPos = new Vector2(1f * targetX / width * tex.width, 1f * targetY / height * tex.height);
           
            return tex.GetPixel((int)targetPos.x, (int)targetPos.y);

        }

    }


}