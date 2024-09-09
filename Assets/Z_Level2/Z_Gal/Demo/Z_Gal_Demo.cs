using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Gal.Core;
using Z_Gal.GalUI;
public class Z_Gal_Demo : MonoBehaviour
{
    // Start is called before the first frame update
     public InputManager inputManager;
    private void Start()
    {
        
     }
    public void FakeData()
    {
        JObject joMainText = new JObject();
        JArray jaMainText = new JArray();
        joMainText["mainText"] = jaMainText;
        jaMainText[0]["id"] = 0;
        jaMainText[0]["text"] = "sdw苏打粉额asdssssssssssssssssssssssssssssssssssssssssssssssssssssssssss外d";
        jaMainText[1]["id"] = 1;
        jaMainText[1]["text"] = "岁数大assadasdawdwadsdfwafsafdaw我！！$yy";
        jaMainText[2]["id"] = 2;
        jaMainText[2]["text"] = "ww2";

        JObject joClip = new JObject();
        JArray jaClip = new JArray();
        joClip["clip"] = jaClip;
        jaClip[0]["id"] = 0;
        jaClip[0]["idInMainTextDatabase"] = 0;
        jaClip[0]["idInMainPictureDatabase"] = 0;
        jaClip[1]["id"] = 1;
        jaClip[1]["idInMainTextDatabase"] = 1;
        jaClip[1]["idInMainPictureDatabase"] = 0;
        jaClip[2]["id"] = 2;
        jaClip[2]["idInMainTextDatabase"] = 2;
        jaClip[2]["idInMainPictureDatabase"] = 0;

        Z_Gal.Core.GameManager.instance.InitDatabase(joMainText + "", joClip+"");
        Z_Gal.Core.GameManager.instance.InitSettings();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            inputManager.Click(Input.mousePosition);
        }
    }
}
