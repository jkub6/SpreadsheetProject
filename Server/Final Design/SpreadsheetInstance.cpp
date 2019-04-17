
#include <thread>
#include "SpreadsheetInstance.h"
#include <map>
#include "SocketState.h"
#include "nlohmann/json.hpp"
#include <iostream>
#include <vector>
#include <string>

SpreadsheetInstance::SpreadsheetInstance(std::string pathToSaveFile)
  {
    this->pathToSaveFile = pathToSaveFile;
    dependencyGraph = new DependencyGraph();
    connectedClients = new std::map<int,SocketState *>();
    running = true;
  }
SpreadsheetInstance::~SpreadsheetInstance()
{
  running = false;
  delete connectedClients;
  delete dependencyGraph;
  std::cout<<"SpreadsheetInstance for: "<<this->pathToSaveFile<<" deconstructed."<<std::endl;
  //TODO
}

void SpreadsheetInstance::newClientConnected(SocketState * sstate)
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

    delete sstate;
  
  //TODO
}
void SpreadsheetInstance::shutdown()
{
  running = false;
  //TODO
}
  
