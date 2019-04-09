


#include "Utilities.h"
#include <string>
#include <sys/socket.h>
#include <unistd.h>  
#include <netinet/in.h>
#include <vector>
#include <iostream>
#include <fstream>
#include <mutex>


std::vector<std::string> *Utilities::spreadsheetList;

std::mutex * Utilities::spreadSheetListMtx;

void Utilities::sendMessage(int socketID, std::string message)
{
  send(socketID, message.c_str() , message.length() , 0 );  
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


//singleton
std::vector<std::string>* Utilities::getSpreadsheetList()
{
  if(Utilities::spreadsheetList == NULL)
    {
     
      Utilities::spreadsheetList = new std::vector<std::string>();
      Utilities::spreadSheetListMtx = new std::mutex();
      
      std::ifstream stream("./save/spreadsheetList");
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
