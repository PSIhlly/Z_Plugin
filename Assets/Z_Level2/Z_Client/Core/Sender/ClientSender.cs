using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using Z_ByteSerialize;

namespace Z_Client
{
    public abstract class ClientSender
    {
        protected string targetIp;
        protected int targetPort;


        public ClientSender(string targetIp, int targetPort)
        {
            this.targetIp = targetIp;
            this.targetPort = targetPort;
        }
        public virtual void Send(byte[] msg)
        {
        }
    }

}
