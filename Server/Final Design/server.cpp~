// Server side C/C++ program to demonstrate Socket programming 
#include <unistd.h> 
#include <sys/socket.h> 
#include <netinet/in.h>
#include <iostream>
#include <string.h> 
#define PORT 2112

#include "ConnectionListener.h"
#include <thread>


int serverStuff(int argc,char const *argv[]);


int main(int argc, char const *argv[]) 
{
  std::thread listeningThread(beginListeningForClients,PORT);

  listeningThread.join();
  
  return 0;
}

