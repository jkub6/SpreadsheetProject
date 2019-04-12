


#include "Utilities.h"
#include <string>
#include <sys/socket.h>
#include <unistd.h>  
#include <netinet/in.h>
#include <vector>
#include <iostream>
#include <fstream>
#include <mutex>
#include "SocketState.h"
#include "sha256.h"


std::vector<std::string> *Utilities::spreadsheetList;

std::mutex * Utilities::spreadSheetListMtx;


std::string Utilities::hash(std::string input)
{
  SHA256 sha256;

  return sha256(input);
}

void Utilities::sendMessage(SocketState * sstate, std::string message)
{
  int socketID = sstate->getID();

  std::string newMsg = message+'\n'+'\n';

  send(socketID, newMsg.c_str(), newMsg.length() , 0 );  
}

std::string Utilities::receiveMessage(SocketState * sstate)
{
  int socketID = sstate->getID();
  int bufferLength = 1024;
  char receiveBuffer[bufferLength];

  int bytesRead = read(socketID,receiveBuffer,bufferLength);


  if(bytesRead<=0)//client disconnected
    {
      sstate->setConnected(false);
      return "";
    }

  
  std::string rawString((char*)receiveBuffer,bytesRead);
  
  return rawString;
}

//singleton
std::vector<std::string>* Utilities::getSpreadsheetList()
{
  if(Utilities::spreadsheetList == NULL)
    {
     
      Utilities::spreadsheetList = new std::vector<std::string>();
      Utilities::spreadSheetListMtx = new std::mutex();
      
      std::ifstream stream("./data/spreadsheetList");
      std::string line;

      Utilities::spreadSheetListMtx->lock();
      while(getline(stream,line))
	{
	  Utilities::spreadsheetList->push_back(line);
	}
      Utilities::spreadSheetListMtx->unlock();

      stream.close();
      
      return Utilities::spreadsheetList;
    }

        return Utilities::spreadsheetList;
}


void Utilities::newSpreadsheetInList(std::string name)
{
  if(Utilities::spreadsheetList==NULL)
    Utilities::getSpreadsheetList();

  
  Utilities::spreadSheetListMtx->lock();

  Utilities::spreadsheetList->push_back(name);
  
  Utilities::spreadSheetListMtx->unlock();
}
