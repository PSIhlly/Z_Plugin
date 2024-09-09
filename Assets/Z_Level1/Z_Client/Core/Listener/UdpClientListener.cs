using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using Z_ByteSerialize;

namespace Z_Client
{
    public class UdpClientListener : ClientListener
    {

        protected UdpClient udpClient;
        public UdpClientListener(UdpClient udpClient, Action<byte[]> onReceive):base(onReceive)
        {
            this.udpClient = udpClient;
            this.onReceive = onReceive;
            Task.Run(() =>
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                int received = 0;
                int length = 0;
                List<byte> lengthBytes = new List<byte>(4);
                byte[] realMsg = new byte[0];
                byte[] rawMsg = new byte[0];
                Debug.Log("在等");
                while (true)
                {
                    int p = 0;
                    try
                    {
                        // 接收数据并记录发送方的终结点
                        rawMsg = udpClient.Receive(ref remoteEP);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                        break;
                    }


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
                                lengthBytes = new List<byte>(4);
                            }
                        }

                        int realDataRemain = rawMsg.Length - p;

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
            object uniqueId = -1;
            BytesSerialize.GetOutHeadBytes(ref realMsg, ref uniqueId);
            byte[] msg = realMsg;
            if (!receivedTimeDic.ContainsKey((int)uniqueId) || Math.Abs(ClientCore.timeNow - receivedTimeDic[(int)uniqueId]) > ClientCore.DIFCHECKTIME)
            {
                receivedTimeDic[(int)uniqueId] = ClientCore.timeNow;
                onReceive?.Invoke(msg);
            }
        }
    }
}