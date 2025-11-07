using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Z_Audio;
using Z_Video;
public class Z_Video_Sample : MonoBehaviour
{
    public VideoPlayer player;

    // Update is called once per frame
    void Start()
    {
        var path = Application.dataPath + "/Z_Level0/Z_Video/Sample/test.mp4";
        if (File.Exists(path))
        if (File.Exists(path))
        {
            player.PlayVideoByPath(path);
        }
        else
        {
            Debug.LogError(path + " not exist");
        }

    }
}
