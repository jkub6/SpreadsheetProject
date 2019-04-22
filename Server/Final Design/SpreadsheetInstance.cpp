
#include <thread>
#include "SpreadsheetInstance.h"
#include <map>
#include "SocketState.h"
#include "nlohmann/json.hpp"
#include <iostream>
#include <vector>
#include <string>
#include <chrono>
#include <ostream>
#include <mutex>

SpreadsheetInstance::SpreadsheetInstance(std::string pathToSave)
  {
    this->pathToSaveFile = pathToSave;
    this->dependencyGraph = new DependencyGraph();
    this->connectedClients = new std::map<int,SocketState *>();
    this->data = new std::map<std::string, std::string>();
    this->usersMtx = new std::mutex();
    this->savingMtx = new std::mutex();
    
    running = true;
    load();
    //Start new thread for infinite loop

     this->sheetThread = new std::thread(&SpreadsheetInstance::loop,this);
  }
SpreadsheetInstance::~SpreadsheetInstance()
{
  savingMtx->lock();
  running = false;
  savingMtx->unlock();
  delete connectedClients;
  delete dependencyGraph;
  delete data;
  delete usersMtx;
  delete savingMtx;
  
  std::cout<<"SpreadsheetInstance for: "<<this->pathToSaveFile<<" deconstructed."<<std::endl;
  //TODO
}

void SpreadsheetInstance::load()
{
  
}

void SpreadsheetInstance::saveToDisk()
{}


//MAster loop of spreadsheet instance
void SpreadsheetInstance::loop()
{
  while(true)
    {
      savingMtx->lock();
      if(!running)
	{
	  savingMtx->unlock();
	  break;
	}
      savingMtx->unlock();


      //Iterate through each client, and process commands:

      std::vector<int> toRemove;

      for(std::map<int,SocketState*>::iterator it = connectedClients->begin();
	  it!=connectedClients->end();
	  it++)
	{
	  SocketState * sstate = it->second;

	  if(!sstate->isConnected())
	    {
	      toRemove.push_back(it->first);
	      continue;
	    }

	  std::vector<std::string> * commandsToProcess = sstate->getCommandsToProcess();

	  //Loop through all the commands and execute them.
	  for(std::vector<std::string>::iterator cmdIterator = commandsToProcess->begin();
	      cmdIterator!=commandsToProcess->end();
	      cmdIterator++)
	    {

	      std::string s = *cmdIterator;
	      std::cout<<"MESSAGE RECEIVED: "<<s<<std::endl;
	       nlohmann::json echoMsg;
	  
	       try
		 {
		   echoMsg = nlohmann::json::parse(s);
		   
		   if(echoMsg["type"]=="edit")
		     {
		       nlohmann::json response;
		       response["type"]="full send";
		       response["spreadsheet"][(std::string)echoMsg["cell"]]=echoMsg["value"];

		       //SEND TO ALL CLIENTS
		       for(std::map<int,SocketState*>::iterator sendIter = connectedClients->begin();
			   sendIter!=connectedClients->end();
			   sendIter++)
			 {
			   sendIter->second->socketSendData(response.dump(0));
			 }

		       
		       //		       sstate->socketSendData(response.dump(0));
		       std::cout<<"RESPONDED WITH: \n"<<response.dump(1)<<std::endl;
		     }else if(echoMsg["type"]=="undo")
		     {
		       //TODO UNDO
		     }else if(echoMsg["type"]=="revert")
		     {
		       //TODO REVERT
		     }
		   
		 }
	       catch(nlohmann::detail::parse_error e){}
	       catch(nlohmann::detail::type_error){}
	       catch(nlohmann::detail::out_of_range){}
	       
	    }

	  delete commandsToProcess;
	 
	  
	}
      
      std::this_thread::sleep_for(std::chrono::milliseconds(50));
      //****************
      //REMOVE SOCKET STATES THAT ARE DISCONNECTED:
      //*****************
	
      for(std::vector<int>::iterator removeIt = toRemove.begin();
	  removeIt!=toRemove.end();
	  removeIt++)
	{
	  connectedClients->erase(*removeIt);
	}
    }
}

void SpreadsheetInstance::newClientConnected(SocketState * sstate)
{

  
  //**********
  //FAKE FULL SEND TEST DELETE LATER
  //***************
  nlohmann::json fullSend;
  
  fullSend["type"]="full send";
  fullSend["spreadsheet"]=nlohmann::json::object();

  fullSend["spreadsheet"]["a1"]="5";
  fullSend["spreadsheet"]["b1"]="=a1*5";
  
  sstate->socketSendData(fullSend.dump(0));
   
  usersMtx->lock();
  (*connectedClients)[sstate->getID()]=sstate;
  usersMtx->unlock();
   
  //TODO
}

void SpreadsheetInstance::disconnectAllClients()
{
  for(std::map<int,SocketState *>::iterator it = connectedClients->begin();it!=connectedClients->end();it++)
    {
      it->second->setConnected(false);
    }
}

void SpreadsheetInstance::shutdown()
{
  savingMtx->lock();
  running = false;
  saveToDisk();
  savingMtx->unlock();

  sheetThread->join();
}
  
