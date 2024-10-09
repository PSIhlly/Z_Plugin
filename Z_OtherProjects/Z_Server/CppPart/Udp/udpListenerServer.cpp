#include <iostream>
#include <WS2tcpip.h>  // Winsock2 库
#include "..\Base\serverCore.h"
#include <thread>
#include <chrono>
#include <vector>
#include "..\Util\byteSerialize.h"
using namespace std;

UdpListenerServer::UdpListenerServer(int _localPort, function<void(Msg)> _onReceiveCallBack) : ListenerServer(_localPort, _onReceiveCallBack)
{

}

void UdpListenerServer::listenerThreadDo()
{
	// 创建一个 UDP 套接字
	SOCKET udpSocket = socket(AF_INET, SOCK_DGRAM, 0);
	if (udpSocket == INVALID_SOCKET) {
		cerr << "Can't create UDP socket! Quitting" << endl;
		return;
	}

	// 绑定套接字到 IP 和端口
	sockaddr_in hint;
	hint.sin_family = AF_INET;
	hint.sin_port = htons(localPort);  // 本地端口，注意 htons 将主机字节序转换为网络字节序
	hint.sin_addr.S_un.S_addr = INADDR_ANY;  // 接收所有 IP 地址

	::bind(udpSocket, (sockaddr*)&hint, sizeof(hint));
	cout << "StartWait" << endl;

	int length = 0;
	int received = 0;
	vector<char> lengthBytes;

	// 等待接收数据
	while (true)
	{
		sockaddr_in clientAddr;
		int clientAddrSize = sizeof(clientAddr);
		char rawMsg[BUFFER_LENGTH];
		vector<char> realMsg;
		ZeroMemory(rawMsg, BUFFER_LENGTH);

		int bytesReceived = recvfrom(udpSocket, rawMsg, BUFFER_LENGTH, 0,
			(sockaddr*)&clientAddr, &clientAddrSize);
		if (bytesReceived == SOCKET_ERROR) {

			cout << "CNM!!!";
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
					//1.登记
					CLIENTTUPLE address = make_tuple(clientAddr.sin_addr.S_un.S_addr, clientAddr.sin_port);
					//2.记录该用户
					if (id2Client[address] == NULL)
					{
						UdpClientServer* client = new UdpClientServer(udpSocket, clientAddr, onReceiveCallBack);
						(*client).clientAddr = clientAddr;

						id2Client[address] = client;
						idList.push_back(address);
						cout << "new:" << clientAddr.sin_addr.S_un.S_addr << endl;
					}
					//3.接收调起
					char* msg = new char[realMsg.size()];
					for (int i = 0; i < realMsg.size(); i++)
					{
						msg[i] = realMsg[i];
					}
					manageRealMsg(address, msg, length);
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
			//
		}
	}
	// 关闭套接字和清理 Winsock
	closesocket(udpSocket);
}

void UdpListenerServer::manageRealMsg(CLIENTTUPLE address, char*& realMsg, int& length)
{
	
	int uniqueId = -1;
	getOutHeadBytes(realMsg, length, uniqueId);
	// 获取当前时间点
	auto now = chrono::system_clock::now();
	// 转换为毫秒时间戳
	auto now_ms = chrono::time_point_cast<chrono::milliseconds>(now);
	// 计算毫秒部分
	auto value = now.time_since_epoch();
	double seconds = chrono::duration<double, std::milli>(value).count()/1000;

	if (client2ReceivedTimeDic[address][uniqueId] == 0 || abs(seconds - client2ReceivedTimeDic[address][uniqueId]) > DIFCHECKTIME)
	{
		client2ReceivedTimeDic[address][uniqueId] = seconds;
		(*id2Client[address]).onReceiveMsg(realMsg, length);
	}
}