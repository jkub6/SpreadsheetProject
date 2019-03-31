
#include "ConnectionListener.h"
#include <iostream>
#include <unistd.h> 
#include <sys/socket.h> 
#include <netinet/in.h>
#include <string.h>
#include <thread>
#include <list>
#include <vector>

ConnectionListener::ConnectionListener(int port,   void (*callBack)(int))
{
  this->port = port;
  this->callBack = callBack;
}
ConnectionListener::~ConnectionListener()
{
  //TODO
}

void ConnectionListener::shutdown()
{
  //TODO
}

void ConnectionListener::beginListeningForClients()
{
   
  int genesisSocket, new_socket, valread; 
  struct sockaddr_in address; 
  int opt = 1; 
  int addrlen = sizeof(address); 
  int bufferLength = 1024;
  char buffer[bufferLength];
  char *hello = (char*)"Hello from server";
  
  // Creating socket file descriptor
  // genesisSocket is a socket descriptor, an integer (like a file handle)
  // parameters= (domain, type, protocol),
  // domain = AF_INET = IPV4, Type = SOCK_STREAM = TCP, PROTOCOL = 0 (IP).
  if ((genesisSocket = socket(AF_INET, SOCK_STREAM, 0)) == 0) 
    { 
      std::cout<<"Failed to connect"<<std::endl;
      exit(EXIT_FAILURE); 
    } 
  
  // Forcefully attaching socket to the port 8080
  // is optional, but prevents errors
  if (setsockopt(genesisSocket, SOL_SOCKET, SO_REUSEADDR | SO_REUSEPORT, 
		 &opt, sizeof(opt))) 
    { 
      std::cout<<"error"<<std::endl;
      exit(EXIT_FAILURE); 
    }
  
  address.sin_family = AF_INET; //use ipv4
  address.sin_addr.s_addr = INADDR_ANY; //for localhost
  address.sin_port = htons( port); //port to bind
  
  // Forcefully attaching socket to the port 8080
  //Binds socket to port using sockaddr struct
  if (bind(genesisSocket, (struct sockaddr *)&address, sizeof(address))<0) 
    { 
      std::cout<<"bind failed"<<std::endl;
      exit(EXIT_FAILURE); 
    }

  std::cout<<"Awaiting Connection on port "<<port<<std::endl;
  //NON blocking, genesisSocket is never used for sending/receiving, is used only by server as a way to get new sockets

      if (listen(genesisSocket, 20) < 0) //20 is number of active participants that can "wait" for a connection
	{
	  exit(EXIT_FAILURE); 
	}

  
  while(true)
    {   
    
      //blocking
    if ((new_socket = accept(genesisSocket, (struct sockaddr *)&address, (socklen_t*)&addrlen))<0) 
      {
	std::cout<<"accept"<<std::endl;
	exit(EXIT_FAILURE); 
      }

    
    std::cout<<"Connection Established. ClientID: "<<new_socket<<std::endl;
    callBack(new_socket);//call the callback
  }
  
}

