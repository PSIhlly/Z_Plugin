#include <iostream>
#include <WS2tcpip.h>  // Winsock2 库
#include "..\Base\serverCore.h"
#include <thread>
#include <chrono>
#include <vector>
#include "..\Util\byteSerialize.h"
using namespace std;
UdpClientServer::UdpClientServer(SOCKET _socket, sockaddr_in _clientAddr, function<void(Msg)> _onReceiveCallBack) :ClientServer(_socket, _clientAddr, _onReceiveCallBack)
{
	uniqueId = 0;
}
void UdpClientServer::sendMsg(char*& msg, int length)
{
	int clientAddrSize = sizeof(clientAddr);
	uniqueId++;
	
	setInHeadBytes(uniqueId, msg, length);

	setInHeadBytes(length, msg, length);

	//连发三次
	for (int i = 0; i < 3; i++)
	{
		int sendSize = sendto(socket, msg, length, 0,
			reinterpret_cast<sockaddr*>(&clientAddr), clientAddrSize);
		if (sendSize == SOCKET_ERROR) {
			cerr << "sendto failed with error: " << WSAGetLastError() << endl;
		}
		else
		{
			cout << "send ok ";
		}
	}



}
void UdpClientServer::sendMsg(string msg)
{
	char* now = new char[msg.size() + 1];
	for (int i = 0; i < msg.size() + 1; i++)
		now[i] = msg[i];

	sendMsg(now, msg.size() + 1);
	delete now;
}
void UdpClientServer::onReceiveMsg(char* data, int length)
{
	// 打印接收到的数据
	debug(data, length);
	Msg msg;
	msg.mes = data;
	msg.mesLength = length;
	msg.client = this;
	if (onReceiveCallBack) {
		onReceiveCallBack(msg);  // 调用回调函数
	}

}