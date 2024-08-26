#include <iostream>
#include <WS2tcpip.h>  // Winsock2 库
#include "..\Base\serverCore.h"
#include <thread>
#include <chrono>
#include <vector>
#include "..\Util\byteSerialize.h"
using namespace std;
UdpClientServer::UdpClientServer(SOCKET _socket, sockaddr_in _clientAddr) :ClientServer(_socket, _clientAddr)
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

void UdpClientServer::onReceiveMsg(char* data, int length)
{
	// 打印接收到的数据
	debug(data, length);
	//内存隔离
	char* sendData = new char[length];
	for (int i = 0; i < length; i++)
	{
		sendData[i]=data[i];
	}
	sendMsg(sendData, length);//发回去
	delete[] sendData;
}