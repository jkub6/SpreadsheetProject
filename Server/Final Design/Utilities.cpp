


#include "Utilities.h"
#include <string>
#include <sys/socket.h>
#include <unistd.h>  
#include <netinet/in.h>
#include <vector>
#include <iostream>

void Utilities::sendMessage(int socketID, std::string message)
{
  //TODO
}

std::vector<std::string>* Utilities::receiveMessage(int socketID, int *bytesRead, std::string * remainingMessage)
{

  int bufferLength = 1024;
  char receiveBuffer[bufferLength];


  *bytesRead = read(socketID,receiveBuffer,bufferLength);


  

  
  std::string *rawString = new std::string((char*)receiveBuffer,*bytesRead);

  
  //*bytesRead = a;
  
  std::string * remain;

  std::vector<std::string> *tokens = Tokenize(rawString);


  
  *remainingMessage = *rawString;

  delete rawString;

  
  return tokens;
}

std::vector<std::string>* Utilities::Tokenize(std::string * input)
  {
    std::vector<std::string> *tokens = new std::vector<std::string>();
    
    std::string delimiter = "\n\n";

    for(int index = input->find(delimiter);index>0;)
      {
	std::string subString = input->substr(0,index);
	tokens->push_back(subString);
	index+=2;

	*input = input->erase(0,index);
	index = input->find(delimiter);
      }
    
    return tokens;
    
  }


//Returns true if connection is still active, false otherwise
bool Utilities::checkConnectionState(int socketID)
{
  return false;
}

