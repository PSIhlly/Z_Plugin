using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Z_DesignStyle;
namespace Z_Client
{
    public enum ProtoType
    {
        Udp,
        Tcp
    }
        public class Param
    {
        public string targetIp;
        public int targetPort;
        public int localPort;
        public ProtoType protoType;
        public Action<int,byte[]> onReceive;
        public Param(ProtoType protoType,int localPort, string targetIp, int targetPort, Action<int, byte[]> onReceive)
        {
            this.targetIp = targetIp;
            this.targetPort = targetPort;
            this.localPort = localPort;
            this.protoType = protoType;
            this.onReceive = onReceive;
        }
    }
    public class ReceiveMsg
    {
        public int localPort;
        public byte[] msg;
        public Action<int, byte[]> onReceive;
    }

    public sealed class ClientCore : Z_MonoSingleton<ClientCore>
    {

        public const int BUFFER_LENGTH = 10240;

        public const float DIFCHECKTIME = 60;

        private Dictionary<int, (ClientListener, ClientSender)> netPairDic;

        internal static float timeNow;

        private List<ReceiveMsg> receiveList = new List<ReceiveMsg>();

        private static string ListLock = "lock";
        
        public void Update()
        {
            timeNow = Time.time;
            lock(ListLock)
            {
            foreach(var rec in receiveList)
            {
                rec.onReceive.Invoke(rec.localPort, rec.msg);
            }
            receiveList.Clear();

            }
        }


        public void Init(Param[] initParams)
        {
            netPairDic = new Dictionary<int, (ClientListener, ClientSender)>();

            foreach (var initParam in initParams)
            {
                switch(initParam.protoType)
                {
                    case ProtoType.Udp:

                        UdpClient udpClient = new UdpClient(initParam.localPort);

                        netPairDic[initParam.localPort] = (new UdpClientListener(udpClient, (msg) =>
                        {
                            OnReceive(new ReceiveMsg(){
                                localPort=initParam.localPort, 
                                msg=msg,
                                onReceive=initParam.onReceive 
                            });
                        }), new UdpClientSender(udpClient, initParam.targetIp, initParam.targetPort));
                        break;
                    case ProtoType.Tcp:

                        //custom localPort
                        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socket.Bind(new IPEndPoint(IPAddress.Any, initParam.localPort));
                        TcpClient tcpClient = new TcpClient{ Client = socket };
                        tcpClient.Connect(initParam.targetIp, initParam.targetPort);

                        netPairDic[initParam.localPort] = (new TcpClientListener(tcpClient, (msg) =>
                        {
                            OnReceive(new ReceiveMsg()
                            {
                                localPort = initParam.localPort,
                                msg = msg,
                                onReceive = initParam.onReceive
                            });
                        }), new TcpClientSender(tcpClient, initParam.targetIp, initParam.targetPort));

                        break;


                }
                
            }
        }

        public void OnReceive(ReceiveMsg msg)
        {
            lock (ListLock)
            {
                receiveList.Add(msg);
            }
        }

        public void Send(int localPort, byte[] msg)
        {
            
            netPairDic[localPort].Item2.Send(msg);

        }
    }
}