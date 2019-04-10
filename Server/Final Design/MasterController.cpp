
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
}
void MasterController::shutdown(){
  //TODO
}


int MasterController::newClientConnected(int socketID)
{
  //***************************
  //send list of spreadsheets:
  //******************************
  std::vector<std::string> *list = Utilities::getSpreadsheetList();
  
  nlohmann::json jsonObject;

  SocketState * sstate = new SocketState(socketID);

  jsonObject["type"]="list";

  for(int i = 0;i<list->size();i++)
    {
      jsonObject["spreadsheet"].push_back((*list)[i]);
    };


  sstate->socketSendData(jsonObject.dump(0));
  //  Utilities::sendMessage(socketID, jsonObject.dump(0));

  //***********************
  //AWAIT RESPONSE
  //**********************
  
  std::cout<<"JSON SENT: \n"<<jsonObject.dump(0)<<std::endl;;//jsonObject.dump();




  while(sstate->isConnected())
    {
      std::cout<<"AWAITING"<<std::endl;
      sstate->socketAwaitData();

      if(!sstate->isConnected())
	break;
      
      std::vector<std::string> * sdata = sstate->getCommandsToProcess();
      std::string remaining = sstate->getBuffer();

      nlohmann::json newCommand;
      
      if(sdata->size()>0)
	newCommand = nlohmann::json::parse((*sdata)[0]);

      std::cout<<"HI: "<<newCommand["hi"]<<std::endl;
      
    }
  std::cout<<"Client: "<<sstate->getID()<<" disconnected."<<std::endl;
  
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

 
