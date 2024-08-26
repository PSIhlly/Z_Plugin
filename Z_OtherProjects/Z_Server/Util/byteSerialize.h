#ifndef byteSerialize_H
#define byteSerialize_H

void setInHeadBytes(int src, char*& tar, int& length);

void getOutHeadBytes(char*& src, int& length, int& tar);

void debug(char* log, int length);
#endif 