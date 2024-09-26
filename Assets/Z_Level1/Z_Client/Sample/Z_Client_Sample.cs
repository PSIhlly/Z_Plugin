using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Z_Client;

public class Z_Client_Sample : MonoBehaviour
{
    // Start is called before the first frame update
    public InputField iF;
    void Start()
    {
        ClientCore.Instance.Init(new Param[] { new Param( ProtoType.Tcp,1234, "118.31.13.94", 55550) });
    }

    public void Update()
    {
      
      
    }
    public void Render()
    {
        System.Random random = new System.Random();
        string code = "";
        for(int i=0;i<5;i++)
        {
            code += (char)random.Next('A', 'Z' + 1);
        }
        iF.text = code;
    }
    public void Send()
    {
        ClientCore.Instance.Send(1234, Encoding.ASCII.GetBytes(iF.text));
    }
}
