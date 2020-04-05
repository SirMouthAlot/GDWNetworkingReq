#pragma once

#include "Client.h"

class Server
{
public:
	std::vector<ClientConnectionData> clients;

	bool InitServer(const char* IP, const int port);
	void Cleanup();

private:
	int  InitSocket();
	int  BindSocket();
	int  InitWinsock();

	bool StartListen();

	void RecvLoop();
	void ProcessMessage(const char* msg, sockaddr_in clientAddr);
	bool IsNewUser(sockaddr_in clientAddr);
	void SendPacketToTargetClient(const char* msg, sockaddr_in clientAddr);
	void BroadcastMessage(const char* msg, sockaddr_in clientAddrExcept);
	ClientConnectionData* FindClientByAddress(sockaddr_in addr);

	SOCKET sock;
	HANDLE threadHandle;
	const char* IP;
	int port;
	bool listening = false;
};