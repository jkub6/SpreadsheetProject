
#include "SocketState.h"
#include <string>
#include "Utilities.h"
#include <vector>
#include <mutex>
#include <iostream>

SocketState::SocketState(int socketID)
{
  this->socketID = socketID;
  this->bufferMtx = new std::mutex();
  this->buffer = new std::string();
  connected = true;
}


SocketState::~SocketState()
{
  delete buffer;
  delete bufferMtx;
  std::cout<<"SockeState for socket ID: "<<socketID<<" deconstructed."<<std::endl;
}

int SocketState::getID()
{
  return this->socketID;
}

bool SocketState::isConnected()
{
  return connected;
}

std::string SocketState::getBuffer()
{
  return *buffer;
}

void SocketState::setConnected(bool con)
{
  this->connected = con;
}

std::string SocketState::getSingleMessage()
{  this->bufferMtx->lock();
  int index = buffer->find("\n\n");
  std::string result="";
  
  if(index>0&&index<buffer->length())
    {
      result = buffer->substr(0,index);
      index+=2;
      *buffer = buffer->erase(0,index);
    }
  
  this->bufferMtx->unlock();

  return result;
}

void SocketState::appendMessage(std::string message)
{
  this->bufferMtx->lock();

  *buffer = buffer->append(message);
  
  this->bufferMtx->unlock();
}

std::vector<std::string> * SocketState::getCommandsToProcess()
{
  std::vector<std::string> * commands = tokenize();

  return commands;
}

std::vector<std::string> * SocketState::tokenize()
{
  this->bufferMtx->lock();
  std::vector<std::string> *tokens = new std::vector<std::string>();
  
  std::string delimiter = "\n\n";

  
  
  for(int index = buffer->find(delimiter);index>0;)
    {
      if(index<0||index>=buffer->length())
	{
	  break;
	}
      
      std::string subString = buffer->substr(0,index);
      tokens->push_back(subString);
      index+=2;
      
      *buffer = buffer->erase(0,index);
      index = buffer->find(delimiter);
    }

  this->bufferMtx->unlock();
  return tokens;
}


void SocketState::socketAwaitData()
{
  
  while(connected)
    {
      std::string newData = Utilities::receiveMessage(this);

      
      this->bufferMtx->lock();
      *buffer = buffer->append(newData);
      this->bufferMtx->unlock();
    }

  std::cout<<"SocketState for SocketID: "<<socketID<<" Disconnected..."<<std::endl;
}
void SocketState::socketSendData(std::string msg)
{
  Utilities::sendMessage(this,msg);
}

    
