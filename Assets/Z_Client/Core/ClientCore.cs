using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
        public Param(ProtoType protoType,int localPort, string targetIp, int targetPort)
        {
            this.targetIp = targetIp;
            this.targetPort = targetPort;
            this.localPort = localPort;
            this.protoType = protoType;
        }
    }

    public class ClientCore : MonoBehaviour//todo:monoSingleton
    {

        public const int BUFFER_LENGTH = 10240;

        public const float DIFCHECKTIME = 60;

        private static ClientCore _instance;

        private Dictionary<int, (ClientListener, ClientSender)> netPairDic;

        internal static float timeNow;

        public static ClientCore Instance
        {
            get
            {
                if (_instance == null)
                {
                    var listener = new GameObject("ClientCore");
                    _instance = listener.AddComponent<ClientCore>();
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        
        public void Update()
        {
            timeNow = Time.time;
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
                            OnReceive(initParam.localPort, msg);
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
                            OnReceive(initParam.localPort, msg);
                        }), new TcpClientSender(tcpClient, initParam.targetIp, initParam.targetPort));

                        break;


                }
                
            }
        }

        public virtual void OnReceive(int localPort, byte[] msg)
        {
            string content = Encoding.UTF8.GetString(msg);
            Debug.Log("收到了" + content);
        }

        public virtual void Send(int localPort, byte[] msg)
        {
            
            netPairDic[localPort].Item2.Send(msg);

        }
    }
}