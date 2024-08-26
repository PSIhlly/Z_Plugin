#include <iostream>
#include <WS2tcpip.h>  // Winsock2 库
#include "serverCore.h"
#include <thread>
#include <chrono>
#include <vector>
#include "..\Util\byteSerialize.h"
using namespace std;
ClientServer::ClientServer(SOCKET _socket, sockaddr_in _clientAddr)
{
    socket = _socket;
    clientAddr = _clientAddr;
}
void ClientServer::sendMsg(char*& msg, int length)
{
}

void ClientServer::onReceiveMsg(char* data, int length)
{
	debug(data, length);
}


