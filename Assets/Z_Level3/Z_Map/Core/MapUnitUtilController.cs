using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map.Form;
using Z_Texture;

namespace Z_Map
{
    public enum AlphaTexBasic5
    {
        OOOOXOOOO,
        OOOXXOOOO,
        OXOXXOOOO,
        OOXOOXXXX,
        XXXOOOOOO,
        XXXXOXXXX
    }
    public class MapUnitUtilController : Z_Controller<MapManager>
    {
        public Dictionary<(string, int), Texture2D> alphaTextureDic=new Dictionary<(string, int), Texture2D>();
        public Dictionary<string, List<Texture2D>> animTextureDic=new Dictionary<string, List<Texture2D>>();

        public void CreateTexAnimVariants(string name, Texture2D[] rawAnimTex)
        {
            if (rawAnimTex == null || rawAnimTex.Length == 0 || rawAnimTex[0] == null)
                return;
            animTextureDic[name] = new List<Texture2D>();
            for (int i=0;i< rawAnimTex.Length;i++)
            {
                animTextureDic[name].Add(rawAnimTex[i]);
            }
        }

        public void CreateAlphaVariantsByBasic5(string name, Texture2D[] rawAlphaTex)
        {
            if (rawAlphaTex == null || rawAlphaTex.Length == 0 || rawAlphaTex[0] == null)
                return;
            Dictionary<(AlphaTexBasic5, int), Texture2D> basicRotate = new Dictionary<(AlphaTexBasic5, int), Texture2D>();

            TextureTransform.GetTargetSize(rawAlphaTex, rawAlphaTex[0].width, rawAlphaTex[1].height);

            for (int i = 0; i < 4; i++)
            {
                basicRotate[(AlphaTexBasic5.OOOOXOOOO, i)] = TextureTransform.RotateTextureClockwise90(rawAlphaTex[(int)AlphaTexBasic5.OOOOXOOOO], i);
                basicRotate[(AlphaTexBasic5.OOOXXOOOO, i)] = TextureTransform.RotateTextureClockwise90(rawAlphaTex[(int)AlphaTexBasic5.OOOXXOOOO], i);
                basicRotate[(AlphaTexBasic5.OOXOOXXXX, i)] = TextureTransform.RotateTextureClockwise90(rawAlphaTex[(int)AlphaTexBasic5.OOXOOXXXX], i);
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, i)] = TextureTransform.RotateTextureClockwise90(rawAlphaTex[(int)AlphaTexBasic5.OXOXXOOOO], i);
                basicRotate[(AlphaTexBasic5.XXXOOOOOO, i)] = TextureTransform.RotateTextureClockwise90(rawAlphaTex[(int)AlphaTexBasic5.XXXOOOOOO], i);
            }

            int[] slash4 = new int[] { 1, 3, 7, 9 };
            int[] straight4 = new int[] { 2, 4, 6, 8 };

            alphaTextureDic[(name, 0)] = basicRotate[(AlphaTexBasic5.OOOOXOOOO, 0)];
            //1*
            foreach (var t in slash4)
            {
                alphaTextureDic[(name, (1 << t))] = basicRotate[(AlphaTexBasic5.OOOOXOOOO, 0)];
            }

            alphaTextureDic[(name, (1 << 4))] = basicRotate[(AlphaTexBasic5.OOOXXOOOO, 0)];
            alphaTextureDic[(name, (1 << 8))] = basicRotate[(AlphaTexBasic5.OOOXXOOOO, 1)];
            alphaTextureDic[(name, (1 << 6))] = basicRotate[(AlphaTexBasic5.OOOXXOOOO, 2)];
            alphaTextureDic[(name, (1 << 2))] = basicRotate[(AlphaTexBasic5.OOOXXOOOO, 3)];

            //2*

            foreach (var t in slash4)
            {
                foreach (var t2 in straight4)
                {
                    alphaTextureDic[(name, (1 << t2) | (1 << t))] = alphaTextureDic[(name, (1 << t2))];
                }
                foreach (var t2 in slash4)
                {
                    if (t != t2)
                    {
                        alphaTextureDic[(name, (1 << t2) | (1 << t))] = alphaTextureDic[(name, 0)];
                    }
                }
            }


            alphaTextureDic[(name, (1 << 4) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                basicRotate[(AlphaTexBasic5.XXXOOOOOO,0)],
                basicRotate[(AlphaTexBasic5.XXXOOOOOO, 2)]
            });

            alphaTextureDic[(name, (1 << 2) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                basicRotate[(AlphaTexBasic5.XXXOOOOOO, 1)],
                basicRotate[(AlphaTexBasic5.XXXOOOOOO, 3)]
            });


            alphaTextureDic[(name, (1 << 4) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                basicRotate[(AlphaTexBasic5.OXOXXOOOO,0)],
                basicRotate[(AlphaTexBasic5.OOXOOXXXX,0)]
            });
            alphaTextureDic[(name, (1 << 8) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, 1)],
                basicRotate[(AlphaTexBasic5.OOXOOXXXX, 1)]
            });
            alphaTextureDic[(name, (1 << 6) | (1 << 2))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, 2)],
                basicRotate[(AlphaTexBasic5.OOXOOXXXX, 2)]
            });
            alphaTextureDic[(name, (1 << 2) | (1 << 4))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, 3)],
                basicRotate[(AlphaTexBasic5.OOXOOXXXX, 3)]
            });

            //3*
            int cur = 0;
            foreach (var t1 in slash4)
            {
                cur ^= (1 << t1);
                foreach (var t2 in slash4)
                {
                    if ((cur & (1 << t2)) > 0)
                        continue;
                    cur ^= (1 << t2);
                    foreach (var t3 in slash4)
                    {
                        if (t2 == t3 || t1 == t3)
                            continue;
                        cur ^= (1 << t3);
                        alphaTextureDic[(name, cur)] = alphaTextureDic[(name, 0)];
                        cur ^= (1 << t3);
                    }
                    foreach (var t3 in straight4)
                    {
                        cur ^= (1 << t3);
                        alphaTextureDic[(name, cur)] = alphaTextureDic[(name, 1 << t3)];
                        cur ^= (1 << t3);
                    }
                    cur ^= (1 << t2);
                }
                alphaTextureDic[(name, cur | (1 << 4) | (1 << 6))] = alphaTextureDic[(name, (1 << 4) | (1 << 6))];
                alphaTextureDic[(name, cur | (1 << 2) | (1 << 8))] = alphaTextureDic[(name, (1 << 2) | (1 << 8))];

                if (t1 == 7)
                {
                    alphaTextureDic[(name, cur | (1 << 4) | (1 << 8))] = rawAlphaTex[(int)AlphaTexBasic5.OOXOOXXXX];
                }
                else
                {
                    alphaTextureDic[(name, cur | (1 << 4) | (1 << 8))] = alphaTextureDic[(name, (1 << 4) | (1 << 8))];
                }
                if (t1 == 9)
                {
                    alphaTextureDic[(name, cur | (1 << 8) | (1 << 6))] = basicRotate[(AlphaTexBasic5.OOXOOXXXX, 1)];
                }
                else
                {
                    alphaTextureDic[(name, cur | (1 << 8) | (1 << 6))] = alphaTextureDic[(name, (1 << 8) | (1 << 6))];
                }
                if (t1 == 3)
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 6))] = basicRotate[(AlphaTexBasic5.OOXOOXXXX, 2)];
                }
                else
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 6))] = alphaTextureDic[(name, (1 << 2) | (1 << 6))];
                }
                if (t1 == 1)
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 4))] = basicRotate[(AlphaTexBasic5.OOXOOXXXX, 3)];
                }
                else
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 4))] = alphaTextureDic[(name, (1 << 2) | (1 << 4))];
                }

                cur ^= (1 << t1);
            }

            alphaTextureDic[(name, (1 << 4) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                basicRotate[(AlphaTexBasic5.OXOXXOOOO,0)],
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, 1)],
                basicRotate[(AlphaTexBasic5.XXXOOOOOO, 2)]
            });
            alphaTextureDic[(name, (1 << 6) | (1 << 8) | (1 << 2))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, 1)],
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, 2)],
                basicRotate[(AlphaTexBasic5.XXXOOOOOO, 3)]
            });
            alphaTextureDic[(name, (1 << 2) | (1 << 4) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, 2)],
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, 3)],
                basicRotate[(AlphaTexBasic5.XXXOOOOOO,0)]
            });
            alphaTextureDic[(name, (1 << 2) | (1 << 4) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                basicRotate[(AlphaTexBasic5.OXOXXOOOO,0)],
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, 3)],
                basicRotate[(AlphaTexBasic5.XXXOOOOOO, 1)]
            });

            //4*
            alphaTextureDic[(name, (1 << 1) | (1 << 3) | (1 << 7) | (1 << 9))] = alphaTextureDic[(name, 0)];

            foreach (var t1 in slash4)
            {
                cur ^= (1 << t1);
                foreach (var t2 in slash4)
                {
                    if ((cur & (1 << t2)) > 0)
                        continue;
                    cur ^= (1 << t2);
                    foreach (var t3 in slash4)
                    {
                        if ((cur & (1 << t3)) > 0)
                            continue;
                        cur ^= (1 << t3);
                        foreach (var t4 in straight4)
                        {
                            cur ^= (1 << t4);
                            alphaTextureDic[(name, cur)] = alphaTextureDic[(name, 1 << t4)];
                            cur ^= (1 << t4);
                        }
                        cur ^= (1 << t3);
                    }
                    alphaTextureDic[(name, cur | (1 << 4) | (1 << 6))] = alphaTextureDic[(name, (1 << 4) | (1 << 6))];
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 8))] = alphaTextureDic[(name, (1 << 2) | (1 << 8))];

                    if ((cur & (1 << 7)) != 0)
                    {
                        alphaTextureDic[(name, cur | (1 << 4) | (1 << 8))] = alphaTextureDic[(name, (1 << 4) | (1 << 7) | (1 << 8))];
                    }
                    else
                    {
                        alphaTextureDic[(name, cur | (1 << 4) | (1 << 8))] = alphaTextureDic[(name, (1 << 4) | (1 << 8))];
                    }

                    if ((cur & (1 << 9)) != 0)
                    {
                        alphaTextureDic[(name, cur | (1 << 6) | (1 << 8))] = alphaTextureDic[(name, (1 << 6) | (1 << 8) | (1 << 9))];
                    }
                    else
                    {
                        alphaTextureDic[(name, cur | (1 << 6) | (1 << 8))] = alphaTextureDic[(name, (1 << 6) | (1 << 8))];
                    }
                    if ((cur & (1 << 3)) != 0)
                    {
                        alphaTextureDic[(name, cur | (1 << 2) | (1 << 6))] = alphaTextureDic[(name, (1 << 2) | (1 << 3) | (1 << 6))];
                    }
                    else
                    {
                        alphaTextureDic[(name, cur | (1 << 2) | (1 << 6))] = alphaTextureDic[(name, (1 << 2) | (1 << 6))];
                    }
                    if ((cur & (1 << 1)) != 0)
                    {
                        alphaTextureDic[(name, cur | (1 << 2) | (1 << 4))] = alphaTextureDic[(name, (1 << 1) | (1 << 2) | (1 << 4))];
                    }
                    else
                    {
                        alphaTextureDic[(name, cur | (1 << 2) | (1 << 4))] = alphaTextureDic[(name, (1 << 2) | (1 << 4))];
                    }

                    cur ^= (1 << t2);
                }


                if ((cur & (1 << 7)) > 0)
                {
                    alphaTextureDic[(name, cur | (1 << 4) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 1)],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 2)]
                    });
                }
                else if ((cur & (1 << 9)) > 0)
                {
                    alphaTextureDic[(name, cur | (1 << 4) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO,0)],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 2)]
                    });
                }
                else
                {
                    alphaTextureDic[(name, cur | (1 << 4) | (1 << 6) | (1 << 8))] = alphaTextureDic[(name, (1 << 4) | (1 << 6) | (1 << 8))];
                }

                if ((cur & (1 << 9)) > 0)
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 2)],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 3)]
                    });
                }
                else if ((cur & (1 << 3)) > 0)
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 1)],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 3)]
                    });
                }
                else
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 6) | (1 << 8))] = alphaTextureDic[(name, (1 << 2) | (1 << 6) | (1 << 8))];
                }

                if ((cur & (1 << 1)) > 0)
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 2)],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO,0)]
                    });
                }
                else if ((cur & (1 << 3)) > 0)
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 3)],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO,0)]
                    });
                }
                else
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 6))] = alphaTextureDic[(name, (1 << 2) | (1 << 4) | (1 << 6))];
                }

                if ((cur & (1 << 1)) > 0)
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO,0)],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 1)]
                    });
                }
                else if ((cur & (1 << 7)) > 0)
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 3)],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 1)]
                    });
                }
                else
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 8))] = alphaTextureDic[(name, (1 << 2) | (1 << 4) | (1 << 8))];
                }
                cur ^= (1 << t1);
            }
            cur = (1 << 2) | (1 << 4) | (1 << 6) | (1 << 8);
            alphaTextureDic[(name, cur)] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                basicRotate[(AlphaTexBasic5.OXOXXOOOO,0)],
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, 1)],
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, 2)],
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, 3)]
            });
            //5*
            alphaTextureDic[(name, cur|(1 << 1) )] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
               basicRotate[(AlphaTexBasic5.OXOXXOOOO,0)],
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, 1)],
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, 2)]
            });
            alphaTextureDic[(name, cur | (1 << 3) )] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
               basicRotate[(AlphaTexBasic5.OXOXXOOOO,0)],
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, 1)],
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, 3)]
            });
            alphaTextureDic[(name, cur | (1 << 7) )] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
               basicRotate[(AlphaTexBasic5.OXOXXOOOO,1)],
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, 2)],
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, 3)]
            });
            alphaTextureDic[(name, cur | (1 << 9) )] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
               basicRotate[(AlphaTexBasic5.OXOXXOOOO, 0 )],
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, 2)],
                basicRotate[(AlphaTexBasic5.OXOXXOOOO, 3)]
            });
            cur = 0;
            foreach (var t1 in slash4)
            {
                cur ^= (1 << t1);
                foreach (var t2 in slash4)
                {
                    if ((cur & (1 << t2)) > 0)
                        continue;
                    cur ^= (1 << t2);
                    foreach (var t3 in slash4)
                    {
                        if ((cur & (1 << t3)) > 0)
                            continue;
                        cur ^= (1 << t3);
                            alphaTextureDic[(name, cur | (1 << 4) | (1 << 6))] = alphaTextureDic[(name,(1<<4)|(1<<6))];
                        alphaTextureDic[(name, cur | (1 << 2) | (1 << 8))] = alphaTextureDic[(name,(1<<2)|(1<<8))];

                        if ((cur & (1 << 7)) > 0)
                        {
                        alphaTextureDic[(name, cur | (1 << 4) | (1 << 8))] = alphaTextureDic[(name,(1<<4) | (1 <<7) | (1 << 8))];

                        }
                        else
                        {
                            alphaTextureDic[(name, cur | (1 << 4) | (1 << 8))] = alphaTextureDic[(name, (1 << 4) | (1 << 8))];
                        }
                        if ((cur & (1 << 9)) > 0)
                        {
                            alphaTextureDic[(name, cur | (1 << 6) | (1 << 8))] = alphaTextureDic[(name, (1 << 6) | (1 << 8) | (1 << 9))];

                        }
                        else
                        {
                            alphaTextureDic[(name, cur | (1 << 6) | (1 << 8))] = alphaTextureDic[(name, (1 << 6) | (1 << 8))];
                        }

                        if ((cur & (1 << 3)) > 0)
                        {
                            alphaTextureDic[(name, cur | (1 << 2) | (1 << 6))] = alphaTextureDic[(name, (1 << 2) | (1 << 3) | (1 << 6))];

                        }
                        else
                        {
                            alphaTextureDic[(name, cur | (1 << 2) | (1 << 6))] = alphaTextureDic[(name, (1 << 2) | (1 << 6))];
                        }

                        if ((cur & (1 << 1)) > 0)
                        {
                            alphaTextureDic[(name, cur | (1 << 2) | (1 << 4))] = alphaTextureDic[(name, (1 << 1) | (1 << 2) | (1 << 4))];

                        }
                        else
                        {
                            alphaTextureDic[(name, cur | (1 << 2) | (1 << 4))] = alphaTextureDic[(name, (1 << 2) | (1 << 4))];
                        }
                        cur ^= (1 << t3);
                    }

                        if ((cur & ((1 << 7) | (1 << 9))) > 0)
                    {
                        alphaTextureDic[(name, cur | (1 << 4) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 2 )]
                    });
                    }
                    else if ((cur & (1 << 7)) > 0)
                    {
                        alphaTextureDic[(name, cur | (1 << 4) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 1 )],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 2 )]     
                        });
                    }
                    else if ((cur & (1 << 9)) > 0)
                    {
                        alphaTextureDic[(name, cur | (1 << 4) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 0 )],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 2 )]
                        });
                    }else
                    {
                        alphaTextureDic[(name, cur | (1 << 4) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 0 )],
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 1 )],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 2 )]
                        });
                    }


                    if ((cur & ((1 << 3) | (1 << 9))) > 0)
                    {
                        alphaTextureDic[(name, cur | (1 << 2) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 3 )]
                    });
                    }
                    else if ((cur & (1 << 3)) > 0)
                    {
                        alphaTextureDic[(name, cur | (1 << 2) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 1 )],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 3 )]
                        });
                    }
                    else if ((cur & (1 << 9)) > 0)
                    {
                        alphaTextureDic[(name, cur | (1 << 2) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 2 )],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 3 )]
                        });
                    }
                    else
                    {
                        alphaTextureDic[(name, cur | (1 << 2) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 1 )],
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 2 )],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 3 )]
                        });
                    }

                    if ((cur & ((1 << 3) | (1 << 1))) > 0)
                    {
                        alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 0 )]
                    });
                    }
                    else if ((cur & (1 << 3)) > 0)
                    {
                        alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 3 )],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 0 )]
                        });
                    }
                    else if ((cur & (1 << 1)) > 0)
                    {
                        alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 2 )],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 0 )]
                        });
                    }
                    else
                    {
                        alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 2 )],
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 3 )],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 0 )]
                        });
                    }

                    if ((cur & ((1 << 7) | (1 << 1))) > 0)
                    {
                        alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 1 )]
                    });
                    }
                    else if ((cur & (1 << 7)) > 0)
                    {
                        alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 3 )],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 1 )]
                        });
                    }
                    else if ((cur & (1 << 1)) > 0)
                    {
                        alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 0 )],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 1 )]
                        });
                    }
                    else
                    {
                        alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 0 )],
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 3 )],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 1 )]
                        });
                    }

                    cur ^= (1 << t2);
                }

                cur ^= (1 << t1);
            }

            foreach (var t in straight4)
            {
                alphaTextureDic[(name, (1 << 1) | (1 << 3) | (1 << 7) | (1 << 9) | (1 << t))] = alphaTextureDic[(name, (1 << t))];
            }

            //6* 

            cur = (1 << 1) | (1 << 3) | (1 << 7) | (1 << 9);
            alphaTextureDic[(name, cur  | (1 << 4) | (1 << 8))] = alphaTextureDic[(name, (1 << 4)|(1 << 7) | (1 << 8))];
            alphaTextureDic[(name, cur  | (1 << 6) | (1 << 8))] = alphaTextureDic[(name, (1 << 6)|(1 << 9) | (1 << 9))];
            alphaTextureDic[(name, cur  | (1 << 2) | (1 << 6))] = alphaTextureDic[(name, (1 << 2)|(1 << 3) | (1 << 6))];
            alphaTextureDic[(name, cur  | (1 << 2) | (1 << 4))] = alphaTextureDic[(name, (1 << 1)|(1 << 2) | (1 << 4))];

            alphaTextureDic[(name, cur  | (1 << 2) | (1 << 8))] = alphaTextureDic[(name, (1 << 2) | (1 << 8))];
            alphaTextureDic[(name, cur  | (1 << 4) | (1 << 6))] = alphaTextureDic[(name, (1 << 4) | (1 << 6))];

            alphaTextureDic[(name, (1 << 1) | (1 << 7) | (1 << 9) | (1 << 4) | (1 << 6) | (1 << 8))] = basicRotate[(AlphaTexBasic5.XXXOOOOOO, 2)];
            alphaTextureDic[(name, (1 << 3) | (1 << 7) | (1 << 9) | (1 << 4) | (1 << 6) | (1 << 8))] = basicRotate[(AlphaTexBasic5.XXXOOOOOO, 2)];
            alphaTextureDic[(name, (1 << 1) | (1 << 3) | (1 << 7) | (1 << 4) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 1 )],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 2 )]
                        });
            alphaTextureDic[(name, (1 << 1) | (1 << 3) | (1 << 9) | (1 << 4) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 0 )],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 2 )]
                        });


            alphaTextureDic[(name, (1 << 1) | (1 << 3) | (1 << 9) | (1 << 2) | (1 << 6) | (1 << 8))] = basicRotate[(AlphaTexBasic5.XXXOOOOOO, 3)];
            alphaTextureDic[(name, (1 << 3) | (1 << 7) | (1 << 9) | (1 << 2) | (1 << 6) | (1 << 8))] = basicRotate[(AlphaTexBasic5.XXXOOOOOO, 3)];
            alphaTextureDic[(name, (1 << 1) | (1 << 3) | (1 << 7) | (1 << 2) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 1 )],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 3 )]
                        });
            alphaTextureDic[(name, (1 << 1) | (1 << 7) | (1 << 9) | (1 << 2) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 2 )],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 3 )]
                        });


            alphaTextureDic[(name, (1 << 1) | (1 << 3) | (1 << 7) | (1 << 2) | (1 << 4) | (1 << 6))] = basicRotate[(AlphaTexBasic5.XXXOOOOOO, 0)];
            alphaTextureDic[(name, (1 << 1) | (1 << 3) | (1 << 9) | (1 << 2) | (1 << 4) | (1 << 6))] = basicRotate[(AlphaTexBasic5.XXXOOOOOO, 0)];
            alphaTextureDic[(name, (1 << 1) | (1 << 7) | (1 << 9) | (1 << 2) | (1 << 4) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 2 )],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 0 )]
                        });
            alphaTextureDic[(name, (1 << 3) | (1 << 7) | (1 << 9) | (1 << 2) | (1 << 4) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 3 )],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 0 )]
                        });

            alphaTextureDic[(name,(1 << 1) | (1 << 7) | (1 << 9) | (1 << 2) | (1 << 4) | (1 << 8))] = basicRotate[(AlphaTexBasic5.XXXOOOOOO, 1)];
            alphaTextureDic[(name, (1 << 1) | (1 << 3) | (1 << 7) | (1 << 2) | (1 << 4) | (1 << 8))] = basicRotate[(AlphaTexBasic5.XXXOOOOOO, 1)];
            alphaTextureDic[(name, (1 << 1) | (1 << 3) | (1 << 9) | (1 << 2) | (1 << 4) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 0 )],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 1 )]
                        });
            alphaTextureDic[(name, (1 << 3) | (1 << 7) | (1 << 9) | (1 << 2) | (1 << 4) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 3 )],
                        basicRotate[(AlphaTexBasic5.XXXOOOOOO, 1 )]
                        });


            cur = (1 << 2) | (1 << 4) | (1 << 6) | (1 << 8);
            alphaTextureDic[(name, cur | (1 << 1) | (1 << 9))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 0 )],
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 2 )]
                        });
            alphaTextureDic[(name, cur | (1 << 3) | (1 << 7) )] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 1 )],
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 3 )]
                        });
            alphaTextureDic[(name, cur | (1 << 1) | (1 << 7) )] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 1 )],
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 2 )]
                        });
            alphaTextureDic[(name, cur | (1 << 7) | (1 << 9))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 2 )],
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 3 )]
                        });
            alphaTextureDic[(name, cur | (1 << 3) | (1 << 9))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 0 )],
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 3 )]
                        });
            alphaTextureDic[(name, cur | (1 << 1) | (1 << 3))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 0 )],
                        basicRotate[(AlphaTexBasic5.OXOXXOOOO, 1 )]
                        });

            //7*
            cur = (1 << 2) | (1 << 4) | (1 << 6) | (1 << 8);
            alphaTextureDic[(name, cur | (1 << 1) | (1 << 3) | (1 << 7) )] = basicRotate[(AlphaTexBasic5.OXOXXOOOO, 1)];
            alphaTextureDic[(name, cur | (1 << 1) | (1 << 7) | (1 << 9) )] = basicRotate[(AlphaTexBasic5.OXOXXOOOO, 2)];
            alphaTextureDic[(name, cur | (1 << 3) | (1 << 7) | (1 << 9) )] = basicRotate[(AlphaTexBasic5.OXOXXOOOO, 3)];
            alphaTextureDic[(name, cur | (1 << 1) | (1 << 3) | (1 << 9) )] = basicRotate[(AlphaTexBasic5.OXOXXOOOO, 0)];

            cur = (1 << 1) | (1 << 3) | (1 << 7) | (1 << 9);
            alphaTextureDic[(name, cur | (1 << 4) | (1 << 6) | (1 << 8))] = basicRotate[(AlphaTexBasic5.XXXOOOOOO, 2)];
            alphaTextureDic[(name, cur  | (1 << 2) | (1 << 6) | (1 << 8))] = basicRotate[(AlphaTexBasic5.XXXOOOOOO, 3)];
            alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 6))] = basicRotate[(AlphaTexBasic5.XXXOOOOOO, 0)];
            alphaTextureDic[(name, cur  | (1 << 2) | (1 << 4) | (1 << 8))] = basicRotate[(AlphaTexBasic5.XXXOOOOOO, 1)];

            //8*
            alphaTextureDic[(name, (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 6) | (1 << 7) | (1 << 8) | (1 << 9))] = rawAlphaTex[(int)AlphaTexBasic5.XXXXOXXXX];

        }
        public void ShowFinalMat(MapInstance ins)
        {
            var data = ins.unit.data;
            for (int i = 0; i < ins.renderers.Length; i++)
            {

                if (data.texNameDic.ContainsKey(i))
                {
                    MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                    ins.renderers[i].GetPropertyBlock(propBlock);

                    propBlock.SetTexture("_Tex", TexAssetForm.DataByName[ModAssetManager.instance.GetTexRealName(data.texNameDic[i],0)].tex);

                    if (data.alphaTexNameDic.ContainsKey(i))
                    {
                        int linkDesc = 0;
                        for (int x = -1; x <= 1; x++)
                        {
                            for (int z = -1; z <= 1; z++)
                            {
                                if (x == 0 && z == 0)
                                    continue;
                                var pos = (x + data.mapPos.x, data.mapPos.y, z + data.mapPos.z);
                                if (_super.dataCtrl.maps.ContainsKey(pos)
                                    && _super.dataCtrl.maps[pos].texNameDic.ContainsKey(i)
                                    && _super.dataCtrl.maps[pos].texNameDic[i] == data.texNameDic[i])
                                {
                                    linkDesc |= 1 << ((z + 1) * 3 + (x + 2));
                                }
                            }
                        }
                        propBlock.SetTexture("_AlphaTex", alphaTextureDic[(data.alphaTexNameDic[i], linkDesc)]);
                    }else
                    {
                        propBlock.SetTexture("_AlphaTex", Texture2D.blackTexture);
                    }

                    ins.renderers[i].SetPropertyBlock(propBlock);
                }
            }
        }
        public void UpdateAnim(MapInstance ins)
        {
            
            var data = ins.unit.data;
            for (int i = 0; i < ins.renderers.Length; i++)
            {
                if (data.texNameDic.ContainsKey(i)&&data.animInterval[i]>0)
                {
                    int all = data.animInterval[i] * animTextureDic[data.texNameDic[i]].Count;

                    int cur=(Time.frameCount % all)/ data.animInterval[i];
                    if (all==0||(ins.animCur.ContainsKey(i) && ins.animCur[i] == cur))
                        continue;
                    ins.animCur[i] = cur;
                    MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                    ins.renderers[i].GetPropertyBlock(propBlock);
                    propBlock.SetTexture("_Tex", animTextureDic[data.texNameDic[i]][cur]);
                    ins.renderers[i].SetPropertyBlock(propBlock);
                }
            }  
        }



    }
}
