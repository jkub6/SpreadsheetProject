
#include "MasterController.h"
#include "ConnectionListener.h"
#include "SpreadsheetController.h"
#include "Utilities.h"
#include <string>
#include <thread>
#include <iostream>
#include <vector>
#include "nlohmann/json.hpp"
#include "SocketState.h"




MasterController::MasterController(int port)
{
  this->port = port;

  this->connectionListener = new ConnectionListener(port, newClientConnected);
  this->spreadsheetController = new SpreadsheetController();

}
MasterController::~MasterController()
{
  delete connectionListener;
  delete spreadsheetController;
}


void MasterController::startServer()
{
  this->connectionListener->beginListeningForClients();

  
  //TODO
}
void MasterController::shutdown(){
  //TODO
}


int MasterController::newClientConnected(int socketID)
{

  std::cout<<"NEW CLIENTS:DLKFJD:L"<<std::endl;
  //***************************
  //send list of spreadsheets:
  //******************************
  std::vector<std::string> *list = Utilities::getSpreadsheetList();
  
  nlohmann::json jsonObject;


  jsonObject["type"]="list";

  for(int i = 0;i<list->size();i++)
    {
      jsonObject["spreadsheet"].push_back((*list)[i]);
    };


  Utilities::sendMessage(socketID, jsonObject.dump(0));

  //***********************
  //AWAIT RESPONSE
  //**********************
  
  std::cout<<"JSON SENT: \n"<<jsonObject.dump(0)<<std::endl;;//jsonObject.dump();

  SocketState * sstate = new SocketState(socketID);


  while(sstate->isConnected())
    {
      sstate->socketAwaitData();

      if(!sstate->isConnected())
	break;
      
      std::vector<std::string> * sdata = sstate->getCommandsToProcess();
      std::string remaining = sstate->getRemainingMessage();
      
      
      std::cout<<"\nNew message from: "<<socketID<<std::endl;
      for(int i = 0;i<sdata->size();i++)
	{
	  std::cout<<(*sdata)[i]<<std::endl;
	}
      std::cout<<"REMAINING\n"<<remaining<<std::endl;
    }
  
  return 0;
}

//ENTRY POINT

int main(int argc, char ** argv)
{
  MasterController *masterController = new MasterController(2112);
  std::thread connectionThread(&MasterController::startServer,masterController);//implicit this parameter

  connectionThread.join();
  
  return 0;
}

 
