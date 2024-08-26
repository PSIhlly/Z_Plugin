using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Z_ByteSerialize;

namespace Z_Client
{

    public abstract class ClientListener
    {
        protected Dictionary<int, float> receivedTimeDic=new Dictionary<int, float>();
        protected Action<byte[]> onReceive;
        public ClientListener(Action<byte[]> onReceive)
        {
           
        }
        
        protected virtual void ManageRealMsg(byte[] realMsg)
        {

        }
        
    }
}
