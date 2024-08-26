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

int start(int type)//0udp 1tcp
{

	// 初始化 Winsock
	WSADATA wsData;
	WORD ver = MAKEWORD(2, 2);
	int wsOK = WSAStartup(ver, &wsData);
	if (wsOK != 0) {
		cerr << "Can't initialize Winsock! Quitting" << endl;
		return -1;
	}
	switch (type)
	{
	case 0:
	{
		UdpListenerServer udpServer = UdpListenerServer(5678);
		udpServer.start();
		break;
	}
	case 1:
	{
		TcpListenerServer tcpServer = TcpListenerServer(5678);
		tcpServer.start();
		break;
	}

	}


	WSACleanup();
	return 0;
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


