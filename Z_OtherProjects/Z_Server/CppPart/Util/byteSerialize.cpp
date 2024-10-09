#include "byteSerialize.h"
#include<vector>
#include <iostream>
using namespace std;
void setInHeadBytes(int src, char*& tar, int& length)
{
    vector<char> realData;

    for (int i = 0; i < sizeof(int); ++i) {
        realData.push_back((src >> (i * 8)) & 0xFF);
    }
    for (int i = 0; i < length; ++i) {
        realData.push_back(tar[i]);
    }

    delete[] tar;
    tar = new char[realData.size()];
    for (int i = 0; i < realData.size(); ++i) {
        tar[i] = realData[i];
    }
    length += sizeof(int);

}

void getOutHeadBytes(char*& src, int& length, int& tar)
{
    vector<char> realData;
    tar = 0;
    for (int i = 3; i >= 0; i--)
    {
        tar = (tar << 8) | (src[i] & 0xFF);  // 最低字节
    }
    for (int i = sizeof(int); i < length; ++i) {
        realData.push_back(src[i]);
    }

    delete[] src;
    src = new char[realData.size()];
    for (int i = 0; i < realData.size(); ++i) {
        src[i] = realData[i];
    }
    length -= sizeof(int);
}

void debug(char* log, int length)
{
    cout << "LOG:";
    for (int i = 0; i < length; i++)
    {
        cout << (log[i]);
    }
    cout << endl;
}