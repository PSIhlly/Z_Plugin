#include <iostream>
#include <WS2tcpip.h>  // Winsock2 库
#include "serverCore.h"
#include <thread>
#include <chrono>
#include <vector>
#include "..\Util\byteSerialize.h"
using namespace std;

void ListenerServer::listenerThreadDo()
{

}
ListenerServer::ListenerServer(int _localPort, std::function<void(Msg)> _onReceiveCallBack)
{
	localPort = _localPort;
	onReceiveCallBack = _onReceiveCallBack;
}
void ListenerServer::manageRealMsg(CLIENTTUPLE address, char*& realMsg, int& length)
{

}
void ListenerServer::start()
{
	thread mainThread(&ListenerServer::listenerThreadDo, this);
	mainThread.join();
}