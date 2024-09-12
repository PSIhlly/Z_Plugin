using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using Z_Os.DllImporter;
using Z_Os.File;
public class Z_Os_Sample : MonoBehaviour
{
    public Text log;


    AndroidJavaObject jo;
    public void Start()
    {
        

        FileImporter.ImportImageBytes(OnImportImageBytesComplete);
    }

    public void OnImportImageBytesComplete(byte[] data)
    {
        log.text =data.Length + "";
    }



}
