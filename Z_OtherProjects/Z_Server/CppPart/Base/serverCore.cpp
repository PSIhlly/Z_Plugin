#include <iostream>
#include <WS2tcpip.h>  // Winsock2 库
#include <thread>
#include <chrono>
#include <map>
#include <vector>
#include "..\Util\byteSerialize.h"
#include <chrono>
#include <cmath>
#include "serverCore.h"
#pragma comment(lib, "ws2_32.lib")  // 链接 ws2_32.lib 库文件
using namespace std;
std::map<int, ListenerServer*>port2listener;
void createServer(int type, int port, function<void(Msg)>  onReceiveCallBack, function<void(CLIENTTUPLE)>  onCloseCallBack);

EXP void run(int type, int port, OnReceiveCallbackType onReceiveCallback, OnCloseCallbackType onCloseCallBack)
{
	std::function<void(Msg)> onReceive = [onReceiveCallback](Msg msg) {
		onReceiveCallback(get<0>(msg.id), get<1>(msg.id), msg.mes, msg.mesLength);
	};
	std::function<void(CLIENTTUPLE)> onClose = [onCloseCallBack](CLIENTTUPLE id) {
		onCloseCallBack(get<0>(id), get<1>(id));
	};
	createServer(type, port, onReceive, onClose);
}
void createServer(int type, int port, function<void(Msg)>  onReceiveCallBack, function<void(CLIENTTUPLE)>  onCloseCallBack)
{
	
	switch (type)
	{
	case 0:
	{
		UdpListenerServer udpServer = UdpListenerServer(port, onReceiveCallBack);
		udpServer.start();
		break;
	}
	case 1:
	{
		TcpListenerServer tcpServer = TcpListenerServer(port, onReceiveCallBack, onCloseCallBack);
		tcpServer.start();
		break;
	}

	}
}

EXP int init()
{
	// 初始化 Winsock
	WSADATA wsData;
	WORD ver = MAKEWORD(2, 2);
	int wsOK = WSAStartup(ver, &wsData);
	if (wsOK != 0) {
		cerr << "Can't initialize Winsock! Quitting" << endl;
		return -1;
	}
}
EXP void over()
{
	WSACleanup();
}

EXP void sendMassage(int port, unsigned long id_ip, unsigned short id_port, char* data, int length)
{
	auto id = make_tuple(id_ip, id_port);
	if(port2listener.find(port)!= port2listener.end())
		if(port2listener[port]->id2Client.find(id)!= port2listener[port]->id2Client.end())
			if(port2listener[port]->id2Client[id]!=NULL)
				port2listener[port]->id2Client[id]->sendMsg(data, length);
}

EXP int test()
{
	return 4;
}
void debugSockaddrIn(sockaddr_in addr) {
	// 获取端口号
	uint16_t port = ntohs(addr.sin_port);

	// 获取 IP 地址
	char ipStr[INET_ADDRSTRLEN]; // INET_ADDRSTRLEN is a constant for IPv4 address strings
	inet_ntop(AF_INET, &(addr.sin_addr), ipStr, INET_ADDRSTRLEN);

	// 打印 IP 地址和端口号
	std::cout << "IP Address: " << ipStr << std::endl;
	std::cout << "Port: " << port << std::endl;
}


