

#include "SpreadsheetController.h"
#include <vector>
#include <string>
#include <iostream>
#include "SocketState.h"
#include <map>
#include <thread>
#include "nlohmann/json.hpp"

SpreadsheetController::SpreadsheetController(std::map<int,std::thread*> *threadpool)
{
  this->running = true;
  this->threadpool = threadpool;
}

SpreadsheetController::~SpreadsheetController()
{
  //TODO
}

void SpreadsheetController::connectedClient(SocketState * sstate, std::string desiredSpreadsheet)
{


  //**********
  //FAKE FULL SEND TEST DELETE LATER
  //***************
  nlohmann::json fullSend;

  fullSend["type"]="full send";
  fullSend["spreadsheet"]=nlohmann::json::object();

  sstate->socketSendData(fullSend.dump(0));
  
  std::cout<<"Full send sent to: "<<sstate->getID()<<std::endl;
  //**************
  //FAKE SERVER LOOP
  //**************
  std::vector<std::string> * sdata;
  while(running)
    {
      if(!sstate->isConnected())
	break;

      sdata = sstate->getCommandsToProcess();
      
      for(int i = 0;i<sdata->size();i++)
	{
	  std::string s = (*sdata)[i];
	  
	  std::cout<<"Message Received: \n["<<s<<"]\n"<<std::endl;

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
		  std::cout<<"RESPONDED WITH: \n"<<response.dump(1)<<std::endl;
		}
	      
	    }catch (nlohmann::detail::parse_error e){}
	  catch(nlohmann::detail::type_error){}
	  
	}
      
      //free sdata MUST CALL
      delete sdata;
      //sleep thread


      //chrono sleep, prevents 100% processor utilization
      std::this_thread::sleep_for(std::chrono::milliseconds(50));
    }

  std::cout<<"Client: "<<sstate->getID()<<" disconnected."<<std::endl;
  
  this->threadpool->erase(sstate->getID());
  //TODO
}
void SpreadsheetController::shutdown()
{
  running = false;
  std::cout<<"SPREADSHEETCONTROLLER SUCCESSFULLY SHUTDOWN."<<std::endl;
  //TODO
}

void SpreadsheetController::loadSpreadsheet()
{
  //TODO
}
