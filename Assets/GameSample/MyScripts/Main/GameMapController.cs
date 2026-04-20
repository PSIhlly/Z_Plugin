using Form;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.Rendering;
using Z_ByteSerialize;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Texture;
using Z_Time;
using Z_UnitSystem.Form;
using static UnityEngine.Rendering.DebugUI;



namespace Z_Map
{
    public partial class MapUnit
    {
        public static string productKey = "pdt";
        private (int, int) _productInfo;
        public (int, int) productInfo
        {
            get
            {
                if (_productInfo == default)
                {
                    _productInfo = (0, 0);
                    if (!string.IsNullOrEmpty(data.extra))
                    {
                        var jo = JObject.Parse(data.extra);
                        if (jo != null && jo[productKey].Type!= JTokenType.Null)
                        {
                            _productInfo.Item1 = (int)jo[productKey][0];
                            _productInfo.Item2 = (int)jo[productKey][1];
                        }
                    }
                }
                return _productInfo;
            }
            set
            {
                _productInfo = value;
            }

        }
        public static string GetProductInfoString(JObject ori, (int, int) info)
        {
            var ja = new JArray();
            ja.Add(info.Item1);
            ja.Add(info.Item2);
            ori[productKey] = ja;
            return ori.ToString();
        }

        public static string paramKey = "prm";
        private Dictionary<string, GameParamForm.Data> _paramInfo;
        public Dictionary<string, GameParamForm.Data> paramInfo
        {
            get
            {
                if (_paramInfo == default)
                {
                    _paramInfo = new Dictionary<string, GameParamForm.Data>();
                    if (!string.IsNullOrEmpty(data.extra))
                    {
                        var jo = JObject.Parse(data.extra);
                        if (jo != null && jo[paramKey] != null)
                        {
                            _paramInfo = jo.Get<Dictionary<string, GameParamForm.Data>>(paramKey);
                        }
                    }
                    if(this is CharacterUnit ch)
                    {
                        var chp=CharacterProductForm.DataByUid.GetDv(ch.productInfo.Item1, null);
                        if(chp!=null)
                        {
                            foreach(var pair in chp.paramDic)
                            {
                                if(!_paramInfo.ContainsKey(pair.Key))
                                {
                                    _paramInfo[pair.Key] = pair.Value;
                                }
                            }
                        }
                    }
                    else if(this is ObjectUnit obj)
                    {
                        var objp = CharacterProductForm.DataByUid.GetDv(obj.productInfo.Item1, null);
                        if (objp != null)
                        {
                            foreach (var pair in objp.paramDic)
                            {
                                if (!_paramInfo.ContainsKey(pair.Key))
                                {
                                    _paramInfo[pair.Key] = pair.Value;
                                }
                            }
                        }
                    }
                }
                return _paramInfo;
            }
            set
            {
                _paramInfo = value;
            }
        }
        public static string GetParamInfoString(JObject ori, Dictionary<string, GameParamForm.Data> info)
        {
            if(info!=null)
            {
                ori.Set(paramKey, info);
            }
            return ori.ToString();
        }
    }
}

//����Ϊ0
public enum AlphaTexBasic6
{
    OOOOXOOOO,
    OOOXXOOOO,
    XXXXXXOXX,
    OOOXXOXXO,
    OOOXXXXXX,
    XXXXXXXXX
}
public class GameMapController : Z_Controller<GameManager>, IZ_Listener<TileEvent>
{
    public GameMapController(GameManager super) : base(super)
    {
        Z_EventHelper.Register(this);
        ProgressForm.changeCameramodeAction += (data,old,now) =>
        {
            DynamicGlobalSettings.cameraMode = now;
        };

        UnitForm.beforeGetAction += (data) =>
        {
            if (data.unit is MapUnit mapU)
            {
                var jo = string.IsNullOrEmpty(data.extra) ? new JObject() : JObject.Parse(data.extra);

                MapUnit.GetProductInfoString(jo, mapU.productInfo);

               MapUnit.GetParamInfoString(jo, mapU.paramInfo);
                data.extra = jo.ToString();
            }

        };
    }

    public Dictionary<(string, int), Texture2D> alphaTextureDic = new Dictionary<(string, int), Texture2D>();
    public Dictionary<TileUnitForm.Data, Dictionary<int, int>> animCurCache = new Dictionary<TileUnitForm.Data, Dictionary<int, int>>();
    public void Reset()
    {
        alphaTextureDic.Clear();
        animCurCache.Clear();
    }

    public void CreateAlphaVariantsByBasic6(string name, Texture2D[] rawAlphaTex)
    {
        if (rawAlphaTex == null || rawAlphaTex.Length == 0 || rawAlphaTex[0] == null)
            return;
        Dictionary<(AlphaTexBasic6, int), Texture2D> basicRotate = new Dictionary<(AlphaTexBasic6, int), Texture2D>();

        TextureTransform.GetTargetSize(rawAlphaTex, rawAlphaTex[0].width, rawAlphaTex[1].height);

        for (int i = 0; i < 4; i++)
        {
            basicRotate[(AlphaTexBasic6.OOOOXOOOO, i)] = TextureTransform.RotateTextureClockwise90(rawAlphaTex[(int)AlphaTexBasic6.OOOOXOOOO], i);
            basicRotate[(AlphaTexBasic6.OOOXXOOOO, i)] = TextureTransform.RotateTextureClockwise90(rawAlphaTex[(int)AlphaTexBasic6.OOOXXOOOO], i);
            basicRotate[(AlphaTexBasic6.OOOXXOXXO, i)] = TextureTransform.RotateTextureClockwise90(rawAlphaTex[(int)AlphaTexBasic6.OOOXXOXXO], i);
            basicRotate[(AlphaTexBasic6.XXXXXXOXX, i)] = TextureTransform.RotateTextureClockwise90(rawAlphaTex[(int)AlphaTexBasic6.XXXXXXOXX], i);
            basicRotate[(AlphaTexBasic6.OOOXXXXXX, i)] = TextureTransform.RotateTextureClockwise90(rawAlphaTex[(int)AlphaTexBasic6.OOOXXXXXX], i);
        }

        int[] slash4 = new int[] { 1, 3, 7, 9 };
        int[] straight4 = new int[] { 2, 4, 6, 8 };

        alphaTextureDic[(name, 0)] = basicRotate[(AlphaTexBasic6.OOOOXOOOO, 0)];
        //1*
        foreach (var t in slash4)
        {
            alphaTextureDic[(name, (1 << t))] = basicRotate[(AlphaTexBasic6.OOOOXOOOO, 0)];
        }

        alphaTextureDic[(name, (1 << 4))] = basicRotate[(AlphaTexBasic6.OOOXXOOOO, 0)];
        alphaTextureDic[(name, (1 << 8))] = basicRotate[(AlphaTexBasic6.OOOXXOOOO, 1)];
        alphaTextureDic[(name, (1 << 6))] = basicRotate[(AlphaTexBasic6.OOOXXOOOO, 2)];
        alphaTextureDic[(name, (1 << 2))] = basicRotate[(AlphaTexBasic6.OOOXXOOOO, 3)];

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
                basicRotate[(AlphaTexBasic6.OOOXXXXXX,0)],
                basicRotate[(AlphaTexBasic6.OOOXXXXXX, 2)]
            });

        alphaTextureDic[(name, (1 << 2) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                basicRotate[(AlphaTexBasic6.OOOXXXXXX, 1)],
                basicRotate[(AlphaTexBasic6.OOOXXXXXX, 3)]
            });


        alphaTextureDic[(name, (1 << 4) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                basicRotate[(AlphaTexBasic6.XXXXXXOXX,0)],
                basicRotate[(AlphaTexBasic6.OOOXXOXXO,0)]
            });
        alphaTextureDic[(name, (1 << 8) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                basicRotate[(AlphaTexBasic6.XXXXXXOXX, 1)],
                basicRotate[(AlphaTexBasic6.OOOXXOXXO, 1)]
            });
        alphaTextureDic[(name, (1 << 6) | (1 << 2))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                basicRotate[(AlphaTexBasic6.XXXXXXOXX, 2)],
                basicRotate[(AlphaTexBasic6.OOOXXOXXO, 2)]
            });
        alphaTextureDic[(name, (1 << 2) | (1 << 4))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                basicRotate[(AlphaTexBasic6.XXXXXXOXX, 3)],
                basicRotate[(AlphaTexBasic6.OOOXXOXXO, 3)]
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
                alphaTextureDic[(name, cur | (1 << 4) | (1 << 8))] = rawAlphaTex[(int)AlphaTexBasic6.OOOXXOXXO];
            }
            else
            {
                alphaTextureDic[(name, cur | (1 << 4) | (1 << 8))] = alphaTextureDic[(name, (1 << 4) | (1 << 8))];
            }
            if (t1 == 9)
            {
                alphaTextureDic[(name, cur | (1 << 8) | (1 << 6))] = basicRotate[(AlphaTexBasic6.OOOXXOXXO, 1)];
            }
            else
            {
                alphaTextureDic[(name, cur | (1 << 8) | (1 << 6))] = alphaTextureDic[(name, (1 << 8) | (1 << 6))];
            }
            if (t1 == 3)
            {
                alphaTextureDic[(name, cur | (1 << 2) | (1 << 6))] = basicRotate[(AlphaTexBasic6.OOOXXOXXO, 2)];
            }
            else
            {
                alphaTextureDic[(name, cur | (1 << 2) | (1 << 6))] = alphaTextureDic[(name, (1 << 2) | (1 << 6))];
            }
            if (t1 == 1)
            {
                alphaTextureDic[(name, cur | (1 << 2) | (1 << 4))] = basicRotate[(AlphaTexBasic6.OOOXXOXXO, 3)];
            }
            else
            {
                alphaTextureDic[(name, cur | (1 << 2) | (1 << 4))] = alphaTextureDic[(name, (1 << 2) | (1 << 4))];
            }

            cur ^= (1 << t1);
        }

        alphaTextureDic[(name, (1 << 4) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                basicRotate[(AlphaTexBasic6.XXXXXXOXX,0)],
                basicRotate[(AlphaTexBasic6.XXXXXXOXX, 1)],
                basicRotate[(AlphaTexBasic6.OOOXXXXXX, 0)]
            });
        alphaTextureDic[(name, (1 << 6) | (1 << 8) | (1 << 2))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                basicRotate[(AlphaTexBasic6.XXXXXXOXX, 1)],
                basicRotate[(AlphaTexBasic6.XXXXXXOXX, 2)],
                basicRotate[(AlphaTexBasic6.OOOXXXXXX, 1)]
            });
        alphaTextureDic[(name, (1 << 2) | (1 << 4) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                basicRotate[(AlphaTexBasic6.XXXXXXOXX, 2)],
                basicRotate[(AlphaTexBasic6.XXXXXXOXX, 3)],
                basicRotate[(AlphaTexBasic6.OOOXXXXXX,2)]
            });
        alphaTextureDic[(name, (1 << 2) | (1 << 4) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                basicRotate[(AlphaTexBasic6.XXXXXXOXX,0)],
                basicRotate[(AlphaTexBasic6.XXXXXXOXX, 3)],
                basicRotate[(AlphaTexBasic6.OOOXXXXXX, 3)]
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
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 1)],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 0)]
                    });
            }
            else if ((cur & (1 << 9)) > 0)
            {
                alphaTextureDic[(name, cur | (1 << 4) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX,0)],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 0)]
                    });
            }
            else
            {
                alphaTextureDic[(name, cur | (1 << 4) | (1 << 6) | (1 << 8))] = alphaTextureDic[(name, (1 << 4) | (1 << 6) | (1 << 8))];
            }

            if ((cur & (1 << 9)) > 0)
            {
                alphaTextureDic[(name, cur | (1 << 2) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 2)],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 1)]
                    });
            }
            else if ((cur & (1 << 3)) > 0)
            {
                alphaTextureDic[(name, cur | (1 << 2) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 1)],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 1)]
                    });
            }
            else
            {
                alphaTextureDic[(name, cur | (1 << 2) | (1 << 6) | (1 << 8))] = alphaTextureDic[(name, (1 << 2) | (1 << 6) | (1 << 8))];
            }

            if ((cur & (1 << 1)) > 0)
            {
                alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 2)],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX,2)]
                    });
            }
            else if ((cur & (1 << 3)) > 0)
            {
                alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 3)],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX,2)]
                    });
            }
            else
            {
                alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 6))] = alphaTextureDic[(name, (1 << 2) | (1 << 4) | (1 << 6))];
            }

            if ((cur & (1 << 1)) > 0)
            {
                alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX,0)],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 3)]
                    });
            }
            else if ((cur & (1 << 7)) > 0)
            {
                alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 3)],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 3)]
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
                basicRotate[(AlphaTexBasic6.XXXXXXOXX,0)],
                basicRotate[(AlphaTexBasic6.XXXXXXOXX, 1)],
                basicRotate[(AlphaTexBasic6.XXXXXXOXX, 2)],
                basicRotate[(AlphaTexBasic6.XXXXXXOXX, 3)]
            });
        //5*
        alphaTextureDic[(name, cur | (1 << 1))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
               basicRotate[(AlphaTexBasic6.XXXXXXOXX,0)],
                basicRotate[(AlphaTexBasic6.XXXXXXOXX, 1)],
                basicRotate[(AlphaTexBasic6.XXXXXXOXX, 2)]
            });
        alphaTextureDic[(name, cur | (1 << 3))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
               basicRotate[(AlphaTexBasic6.XXXXXXOXX,0)],
                basicRotate[(AlphaTexBasic6.XXXXXXOXX, 1)],
                basicRotate[(AlphaTexBasic6.XXXXXXOXX, 3)]
            });
        alphaTextureDic[(name, cur | (1 << 7))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
               basicRotate[(AlphaTexBasic6.XXXXXXOXX,1)],
                basicRotate[(AlphaTexBasic6.XXXXXXOXX, 2)],
                basicRotate[(AlphaTexBasic6.XXXXXXOXX, 3)]
            });
        alphaTextureDic[(name, cur | (1 << 9))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
               basicRotate[(AlphaTexBasic6.XXXXXXOXX, 0 )],
                basicRotate[(AlphaTexBasic6.XXXXXXOXX, 2)],
                basicRotate[(AlphaTexBasic6.XXXXXXOXX, 3)]
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
                    alphaTextureDic[(name, cur | (1 << 4) | (1 << 6))] = alphaTextureDic[(name, (1 << 4) | (1 << 6))];
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 8))] = alphaTextureDic[(name, (1 << 2) | (1 << 8))];

                    if ((cur & (1 << 7)) > 0)
                    {
                        alphaTextureDic[(name, cur | (1 << 4) | (1 << 8))] = alphaTextureDic[(name, (1 << 4) | (1 << 7) | (1 << 8))];

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
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 0 )]
                    });
                }
                else if ((cur & (1 << 7)) > 0)
                {
                    alphaTextureDic[(name, cur | (1 << 4) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 1 )],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 0 )]
                        });
                }
                else if ((cur & (1 << 9)) > 0)
                {
                    alphaTextureDic[(name, cur | (1 << 4) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 0 )],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 0 )]
                        });
                }
                else
                {
                    alphaTextureDic[(name, cur | (1 << 4) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 0 )],
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 1 )],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 0 )]
                        });
                }


                if ((cur & ((1 << 3) | (1 << 9))) > 0)
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 1 )]
                    });
                }
                else if ((cur & (1 << 3)) > 0)
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 1 )],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 1 )]
                        });
                }
                else if ((cur & (1 << 9)) > 0)
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 2 )],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 1 )]
                        });
                }
                else
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 1 )],
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 2 )],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 1 )]
                        });
                }

                if ((cur & ((1 << 3) | (1 << 1))) > 0)
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 2 )]
                    });
                }
                else if ((cur & (1 << 3)) > 0)
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 3 )],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 2 )]
                        });
                }
                else if ((cur & (1 << 1)) > 0)
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 2 )],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 2 )]
                        });
                }
                else
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 2 )],
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 3 )],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 2 )]
                        });
                }

                if ((cur & ((1 << 7) | (1 << 1))) > 0)
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 3 )]
                    });
                }
                else if ((cur & (1 << 7)) > 0)
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 3 )],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 3 )]
                        });
                }
                else if ((cur & (1 << 1)) > 0)
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 0 )],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 3 )]
                        });
                }
                else
                {
                    alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 0 )],
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 3 )],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 3 )]
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
        alphaTextureDic[(name, cur | (1 << 4) | (1 << 8))] = alphaTextureDic[(name, (1 << 4) | (1 << 7) | (1 << 8))];
        alphaTextureDic[(name, cur | (1 << 6) | (1 << 8))] = alphaTextureDic[(name, (1 << 6) | (1 << 9) | (1 << 9))];
        alphaTextureDic[(name, cur | (1 << 2) | (1 << 6))] = alphaTextureDic[(name, (1 << 2) | (1 << 3) | (1 << 6))];
        alphaTextureDic[(name, cur | (1 << 2) | (1 << 4))] = alphaTextureDic[(name, (1 << 1) | (1 << 2) | (1 << 4))];

        alphaTextureDic[(name, cur | (1 << 2) | (1 << 8))] = alphaTextureDic[(name, (1 << 2) | (1 << 8))];
        alphaTextureDic[(name, cur | (1 << 4) | (1 << 6))] = alphaTextureDic[(name, (1 << 4) | (1 << 6))];

        alphaTextureDic[(name, (1 << 1) | (1 << 7) | (1 << 9) | (1 << 4) | (1 << 6) | (1 << 8))] = basicRotate[(AlphaTexBasic6.OOOXXXXXX, 2)];
        alphaTextureDic[(name, (1 << 3) | (1 << 7) | (1 << 9) | (1 << 4) | (1 << 6) | (1 << 8))] = basicRotate[(AlphaTexBasic6.OOOXXXXXX, 2)];
        alphaTextureDic[(name, (1 << 1) | (1 << 3) | (1 << 7) | (1 << 4) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 1 )],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 0 )]
                        });
        alphaTextureDic[(name, (1 << 1) | (1 << 3) | (1 << 9) | (1 << 4) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 0 )],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 0 )]
                        });


        alphaTextureDic[(name, (1 << 1) | (1 << 3) | (1 << 9) | (1 << 2) | (1 << 6) | (1 << 8))] = basicRotate[(AlphaTexBasic6.OOOXXXXXX, 1)];
        alphaTextureDic[(name, (1 << 3) | (1 << 7) | (1 << 9) | (1 << 2) | (1 << 6) | (1 << 8))] = basicRotate[(AlphaTexBasic6.OOOXXXXXX, 1)];
        alphaTextureDic[(name, (1 << 1) | (1 << 3) | (1 << 7) | (1 << 2) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 1 )],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 1 )]
                        });
        alphaTextureDic[(name, (1 << 1) | (1 << 7) | (1 << 9) | (1 << 2) | (1 << 6) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 2 )],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 1 )]
                        });


        alphaTextureDic[(name, (1 << 1) | (1 << 3) | (1 << 7) | (1 << 2) | (1 << 4) | (1 << 6))] = basicRotate[(AlphaTexBasic6.OOOXXXXXX, 2)];
        alphaTextureDic[(name, (1 << 1) | (1 << 3) | (1 << 9) | (1 << 2) | (1 << 4) | (1 << 6))] = basicRotate[(AlphaTexBasic6.OOOXXXXXX, 2)];
        alphaTextureDic[(name, (1 << 1) | (1 << 7) | (1 << 9) | (1 << 2) | (1 << 4) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 2 )],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 2 )]
                        });
        alphaTextureDic[(name, (1 << 3) | (1 << 7) | (1 << 9) | (1 << 2) | (1 << 4) | (1 << 6))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 3 )],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 2 )]
                        });

        alphaTextureDic[(name, (1 << 1) | (1 << 7) | (1 << 9) | (1 << 2) | (1 << 4) | (1 << 8))] = basicRotate[(AlphaTexBasic6.OOOXXXXXX, 3)];
        alphaTextureDic[(name, (1 << 1) | (1 << 3) | (1 << 7) | (1 << 2) | (1 << 4) | (1 << 8))] = basicRotate[(AlphaTexBasic6.OOOXXXXXX, 3)];
        alphaTextureDic[(name, (1 << 1) | (1 << 3) | (1 << 9) | (1 << 2) | (1 << 4) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 0 )],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 3 )]
                        });
        alphaTextureDic[(name, (1 << 3) | (1 << 7) | (1 << 9) | (1 << 2) | (1 << 4) | (1 << 8))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 3 )],
                        basicRotate[(AlphaTexBasic6.OOOXXXXXX, 3 )]
                        });


        cur = (1 << 2) | (1 << 4) | (1 << 6) | (1 << 8);
        alphaTextureDic[(name, cur | (1 << 1) | (1 << 9))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 0 )],
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 2 )]
                        });
        alphaTextureDic[(name, cur | (1 << 3) | (1 << 7))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 1 )],
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 3 )]
                        });
        alphaTextureDic[(name, cur | (1 << 1) | (1 << 7))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 1 )],
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 2 )]
                        });
        alphaTextureDic[(name, cur | (1 << 7) | (1 << 9))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 2 )],
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 3 )]
                        });
        alphaTextureDic[(name, cur | (1 << 3) | (1 << 9))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 0 )],
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 3 )]
                        });
        alphaTextureDic[(name, cur | (1 << 1) | (1 << 3))] = TextureCombine.OverlayTexture2DsToTexture2DByMinR(new Texture2D[] {
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 0 )],
                        basicRotate[(AlphaTexBasic6.XXXXXXOXX, 1 )]
                        });

        //7*
        cur = (1 << 2) | (1 << 4) | (1 << 6) | (1 << 8);
        alphaTextureDic[(name, cur | (1 << 1) | (1 << 3) | (1 << 7))] = basicRotate[(AlphaTexBasic6.XXXXXXOXX, 1)];
        alphaTextureDic[(name, cur | (1 << 1) | (1 << 7) | (1 << 9))] = basicRotate[(AlphaTexBasic6.XXXXXXOXX, 2)];
        alphaTextureDic[(name, cur | (1 << 3) | (1 << 7) | (1 << 9))] = basicRotate[(AlphaTexBasic6.XXXXXXOXX, 3)];
        alphaTextureDic[(name, cur | (1 << 1) | (1 << 3) | (1 << 9))] = basicRotate[(AlphaTexBasic6.XXXXXXOXX, 0)];

        cur = (1 << 1) | (1 << 3) | (1 << 7) | (1 << 9);
        alphaTextureDic[(name, cur | (1 << 4) | (1 << 6) | (1 << 8))] = basicRotate[(AlphaTexBasic6.OOOXXXXXX, 0)];
        alphaTextureDic[(name, cur | (1 << 2) | (1 << 6) | (1 << 8))] = basicRotate[(AlphaTexBasic6.OOOXXXXXX, 1)];
        alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 6))] = basicRotate[(AlphaTexBasic6.OOOXXXXXX, 2)];
        alphaTextureDic[(name, cur | (1 << 2) | (1 << 4) | (1 << 8))] = basicRotate[(AlphaTexBasic6.OOOXXXXXX, 3)];

        //8*
        alphaTextureDic[(name, (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 6) | (1 << 7) | (1 << 8) | (1 << 9))] = rawAlphaTex[(int)AlphaTexBasic6.XXXXXXXXX];


    }
    public void ShowFinalMat(TileInstance ins)
    {
        var data = ins.unit.data;
        if (!animCurCache.ContainsKey(data))
        {
            animCurCache[data] = new Dictionary<int, int>();
        }

        for (int i = 0; i < ins.renderers.Length; i++)
        {
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            ins.renderers[i].GetPropertyBlock(propBlock);

            if (data.texNameDic.ContainsKey(i) && MapTextureForm.DataByName.ContainsKey(data.texNameDic[i]))
            {

                int maskId = i + GlobalSettings.TERRAIN_LAYER_MAX;
                if (data.texNameDic.ContainsKey(maskId) && MapMaskForm.DataByName.ContainsKey(data.texNameDic[maskId]))
                {
                    var maskForm = MapMaskForm.DataByName[data.texNameDic[maskId]];

                    int linkDesc = 0;
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int z = -1; z <= 1; z++)
                        {
                            if (x == 0 && z == 0)
                                continue;
                            var pos = (x + data.mapPos.x, data.mapPos.y, z + data.mapPos.z);
                            if (MapManager.instance.data.maps.ContainsKey(pos)
                                && MapManager.instance.data.maps[pos].texNameDic.ContainsKey(i)
                                && MapManager.instance.data.maps[pos].texNameDic[i] == data.texNameDic[i])
                            {
                                linkDesc |= 1 << ((z + 1) * 3 + (x + 2));
                            }
                        }
                    }
                    propBlock.SetTexture("_AlphaTex", alphaTextureDic[(maskForm.name, linkDesc)]);

                }
                else
                {
                    propBlock.SetTexture("_AlphaTex", Texture2D.whiteTexture);
                }



                ins.renderers[i].enabled = true;

                var texForm = MapTextureForm.DataByName[data.texNameDic[i]];

                TimeManager.instance.CancelTimer(ins.animTimer[i]);

                if (texForm.animTimeInterval > 0)
                {
                    float all = texForm.animTimeInterval * texForm.texsName.Count;

                    int cur = (int)((Time.time % all) / texForm.animTimeInterval);
                    float timeProgress = (Time.time % texForm.animTimeInterval);

                    ins.renderers[i].GetPropertyBlock(propBlock);
                    animCurCache[data][i] = cur;
                    propBlock.SetTexture("_Tex", TexAssetForm.DataByName[texForm.texsName[cur]].GetTex());
                    int renderId = i;
                    ins.animTimer[i] = TimeManager.instance.StartTimer(timeProgress, texForm.animTimeInterval, () =>
                    {
                        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                        ins.renderers[renderId].GetPropertyBlock(propBlock);
                        cur = (cur + 1) % texForm.texsName.Count;
                        animCurCache[data][renderId] = cur;
                        propBlock.SetTexture("_Tex", TexAssetForm.DataByName[texForm.texsName[cur]].GetTex());
                        ins.renderers[renderId].SetPropertyBlock(propBlock);
                        return false;
                    }, ins);
                }
                else
                {
                    propBlock.SetTexture("_Tex", TexAssetForm.DataByName[texForm.texsName[0]].GetTex());
                }
            }
            else
            {
                ins.renderers[i].enabled = false;
                propBlock.SetTexture("_AlphaTex", Texture2D.blackTexture);
            }



            ins.renderers[i].SetPropertyBlock(propBlock);
        }
    }
    public void RegisterObject(ObjectUnitForm.Data newObjectData,MapObjectForm.Data objectData)
    {
        newObjectData.unit.productInfo = (objectData.id, -1);
        newObjectData.unit.paramInfo = new Dictionary<string, GameParamForm.Data>();
        foreach (var pair in objectData.paramDic)
            newObjectData.unit.paramInfo[pair.Key] = pair.Value;

        newObjectData.isObstacle = objectData.collision;
    }
    public void OnEvent(TileEvent evt)
    {
        switch (evt.type)
        {
            case MapEventType.Show:
                ShowFinalMat((TileInstance)evt.unit.ins);
                break;
            case MapEventType.AfterUpdate:
                break;
        }
    }

}
