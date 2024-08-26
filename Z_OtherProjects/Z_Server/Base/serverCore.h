#ifndef serverCore_H
#define serverCore_H

#include <WS2tcpip.h>  // Winsock2 库
#include<map>
#include<vector>
#include<iostream>

#define CLIENTTUPLE std::tuple<unsigned long, unsigned short>
#define BUFFER_LENGTH 10240
#define DIFCHECKTIME 60

class ClientServer
{
public:
	ClientServer(SOCKET client, sockaddr_in clientAddr);
	virtual void sendMsg(char*& msg, int length);
	virtual void onReceiveMsg(char* data, int length);
	SOCKET socket;
	sockaddr_in clientAddr;



protected:

	//void threadDo();
};
class UdpClientServer : public ClientServer
{
public:
	UdpClientServer(SOCKET client, sockaddr_in clientAddr);
	void onReceiveMsg(char* data, int length) override;
	void sendMsg(char*& msg, int length) override;
protected:

	int uniqueId;
};
class TcpClientServer : public ClientServer
{
public:
	TcpClientServer(SOCKET client, sockaddr_in clientAddr);
	void onReceiveMsg(char* data, int length) override;
	void sendMsg(char*& msg, int length) override;
};




class ListenerServer
{
public:
	ListenerServer(int _localPort);
	virtual void listenerThreadDo();
	void start();
protected:
	std::map<CLIENTTUPLE, ClientServer*> id2Client;
	std::vector<CLIENTTUPLE>idList;

	int localPort;


	virtual void manageRealMsg(CLIENTTUPLE address, char*& realMsg, int& length);

};
class UdpListenerServer : public ListenerServer
{
public:
	UdpListenerServer(int _localPort);
	void listenerThreadDo() override;
protected:
	void manageRealMsg(CLIENTTUPLE address, char*& realMsg, int& length) override;
	int idTot;
	std::map<CLIENTTUPLE, std::map<int, double>>client2ReceivedTimeDic;
};
class TcpListenerServer : public ListenerServer
{

public:
	TcpListenerServer(int _localPort);
	void listenerThreadDo() override;
protected:
	void manageRealMsg(CLIENTTUPLE address, char*& realMsg, int& length) override;
	void subListenerThreadDo(CLIENTTUPLE address, SOCKET socketClient);
};




int start(int type);

void debugSockaddrIn(sockaddr_in addr);

#endif 
