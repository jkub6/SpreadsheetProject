
#include "CellState.h"
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
#include <utility>

SpreadsheetInstance::SpreadsheetInstance(std::string pathToSave)
  {
    this->pathToSaveFile = pathToSave;
    this->dependencyGraph = new DependencyGraph();
    this->connectedClients = new std::map<int,SocketState *>();
    this->data = new std::map<std::string, std::string>();
    this->usersMtx = new std::mutex();
    this->savingMtx = new std::mutex();

    this->undoStack = new std::vector<CellState *>();
    this->spreadsheetData = new std::map<std::string,std::string>();
    this->revertStack = new std::map<std::string,std::vector<CellState*>*>();

    
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
  delete undoStack;
  delete spreadsheetData;
  delete revertStack;

  
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

		       bool successfulEdit = edit((std::string)echoMsg["cell"],(std::string)echoMsg["value"],NULL);

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
		       undo();
		     }else if(echoMsg["type"]=="revert")
		     {
		       std::string cell = echoMsg["cell"];
		       revert(cell);
		     }
		   
		 }
	       catch(nlohmann::detail::parse_error e){}
	       catch(nlohmann::detail::type_error){}
	       catch(nlohmann::detail::out_of_range){}
	       
	    }

	  delete commandsToProcess;
	 
	  
	}
      
      std::this_thread::sleep_for(std::chrono::milliseconds(10));
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

void SpreadsheetInstance::revert(std::string cell)
{
  std::string oldValue = (*spreadsheetData)[cell];


  if(!(*revertStack)[cell])
    return;

  if((*revertStack)[cell]->size()<=0)
    return;

  CellState * newValue = (*revertStack)[cell]->back();
  
  (*spreadsheetData)[cell]=newValue->getValue();
  (*revertStack)[cell]->pop_back();
  undoStack->push_back(new CellState(cell,oldValue));
  
  nlohmann::json response;
  response["type"]="full send";
  response["spreadsheet"][cell]=newValue->getValue();
  
  //SEND TO ALL CLIENTS
  for(std::map<int,SocketState*>::iterator sendIter = connectedClients->begin();
      sendIter!=connectedClients->end();
      sendIter++)
    {
      sendIter->second->socketSendData(response.dump(0));
    }
  
}

void SpreadsheetInstance::undo()
{
  if(undoStack->size()<=0)
    return;
  
  CellState * p = undoStack->back();

  std::string oldValue = (*spreadsheetData)[p->getCell()];

  (*revertStack)[p->getCell()]->push_back(new CellState(p->getCell(),oldValue));
  
  (*spreadsheetData)[p->getCell()]=p->getValue();
  
  undoStack->pop_back();
  
  nlohmann::json response;
  response["type"]="full send";
  response["spreadsheet"][p->getCell()]=p->getValue();
  
  //SEND TO ALL CLIENTS
  for(std::map<int,SocketState*>::iterator sendIter = connectedClients->begin();
      sendIter!=connectedClients->end();
      sendIter++)
    {
      sendIter->second->socketSendData(response.dump(0));
    }
}

bool SpreadsheetInstance::edit(std::string cell, std::string value, std::vector<std::string>* dependencies)
{

  std::string oldValue = (*spreadsheetData)[cell];

  std::cout<<"Old Value :" <<oldValue<<std::endl;
  
  (*spreadsheetData)[cell]=value;


  if(!(*revertStack)[cell])
    (*revertStack)[cell]=new std::vector<CellState*>();
  
  (*revertStack)[cell]->push_back(new CellState(cell,oldValue));
  undoStack->push_back(new CellState(cell,oldValue));

  for(std::vector<CellState*>::iterator it = undoStack->begin();it!=undoStack->end();it++)
    {
      std::cout<<"UNDO STACK: "<<(*it)->getCell()<<" ,"<<(*it)->getValue()<<std::endl;
    }

  for(std::map<std::string,std::vector<CellState*>*>::iterator it = revertStack->begin();it!=revertStack->end();it++)
    {
      std::cout<<"REVERT STACK FOR: "<<it->first<<std::endl<<"[ ";
      for(std::vector<CellState*>::iterator in=it->second->begin();in!=it->second->end();in++)
	{
	  std::cout<<(*in)->getValue()<<", ";
	}
      std::cout<<"]\n";
    }
  
  //TODO
  return true;
}

void SpreadsheetInstance::newClientConnected(SocketState * sstate)
{

  
  //**********
  //FAKE FULL SEND TEST DELETE LATER
  //***************
  nlohmann::json fullSend;
  
  fullSend["type"]="full send";
  fullSend["spreadsheet"]=nlohmann::json::object();

  for(std::map<std::string,std::string>::iterator it = spreadsheetData->begin();it!=spreadsheetData->end();it++)
    {
      fullSend["spreadsheet"][(std::string)it->first]=it->second;
    }
  
  //  fullSend["spreadsheet"]["a1"]="5";
  //fullSend["spreadsheet"]["b1"]="=a1*5";
  
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
  
