#ifndef serverCore_H
#define serverCore_H

#include <WS2tcpip.h>  // Winsock2 库
#include<map>
#include<vector>
#include<iostream>
#include <functional>
#include <thread>
#define EXP extern "C" _declspec(dllexport)
#define CLIENTTUPLE std::tuple<unsigned long, unsigned short>
#define BUFFER_LENGTH 10240
#define DIFCHECKTIME 60

class ListenerServer;

extern std::map<int, ListenerServer*>port2listener;
typedef void (*OnReceiveCallbackType)(unsigned long, unsigned short, char[], int); // 定义回调函数指针类型
typedef void (*OnCloseCallbackType)(unsigned long, unsigned short); // 定义回调函数指针类型

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
	std::map<CLIENTTUPLE, ClientServer*> id2Client;
	void start();
protected:
	
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

EXP void run(int type, int port, OnReceiveCallbackType onReceiveCallback, OnCloseCallbackType onCloseCallBack);
EXP int init();
EXP void over();
EXP void sendMassage(int port, unsigned long id_ip, unsigned short id_port, char* data, int length);
EXP int test();
void debugSockaddrIn(sockaddr_in addr);

#endif 
