#include <iostream>
#include <WS2tcpip.h>  // Winsock2 库
#include "..\Base\serverCore.h"
#include <thread>
#include <chrono>
#include <vector>
#include <WinSock2.h>
#include "..\Util\byteSerialize.h"
#include < cstring >
using namespace std;

TcpClientServer::TcpClientServer(SOCKET _socket, sockaddr_in _clientAddr,function<void(Msg)> _onReceiveCallBack, function<void(CLIENTTUPLE)> _onCloseCallBack) :ClientServer(_socket, _clientAddr, _onReceiveCallBack)
{
	onCloseCallBack = _onCloseCallBack;
}
void TcpClientServer::sendMsg(char*& msg, int length)
{
	char* now = new char[length+1]; 
	std::copy(msg, msg + length + 1, now);

	setInHeadBytes(length, now, length);

    int sendSize=send(socket, now, length, 0);
		if (sendSize == SOCKET_ERROR) {
			cerr << "sendto failed with error: " << WSAGetLastError() << endl;
		}
		else
		{
			cout << "send ok ";
		}
		delete[] now;
}
void TcpClientServer::sendMsg(string msg)
{
	char* now = new char[msg.size()];
	for (int i = 0; i < msg.size(); i++)
	{
		now[i] = msg[i];
	}

	sendMsg(now, msg.size() + 1);
	delete[] now;
}
void TcpClientServer::onReceiveMsg(char* data, int length)
{
	// 打印接收到的数据
	debug(data, length);
	char* now = new char[length + 1];
	std::copy(data, data + length + 1, now);
	Msg msg;
	msg.mes = now;
	msg.id = make_tuple(clientAddr.sin_addr.S_un.S_addr,clientAddr.sin_port);
	msg.mesLength = length;
	msg.client = this;
	if (onReceiveCallBack) {
		onReceiveCallBack(msg);  // 调用回调函数
	}
	delete[] now;
}

void TcpClientServer::close()
{
	if (onCloseCallBack)
	{
		onCloseCallBack(make_tuple(clientAddr.sin_addr.S_un.S_addr, clientAddr.sin_port));
	}
}