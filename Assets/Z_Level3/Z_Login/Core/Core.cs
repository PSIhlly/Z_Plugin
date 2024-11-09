using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Z_Client;

public class Core : MonoBehaviour
{
    public Text tip;
    public const int loginPort = 1234;
    public InputField account;
    public InputField code;
    void Start()
    {
        ClientCore.instance.Init(new Param[] {new Param(ProtoType.Tcp, loginPort, "127.0.0.1", 44441, OnReceive) });
    }
    public void OnClickGetCode()
    {
        ClientCore.instance.Send(loginPort, Encoding.UTF8.GetBytes("1$" + account.text));
    }
    // Update is called once per frame
    public void OnClickLogin()
    {
        ClientCore.instance.Send(loginPort, Encoding.UTF8.GetBytes("2$" + account.text+"$"+ code.text));
    }
    public void OnReceive(int port,byte[] mes)
    {
        string content = Encoding.UTF8.GetString(mes);
        print("接收：" + content);
        string[] res = content.Split("$");
        switch(res[0])
        {
            case "1":
                tip.text = res[1];
                break;
            case "2":
                
                tip.text = res[2];
                break;
        }
    }
}
