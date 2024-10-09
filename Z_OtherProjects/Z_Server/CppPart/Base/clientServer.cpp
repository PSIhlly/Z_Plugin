#include <iostream>
#include <WS2tcpip.h>  // Winsock2 库
#include "serverCore.h"
#include <thread>
#include <chrono>
#include <vector>
#include "..\Util\byteSerialize.h"
using namespace std;
ClientServer::ClientServer(SOCKET _socket, sockaddr_in _clientAddr,function<void(Msg)> _onReceiveCallBack)
{
    socket = _socket;
    clientAddr = _clientAddr;
    onReceiveCallBack = _onReceiveCallBack;
}
void ClientServer::sendMsg(char*& msg, int length)
{
}
void ClientServer::sendMsg(string msg)
{
}

void ClientServer::onReceiveMsg(char* data, int length)
{
	debug(data, length);
}


