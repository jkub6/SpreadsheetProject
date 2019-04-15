
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
#include <map>
#include <chrono>

MasterController::MasterController(int port)
{
  this->port = port;
  this->connectionListener = new ConnectionListener(port,this);
  this->spreadsheetController = new SpreadsheetController();
  this->threadpool = new std::map<int,std::thread*>();
  this->running = true;
}
MasterController::~MasterController()
{
  delete connectionListener;
  delete spreadsheetController;

 
  delete threadpool;

  
}


void MasterController::startServer()
{
  this->connectionListener->beginListeningForClients();
}
void MasterController::shutdown(){
    std::cout<<"\n\n********************\n\n"<<"SHUTTING DOWN"<<std::endl;
  this->running = false;
  Utilities::shutdown();
  connectionListener->shutdownListener();
  spreadsheetController->shutdown();
}


int MasterController::newClientConnected(int socketID)
{
  std::cout<<"NEW CONNECT"<<std::endl;
  //***************************
  //send list of spreadsheets:
  //******************************
  std::vector<std::string> *list = Utilities::getSpreadsheetList();

  std::cout<<"SH LIST"<<std::endl;
  
  nlohmann::json jsonObject;

  SocketState * sstate = new SocketState(socketID);

  jsonObject["type"]="list";

  for(int i = 0;i<list->size();i++)
    {
      jsonObject["spreadsheets"].push_back((*list)[i]);
    };


  sstate->socketSendData(jsonObject.dump(0));
  
    std::cout<<"\nSent {SocketID: "<<sstate->getID()<<"} Spreadsheet List Successfully."<<std::endl;
    
    

  //***********************
  //AWAIT RESPONSE
  //**********************
  
  //start thread for listening:
  (*threadpool)[socketID]=new std::thread(&SocketState::socketAwaitData,sstate);
  
//  threadpool->push_back(new std::thread(&SocketState::socketAwaitData,sstate)
  
  
  /*    while(sstate->isConnected())
  {
      sstate->socketAwaitData();

      if(!sstate->isConnected())
      break;;*/


  std::cout<<"THREADPOOL:" <<threadpool->size()<<std::endl;
  
  std::vector<std::string> * sdata;

  //********************
  // Get Username and password and validate
  //*********************

  bool userValidated = false;

  
  while(!userValidated)
    {
      if(!sstate->isConnected())
	break;
      
      std::string userRequest = sstate->getSingleMessage();// getCommandsToProcess();

      nlohmann::json newCommand;
      
      if(userRequest!="")
	{
	  try{
	    newCommand = nlohmann::json::parse(userRequest);
	    if(newCommand.at("type") == "open")
	      {
		std::string username = newCommand["username"];
		std::string password = newCommand["password"];

		if(Utilities::validateUser(username,password))
		  {
		    userValidated=true;
		  }else{
		  //generate failed response
		  nlohmann::json badLoginResponse;
		  badLoginResponse["type"]="error";
		  badLoginResponse["code"]=1;
		  badLoginResponse["source"]="";
		  
		  sstate->socketSendData(badLoginResponse.dump(0));
		}
		
	      }
	    
	  }catch (nlohmann::detail::parse_error e){
	    //send bad response again. sloppy code
	    nlohmann::json badLoginResponse;
	    badLoginResponse["type"]="error";
	    badLoginResponse["code"]=1;
	    badLoginResponse["source"]="";
	    
	    sstate->socketSendData(badLoginResponse.dump(0));
	  }
	}
      
      std::this_thread::sleep_for(std::chrono::milliseconds(10));
    }

  //**********
  //FAKE FULL SEND TEST DELETE LATER
  //***************
  nlohmann::json fullSend;

  fullSend["type"]="full send";
  fullSend["spreadsheet"]=nlohmann::json::object();

  sstate->socketSendData(fullSend.dump(0));
  
  std::cout<<"Full send sent"<<std::endl;
  //**************
  //FAKE SERVER LOOP
  //**************
  
  while(running && userValidated)
    {
      if(!sstate->isConnected())
	break;

      sdata = sstate->getCommandsToProcess();
      
      for(int i = 0;i<sdata->size();i++)
	{
	  std::string s = (*sdata)[i];
	  
	  std::cout<<"New Message: \n["<<s<<"]\n"<<std::endl;

	  nlohmann::json echoMsg;
	  
	  try
	    {
	      echoMsg = nlohmann::json::parse(s);

	      if(echoMsg["type"]=="edit")
		{
		  nlohmann::json response;
		  response["type"]="full send";
		  response["spreadsheet"][(std::string)echoMsg["cell"]]=echoMsg["value"];

		  sstate->socketSendData(response.dump(0));
		  std::cout<<"\n\nRESPONDED WITH: \n"<<response.dump(1)<<std::endl;
		}
	      
	    }catch (nlohmann::detail::parse_error e){}
	  
	}
      
      //free sdata MUST CALL
      delete sdata;
      //sleep thread


      //chrono sleep, prevents 100% processor utilization
      std::this_thread::sleep_for(std::chrono::milliseconds(1000));
    }
  

  //      std::string remaining = sstate->getBuffer();

      /*    nlohmann::json newCommand;

      try{
      if(sdata->size()>0)
	newCommand = nlohmann::json::parse((*sdata)[0]);

      }catch (nlohmann::detail::parse_error e){}
      
      std::cout<<"HI: "<<newCommand["hi"]<<std::endl;*/
  /*  std::cout<<"\nNew Message From: "<<sstate->getID()<<".\nFull messages:\n"<<std::endl;
      for(int i = 0;i<sdata->size();i++)
	{
	  std::cout<<"["<<(*sdata)[i]<<"]\n";
	}

      std::cout<<"Remaining Message:\n\n"<<remaining;
      
      
      }*/
  std::cout<<"Client: "<<sstate->getID()<<" disconnected."<<std::endl;
  
  threadpool->erase(sstate->getID());


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

 
