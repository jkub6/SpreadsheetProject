//https://www.geeksforgeeks.org/socket-programming-cc/

// Server side C/C++ program to demonstrate Socket programming 
#include <unistd.h> 
#include <sys/socket.h> 
#include <netinet/in.h>
#include <iostream>
#include <string.h> 
#define PORT 8080 
int main(int argc, char const *argv[]) 
{ 
  int socketID, new_socket, valread; 
  struct sockaddr_in address; 
  int opt = 1; 
  int addrlen = sizeof(address); 
  int bufferLength = 1024;
  char buffer[bufferLength] = {0};
  char *hello = (char*)"Hello from server";
  
  // Creating socket file descriptor
  // socketID is a socket descriptor, an integer (like a file handle)
  // parameters= (domain, type, protocol),
  // domain = AF_INET = IPV4, Type = SOCK_STREAM = TCP, PROTOCOL = 0 (IP).
  if ((socketID = socket(AF_INET, SOCK_STREAM, 0)) == 0) 
    { 
      std::cout<<"Failed to connect"<<std::endl;
      exit(EXIT_FAILURE); 
    } 
  
  // Forcefully attaching socket to the port 8080
  // is optional, but prevents errors
  if (setsockopt(socketID, SOL_SOCKET, SO_REUSEADDR | SO_REUSEPORT, 
		 &opt, sizeof(opt))) 
    { 
      std::cout<<"error"<<std::endl;
      exit(EXIT_FAILURE); 
    }
  
  address.sin_family = AF_INET; //use ipv4
  address.sin_addr.s_addr = INADDR_ANY; //for localhost
  address.sin_port = htons( PORT ); //port to bind
  
  // Forcefully attaching socket to the port 8080
  //Binds socket to port using sockaddr struct
  if (bind(socketID, (struct sockaddr *)&address, sizeof(address))<0) 
    { 
      std::cout<<"bind failed"<<std::endl;
      exit(EXIT_FAILURE); 
    }

  std::cout<<"Awaiting Connection on port "<<PORT<<std::endl;
  //NON blocking, socketID is never used for sending/receiving, is used only by server as a way to get new sockets
  if (listen(socketID, 20) < 0) //20 is number of active participants that can "wait" for a connection
    {
      exit(EXIT_FAILURE); 
    }
  //blocking
  if ((new_socket = accept(socketID, (struct sockaddr *)&address, (socklen_t*)&addrlen))<0) 
    {
      std::cout<<"accept"<<std::endl;
      exit(EXIT_FAILURE); 
    }

  std::cout<<"Connection Established..."<<std::endl;

  for(;;){
  valread = read( new_socket , buffer, bufferLength);
  std::cout<<"Message Received"<<std::endl;
  std::cout<<buffer<<std::endl;
  send(new_socket , buffer , bufferLength , 0 );
  std::cout<<"sent: "<<buffer<<std::endl;
  }
  return 0; 
} 
