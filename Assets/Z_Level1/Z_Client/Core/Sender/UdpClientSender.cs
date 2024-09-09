using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using Z_ByteSerialize;

namespace Z_Client
{
    public class UdpClientSender: ClientSender
    {
        protected int uniqueId;//重复消息唯一识别id
        protected UdpClient udpClient;
        public UdpClientSender(UdpClient udpClient, string targetIp, int targetPort):base(targetIp,targetPort)
        {
            this.udpClient = udpClient;
        }
        public override void Send(byte[] msg)
        {
            Debug.Log(msg.Length + " " + targetIp + " " + targetPort);

            //套一层uid
            uniqueId++;
            BytesSerialize.SetInHeadBytes(uniqueId,ref msg);
            byte[] realMsg = msg;

            //长度做头
            BytesSerialize.SetInHeadBytes(realMsg.Length,ref realMsg);

            // 将消息内容复制到新数组中

            byte[] rawMsg = realMsg;

            // 发送数据到指定的远程主机和端口
            //三连发
            for(int i=0;i<3;i++)
            {
                udpClient.Send(rawMsg, rawMsg.Length, targetIp, targetPort);
            }
            


        }
    }

}
