using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using Z_ByteSerialize;

namespace Z_Client
{
    public class TcpClientSender: ClientSender
    {
        protected int uniqueId;//重复消息唯一识别id
        protected TcpClient tcpClient;

        public TcpClientSender(TcpClient tcpClient, string targetIp, int targetPort):base(targetIp,targetPort)
        {
            this.tcpClient = tcpClient;
        }
        public override bool Send(byte[] msg)
        {
            lock (streamLock)
            {
                try
                {
                    NetworkStream stream = tcpClient.GetStream();

                    BytesSerialize.SetInHeadBytes(msg.Length, ref msg);
                    byte[] rawMsg = msg;

                    stream.Write(rawMsg, 0, rawMsg.Length);

                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return false;
                }

            }

        }
    }

}
