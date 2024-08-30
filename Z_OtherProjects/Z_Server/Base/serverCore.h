#ifndef serverCore_H
#define serverCore_H

#include <WS2tcpip.h>  // Winsock2 库
#include<map>
#include<vector>
#include<iostream>
#include <functional>
#include <thread>

#define CLIENTTUPLE std::tuple<unsigned long, unsigned short>
#define BUFFER_LENGTH 10240
#define DIFCHECKTIME 60

class ClientServer;


struct Msg
{
	ClientServer* client;
	CLIENTTUPLE id;
	char* mes;
	int mesLength;
};

class ClientServer
{
public:
	ClientServer(SOCKET client, sockaddr_in clientAddr, std::function<void(Msg)> onReceiveCallBack);
	virtual void sendMsg(char*& msg, int length);
	virtual void sendMsg(std::string msg);
	virtual void onReceiveMsg(char* data, int length);
	SOCKET socket;
	sockaddr_in clientAddr;

protected:
	std::function<void(Msg)> onReceiveCallBack;
	//void threadDo();
};
class UdpClientServer : public ClientServer
{
public:
	UdpClientServer(SOCKET client, sockaddr_in clientAddr, std::function<void(Msg)> onReceiveCallBack);
	void onReceiveMsg(char* data, int length) override;
	void sendMsg(char*& msg, int length) override;
    void sendMsg(std::string msg) override;
protected:

	int uniqueId;
};
class TcpClientServer : public ClientServer
{
public:
	TcpClientServer(SOCKET client, sockaddr_in clientAddr, std::function<void(Msg)> onReceiveCallBack, std::function<void(CLIENTTUPLE)> onCloseCallBack);
	void onReceiveMsg(char* data, int length) override;
	void sendMsg(char*& msg, int length) override;
	void sendMsg(std::string msg) override;

	std::function<void(CLIENTTUPLE)> onCloseCallBack;
	void close();
};




class ListenerServer
{
public:
	ListenerServer(int _localPort, std::function<void(Msg)> onReceiveCallBack);
	virtual void listenerThreadDo();
	void start();
protected:
	std::map<CLIENTTUPLE, ClientServer*> id2Client;
	std::vector<CLIENTTUPLE>idList;

	int localPort;
	std::function<void(Msg)> onReceiveCallBack;

	virtual void manageRealMsg(CLIENTTUPLE address, char*& realMsg, int& length);

};
class UdpListenerServer : public ListenerServer
{
public:
	UdpListenerServer(int _localPort, std::function<void(Msg)> onReceiveCallBack);
	void listenerThreadDo() override;
protected:
	void manageRealMsg(CLIENTTUPLE address, char*& realMsg, int& length) override;
	int idTot;
	std::map<CLIENTTUPLE, std::map<int, double>>client2ReceivedTimeDic;
	
};
class TcpListenerServer : public ListenerServer
{

public:
	TcpListenerServer(int _localPort, std::function<void(Msg)> onReceiveCallBack, std::function<void(CLIENTTUPLE)> onCloseCallBack);
	void listenerThreadDo() override;
protected:
	std::vector<std::thread> threads;
	void manageRealMsg(CLIENTTUPLE address, char*& realMsg, int& length) override;
	void subListenerThreadDo(CLIENTTUPLE address, SOCKET socketClient);
	void closeClient(CLIENTTUPLE targetClient);

	std::function<void(CLIENTTUPLE)> onCloseCallBack;
};




std::thread start(int type,int port, std::function<void(Msg)> onReceiveCallBack, std::function<void(CLIENTTUPLE)> onCloseCallBack);

void debugSockaddrIn(sockaddr_in addr);

#endif 
