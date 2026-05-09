using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Z_Texture
{
    public static class TextureTransform
    {

        public static Texture2D GetTargetSize(Texture2D tex, int width, int height)
        {
                var texNew = new Texture2D(width, height);
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        texNew.SetPixel(x, y, CalculatePixel(tex, x, y, width, height));
                    }
                texNew.Apply();
                return texNew;
        }
        public static void GetTargetSize(Texture2D[] texs, int width, int height)
        {
            if (texs == null || texs.Length == 0 || texs[0] == null)
                return;

            for(int i=0;i<texs.Length;i++)
            {
                texs[i]=GetTargetSize(texs[i], width, height);
            }
        }
        public static Texture2D Copy(this Texture2D tex)
        {
            var texNew = new Texture2D(tex.width, tex.height);
            for(int y=0;y< tex.height;y++)
                for(int x=0;x< tex.width;x++)
                {
                    texNew.SetPixel(x, y, tex.GetPixel(x, y));
                }
            return texNew;
        }

        public static Texture2D RotateTextureClockwise90(Texture2D original,int times)
        {
            if (times % 4 == 0)
                return original;
            int width = original.width;
            int height = original.height;

            Texture2D rotatedTexture = new Texture2D(times%2==1?height: width, times % 2 == 1 ? width : height);

            Color32[] originalPixels = original.GetPixels32();
            Color32[] rotatedPixels = new Color32[originalPixels.Length];

            
            // 旋转像素数据
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int originalIndex = x + y * width;
                    int rotatedIndex = 0;
                    switch(times%4)
                    {
                        case 0:
                            rotatedIndex = x + y * width;
                            break;
                        case 1:
                            rotatedIndex = y + (width-1-x) * height;
                            break;
                        case 2:
                            rotatedIndex = (width - 1 - x) + (height -1- y)* width;
                            break;
                        case 3:
                            rotatedIndex = x * height + (height - 1 - y);
                            break;

                    }
                    rotatedPixels[rotatedIndex] = originalPixels[originalIndex];
                    
                }
            }

            // 应用旋转后的像素数据
            rotatedTexture.SetPixels32(rotatedPixels);
            rotatedTexture.Apply();

            return rotatedTexture;
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

        public static Texture2D FlipTexture(Texture2D original, bool flipX, bool flipY)
        {
            if (!flipX && !flipY)
                return original;

            int width = original.width;
            int height = original.height;
            Texture2D flippedTexture = new Texture2D(width, height);

            Color32[] originalPixels = original.GetPixels32();
            Color32[] flippedPixels = new Color32[originalPixels.Length];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int srcX = flipX ? width - 1 - x : x;
                    int srcY = flipY ? height - 1 - y : y;
                    int originalIndex = srcX + srcY * width;
                    int flippedIndex = x + y * width;
                    flippedPixels[flippedIndex] = originalPixels[originalIndex];
                }
            }

            flippedTexture.SetPixels32(flippedPixels);
            flippedTexture.Apply();
            return flippedTexture;
        }

    }


}