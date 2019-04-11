
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
#include <signal.h>

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
  std::cout<<"\n\n********************\n\n"<<"SHUTTING DOWN"<<std::endl;
  connectionListener->shutdownListener();
  spreadsheetController->shutdown();
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
      jsonObject["spreadsheets"].push_back((*list)[i]);
    };


  sstate->socketSendData(jsonObject.dump(0));
  //  Utilities::sendMessage(socketID, jsonObject.dump(0));

  //***********************
  //AWAIT RESPONSE
  //**********************
  
  std::cout<<"JSON SENT: \n"<<jsonObject.dump(0)<<std::endl;;//jsonObject.dump();




    while(sstate->isConnected())
  {
      sstate->socketAwaitData();

      if(!sstate->isConnected())
	break;;
      
      std::vector<std::string> * sdata = sstate->getCommandsToProcess();
      std::string remaining = sstate->getBuffer();

      /*    nlohmann::json newCommand;

      try{
      if(sdata->size()>0)
	newCommand = nlohmann::json::parse((*sdata)[0]);

      }catch (nlohmann::detail::parse_error e){}
      
      std::cout<<"HI: "<<newCommand["hi"]<<std::endl;*/
      std::cout<<"\nNew Message From: "<<sstate->getID()<<".\nFull messages:\n"<<std::endl;
      for(int i = 0;i<sdata->size();i++)
	{
	  std::cout<<"["<<(*sdata)[i]<<"]\n";
	}

      std::cout<<"Remaining Message:\n\n"<<remaining;
      
      
      }
  std::cout<<"Client: "<<sstate->getID()<<" disconnected."<<std::endl;
  
  return 0;
}



MasterController * masterController;

//Handle Ctrl-C
void sighandler(int sig)
{
  //  std::cout<<"SIGNAL "<<sig<<" caught..."<<std::endl;
  if(sig==2)//ctrl C command
    masterController->shutdown();
}


//ENTRY POINT



int main(int argc, char ** argv)
{


  
  masterController = new MasterController(2112);

  signal(SIGABRT, &sighandler);
  signal(SIGTERM,&sighandler);
  signal(SIGINT,&sighandler);
  signal(SIGPIPE,&sighandler);
  
  std::thread connectionThread(&MasterController::startServer,masterController);//implicit this parameter

  

  connectionThread.join();

  std::cout<<"SERVER SUCCESSFULLY SHUTDOWN"<<std::endl;
  
  return 0;
}

 
