#include"Base/serverCore.h"
#include <iostream>
#include <thread>
#include <functional>
#include"game.h"
using namespace std;





//void GG(std::function<void(string)> func)
//{
//	func();
//	cout << "hahaha";
//}



int main()
{
	//thread th(GG);


	//return 0;
	// 初始化 Winsock
	WSADATA wsData;
	WORD ver = MAKEWORD(2, 2);
	int wsOK = WSAStartup(ver, &wsData);
	if (wsOK != 0) {
		cerr << "Can't initialize Winsock! Quitting" << endl;
		return -1;
	}


	thread modDownloadThread=modDownloadStart();
	thread pvploadThread = pvpStart();
	modDownloadThread.join();
	pvploadThread.join();

	WSACleanup();
}
