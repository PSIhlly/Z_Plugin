#include <iostream>
#include <WS2tcpip.h>  // Winsock2 库
#include "..\Base\serverCore.h"
#include <chrono>
#include <vector>
#include "..\Util\byteSerialize.h"
using namespace std;
TcpListenerServer::TcpListenerServer(int _localPort, function<void(Msg)> _onReceiveCallBack, function<void(CLIENTTUPLE)> _onCloseCallBack) : ListenerServer(_localPort, _onReceiveCallBack)
{
	onCloseCallBack = _onCloseCallBack;
}
void TcpListenerServer::listenerThreadDo()
{
	// 创建一个 TCP 套接字
	SOCKET tcpSocket = socket(AF_INET, SOCK_STREAM, 0);
	if (tcpSocket == INVALID_SOCKET) {
		cerr << "Can't create TCP socket! Quitting" << endl;
		return;
	}

	// 绑定套接字到 IP 和端口
	sockaddr_in hint;
	hint.sin_family = AF_INET;
	hint.sin_port = htons(localPort);  // 本地端口，注意 htons 将主机字节序转换为网络字节序
	hint.sin_addr.S_un.S_addr = INADDR_ANY;  // 接收所有 IP 地址

	::bind(tcpSocket, (sockaddr*)&hint, sizeof(hint));
	cout << localPort<<"StartWait" << endl;

	int socketClient;
	listen(tcpSocket, 5);
	while (true)
	{
		sockaddr_in clientAddr;
		int clientAddrSize = sizeof(clientAddr);
		

		socketClient = accept(tcpSocket, (struct sockaddr*)&clientAddr, (socklen_t*)&clientAddrSize);
		if (socketClient == INVALID_SOCKET)
		{
			cerr << "Connect fail";
			continue;
		}
		//1.登记
		CLIENTTUPLE address = make_tuple(clientAddr.sin_addr.S_un.S_addr, clientAddr.sin_port);

		//2.记录该用户
		auto bindCloseClient = std::bind(&TcpListenerServer::closeClient, this, std::placeholders::_1);
		TcpClientServer* client = new TcpClientServer(socketClient, clientAddr, onReceiveCallBack, bindCloseClient);

		id2Client[address] = client;
		idList.push_back(address);
		cout << "new!" << endl;
		cout << socketClient << endl;
		//起接收线程
		threads.emplace_back(&TcpListenerServer::subListenerThreadDo, this, address, socketClient);
		//thread mainThread();

	}
}

void TcpListenerServer::closeClient(CLIENTTUPLE address)
{
	if (id2Client[address]!=NULL)
	{
		if (onCloseCallBack)
		{
			onCloseCallBack(address);
		}
		cout << "Over " << endl << id2Client[address]->socket << endl;
		// 关闭套接字和清理 Winsock
		closesocket(id2Client[address]->socket);
		delete id2Client[address];
	}
	id2Client[address] = NULL;

	auto it = std::find(idList.begin(), idList.end(), address);
	if (it != idList.end()) {
		idList.erase(it);
	}


}

void TcpListenerServer::subListenerThreadDo(CLIENTTUPLE address, SOCKET socketClient)
{
	vector<char> lengthBytes;
	// 等待接收数据
		char rawMsg[BUFFER_LENGTH];
		vector<char> realMsg;
		ZeroMemory(rawMsg, BUFFER_LENGTH);
		int length = 0;
		int received = 0;
		while (true)
		{
			if (id2Client[address] == NULL)
				break;
			cout << "qianlai!"  << endl;
			int bytesReceived = recv(socketClient, rawMsg, BUFFER_LENGTH, 0);
			cout << bytesReceived << endl;
			if (bytesReceived <=0) {
				cerr << "connect fail"<<endl;
				break;
			}
			int p = 0;
			while (true)
			{
				int remain = length - received;
				//read
				if (remain > 0)
				{
					if (bytesReceived - p < remain)
					{
						for (; p < bytesReceived; p++)
						{
							realMsg.push_back(rawMsg[p]);
							received++;
						}
						break;
					}
					else
					{
						for (int i = 0; i < remain; p++, i++)
						{
							realMsg.push_back(rawMsg[p]);
							received++;
						}


						//3.接收调起

						char* msg = new char[realMsg.size()];
						for (int i = 0; i < realMsg.size(); i++)
						{
							msg[i] = realMsg[i];
						}	cout << static_cast<void*>(msg) << std::endl;

						manageRealMsg(address, msg, length);
						cout << static_cast<void*>(msg) << std::endl;
						delete[] msg;

						realMsg.clear();
						lengthBytes.clear();
						length = 0;
						received = 0;
					}
				}

				int realDataRemain = bytesReceived - p;
				//finish
				if (realDataRemain == 0)
					break;

				//getlength
				if (realDataRemain < 4 - lengthBytes.size())
				{

					for (; p < bytesReceived; p++)
					{
						lengthBytes.push_back(rawMsg[p]);
					}
					break;
				}
				for (; lengthBytes.size() < 4; p++)
				{
					lengthBytes.push_back(rawMsg[p]);
				}
				for (int i = 3; i >= 0; i--)
				{
					length = (length << 8) | (lengthBytes[i] & 0xFF);  // 最低字节
				}
			}

		}
		closeClient(address);
}

void TcpListenerServer::manageRealMsg(CLIENTTUPLE address, char*& realMsg, int& length)
{
	(*id2Client[address]).onReceiveMsg(realMsg, length);
}