#include <iostream>
#include <WS2tcpip.h>  // Winsock2 库
#include "..\Base\serverCore.h"
#include <thread>
#include <chrono>
#include <vector>
#include <WinSock2.h>
#include "..\Util\byteSerialize.h"
using namespace std;

TcpClientServer::TcpClientServer(SOCKET _socket, sockaddr_in _clientAddr) :ClientServer(_socket, _clientAddr)
{

}
void TcpClientServer::sendMsg(char*& msg, int length)
{
	int clientAddrSize = sizeof(clientAddr);
	setInHeadBytes(length, msg, length);

    int sendSize=send(socket, msg, length, 0);
		if (sendSize == SOCKET_ERROR) {
			cerr << "sendto failed with error: " << WSAGetLastError() << endl;
		}
		else
		{
			cout << "send ok ";
		}
}

void TcpClientServer::onReceiveMsg(char* data, int length)
{
	// 打印接收到的数据
	debug(data, length);

	//内存隔离
	char* sendData = new char[length];
	for (int i = 0; i < length; i++)
	{
		sendData[i] = data[i];
	}
	sendMsg(sendData, length);//发回去
	delete[] sendData;
}