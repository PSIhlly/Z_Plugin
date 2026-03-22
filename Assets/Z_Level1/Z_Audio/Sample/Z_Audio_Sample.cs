using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Z_Audio;
public class Z_Audio_Sample :MonoBehaviour
{
   

    // Update is called once per frame
    void Start()
    {
        AudioManager.instance.Bgm("D:\\Works\\Game\\Z_Plugin\\Assets\\Z_Level1\\Z_Audio\\Sample\\东方境能使.mp3");
        //AudioManager.instance.BGM();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            AudioManager.instance.Play("D:\\Works\\Game\\Z_Plugin\\Assets\\Z_Level1\\Z_Audio\\Sample\\点击.wav");
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            AudioManager.instance.Bgm("D:\\Works\\Game\\Z_Plugin\\Assets\\Z_Level1\\Z_Audio\\Sample\\胜利.wav");
        }
    }
}
