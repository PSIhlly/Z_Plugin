using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Z_Audio;
public class Z_Audio_Sample : MonoBehaviour
{
    public AudioSource audioSource;

    // Update is called once per frame
    async void Start()
    {
        var path = Application.dataPath + "/Z_Level0/Z_Audio/Sample/test.mp3";
        if (File.Exists(path))
        {
            var clip = await AudioHelper.GetAudioByPath(path);
            Debug.Log(clip.length);
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogError(path + " not exist");
        }


    }
}
