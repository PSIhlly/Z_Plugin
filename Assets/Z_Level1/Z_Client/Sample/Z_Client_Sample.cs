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
    public InputField iFCoin;
    public Text showCoins;
    void Start()
    {
        ClientCore.instance.Init(new Param[] { new Param( ProtoType.Tcp,1234, "127.0.0.1", 55550, null),
        new Param(ProtoType.Tcp, 2345, "127.0.0.1", 55551, OnReceive)});
    }

    public void OnReceive(int port,byte[] data)
    {
        string content = Encoding.UTF8.GetString(data);
        Debug.Log(content + "<__");
        showCoins.text = content;
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
        int real = 0;
        if (int.TryParse(iFCoin.text, out real))
        {
            real *= 15000;
        }

        ClientCore.instance.Send(1234, Encoding.ASCII.GetBytes(iF.text+"$"+ real));
    }
    public void Get()
    {
        ClientCore.instance.Send(2345, Encoding.ASCII.GetBytes(iF.text));
    }
}
