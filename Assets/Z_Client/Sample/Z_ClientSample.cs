using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Z_Client;

public class Z_ClientSample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ClientCore.Instance.Init(new Param[] { new Param( ProtoType.Tcp,1234, "127.0.0.1", 5678), new Param(ProtoType.Tcp, 2468, "127.0.0.1", 5678) });
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.W))
        {
            ClientCore.Instance.Send(1234, Encoding.ASCII.GetBytes("wwww"));
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            ClientCore.Instance.Send(2468, Encoding.ASCII.GetBytes("ssss"));
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            ClientCore.Instance.Send(1234, Encoding.ASCII.GetBytes("aaaa"));
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            ClientCore.Instance.Send(2468, Encoding.ASCII.GetBytes("dddd"));
        }
    }
}
