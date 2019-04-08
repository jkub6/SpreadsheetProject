
#include "MasterController.h"
#include "ConnectionListener.h"
#include "SpreadsheetController.h"
#include "Utilities.h"
#include <string>
#include <thread>
#include <iostream>
#include <vector>

MasterController::MasterController(int port, std::string pathToSaveDirectory)
{
  this->port = port;
  this->pathToSaveDirectory = pathToSaveDirectory;

  this->connectionListener = new ConnectionListener(port, newClientConnected);
  this->spreadsheetController = new SpreadsheetController(MasterController::pathToSaveDirectory);
  
}
MasterController::~MasterController()
{
  //TODO
}

void MasterController::startServer()
{
  this->connectionListener->beginListeningForClients();

  
  //TODO
}
void MasterController::shutdown(){
  //TODO
}


void MasterController::newClientConnected(int socketID)
{
  bool connected = true;
  
  while(connected)
    {
      
      
      std::string *remainingMessage = new std::string();
      int bytesread = 0;
      
      std::vector<std::string> *vec = Utilities::receiveMessage(socketID, &bytesread,remainingMessage);


      if(bytesread == 0)//socket disconnected
	{
	  std::cout<<"Socket ID: "<<socketID<<" disconnected."<<std::endl;
	  connected = false;
	  return;
	}

      std::cout<<std::endl<<"Message from: "<<socketID<<" Full commands: "<<std::endl;
      for(int i = 0;i<vec->size();i++)
	{
	  std::cout<<"["<<(*vec)[i]<<"]"<<std::endl;
	}
      
      
      std::cout<<"Remaining Message: "<<std::endl<<(*remainingMessage)<<std::endl;
    }
}

//ENTRY POINT

int main(int argc, char ** argv)
{
  MasterController *masterController = new MasterController(2112,".");
  std::thread connectionThread(&MasterController::startServer,masterController);//implicit this parameter

  connectionThread.join();
  
  return 0;
}

 
