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
    public class TcpClientListener : ClientListener
    {

        protected TcpClient tcpClient;
        public TcpClientListener(TcpClient tcpClient, Action<byte[]> onReceive):base(onReceive)
        {
            this.tcpClient = tcpClient;
            this.onReceive = onReceive;
            Task.Run(() =>
            {

                NetworkStream stream = tcpClient.GetStream();
                byte[] rawMsg = new byte[ClientCore.BUFFER_LENGTH];
                byte[] realMsg=new byte[0];
                List<byte> lengthBytes = new List<byte>(4);
                int length = 0;
                int received = 0;
                while (true)
                {
                    Debug.Log("wating");
                    int bytesRead = stream.Read(rawMsg, 0, rawMsg.Length);
                    int p = 0;
                    while (true)
                    {

                        int remain = length - received;
                        if (remain > 0)
                        {
                            if (rawMsg.Length - p < remain)
                            {
                                Buffer.BlockCopy(rawMsg, 0, realMsg, received, rawMsg.Length);
                                received += rawMsg.Length - p;
                                break;
                            }
                            else
                            {
                                Buffer.BlockCopy(rawMsg, p, realMsg, received, remain);
                                p += remain;

                                ManageRealMsg(realMsg);

                                length = 0;
                                received = 0;
                                lengthBytes.Clear();
                            }
                        }

                        int realDataRemain = bytesRead - p;
                        Debug.Log(realDataRemain);
                        if (realDataRemain == 0)
                            break;
                        //length
                        if (realDataRemain < 4 - lengthBytes.Count)
                        {

                            for (; p < rawMsg.Length; p++)
                            {
                                lengthBytes.Add(rawMsg[p]);
                            }
                            break;
                        }

                        for (; lengthBytes.Count < 4; p++)
                        {
                            lengthBytes.Add(rawMsg[p]);
                        }

                        length = BitConverter.ToInt32(lengthBytes.ToArray());
                        realMsg = new byte[length];
                    }

                }

            });
        }

        protected override void ManageRealMsg(byte[] realMsg)
        {
            onReceive?.Invoke(realMsg);
        }
    }
}