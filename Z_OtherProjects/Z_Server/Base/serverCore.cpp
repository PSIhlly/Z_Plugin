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

void createServer(int type, int port, function<void(Msg)>  onReceiveCallBack,function<void(CLIENTTUPLE)>  onCloseCallBack);

thread start(int type,int port, function<void(Msg)>  onReceiveCallBack, function<void(CLIENTTUPLE)>  onCloseCallBack)//0udp 1tcp
{
	thread mainThread(createServer,type,port, onReceiveCallBack, onCloseCallBack);
	return mainThread;
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


