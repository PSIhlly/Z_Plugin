using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Z_Client;

public class Z_Client_Sample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ClientCore.Instance.Init(new Param[] { new Param( ProtoType.Tcp,1234, "127.0.0.1", 55555), new Param(ProtoType.Tcp, 5678, "127.0.0.1", 55555) });
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            ClientCore.Instance.Send(1234, Encoding.ASCII.GetBytes("1$weq"));
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            ClientCore.Instance.Send(1234, Encoding.ASCII.GetBytes("10$"));
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            ClientCore.Instance.Send(1234, Encoding.ASCII.GetBytes("11$1"));
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ClientCore.Instance.Send(1234, Encoding.ASCII.GetBytes("12$"));
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            ClientCore.Instance.Send(1234, Encoding.ASCII.GetBytes("3$weq"));
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            ClientCore.Instance.Send(1234, Encoding.ASCII.GetBytes("13$0$1$1"));
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            ClientCore.Instance.Send(5678, Encoding.ASCII.GetBytes("1$sweq"));
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            ClientCore.Instance.Send(5678, Encoding.ASCII.GetBytes("10$"));
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            ClientCore.Instance.Send(5678, Encoding.ASCII.GetBytes("11$1"));
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            ClientCore.Instance.Send(5678, Encoding.ASCII.GetBytes("12$"));
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            ClientCore.Instance.Send(1234, Encoding.ASCII.GetBytes("31$aaaaaaa"));
        }
    }
}
