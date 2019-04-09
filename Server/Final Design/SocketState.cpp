
#include "SocketState.h"
#include <string>
#include "Utilities.h"
#include <vector>

SocketState::SocketState(int socketID)
{
  this->socketID = socketID;
  commandsToProcess = new std::vector<std::string>();
  remainingMessage = new std::string();
  connected = true;
}
SocketState::~SocketState()
{
  delete remainingMessage;
  delete commandsToProcess;
}

bool SocketState::isConnected()
{
  return connected;
}

std::string SocketState::getRemainingMessage()
{
  return *remainingMessage;
}
std::vector<std::string> * SocketState::getCommandsToProcess()
{
  return this->commandsToProcess;
}

void SocketState::socketAwaitData()
{
  
  std::string previousRemaining = *remainingMessage;
  int bytesread = 0;
  std::vector<std::string> * newCommands = Utilities::receiveMessage(this->socketID,&bytesread,remainingMessage);
  
  if(bytesread==0)
    {
      //client disconnected
      this->connected = false;
      return;
    }
  //add previous remaining to first command
  if(newCommands->size()>0)
    {
      std::string newStr = previousRemaining + (std::string)(*newCommands)[0];
      (*newCommands)[0]=newStr;
    }
  
  //add new messages to vector
  
  for(int i = 0;i<newCommands->size();i++)
    {
      commandsToProcess->push_back((*newCommands)[i]);
    }
  
}
void SocketState::socketSendData(std::string msg)
{
  //TODO
}

    
