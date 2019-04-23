
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
}

void SpreadsheetInstance::load()
{
  try{

    std::ifstream in(pathToSaveFile);

    if(!in.is_open())
      return;

    std::cout<<"OPENING "<<pathToSaveFile<<"\n";
    
    nlohmann::json test;
    in>>test;
    
    in.close();
    for(auto it = test["cells"].begin();it!=test["cells"].end();it++)
      {
	nlohmann::json a = *it;
	std::string cell = a["cell"];
	std::string value = a["value"];
	(*spreadsheetData)[cell]=value;
	
	for(auto de = a["history"].begin();de!=a["history"].end();de++)
	  {
	    CellState * hist = new CellState(cell,(std::string)*de);
	    if(!(*revertStack)[cell])
	      (*revertStack)[cell]=new std::vector<CellState*>();

	    (*revertStack)[cell]->push_back(hist);
	    
	  }
      }
    
    for(auto it = test["undo"].begin();it!=test["undo"].end();it++)
      {
	nlohmann::json a = *it;
	CellState * item = new CellState((std::string)a["cell"],(std::string)a["value"]);

	undoStack->push_back(item);
      }


    /*    for(auto it = undoStack->begin();it!=undoStack->end();it++)
      {
	CellState * item = *it;
	std::cout<<"["<<item->getCell()<<", "<<item->getValue()<<std::endl;
	}*/
    
    for(auto it =test["dee"].begin();it!=test["dee"].end();it++)
      {
	for(auto de = (*it)["dee"].begin();de!=(*it)["dee"].end();de++)
	  {
	    dependencyGraph->AddDependency(*de,*it);
	  }
      }    
  }catch(nlohmann::detail::parse_error e){}
  catch(nlohmann::detail::type_error){}
  catch(nlohmann::detail::out_of_range){}
  catch(...){}
}

void SpreadsheetInstance::saveToDisk()
{
  std::cout<<"SAVING "<<pathToSaveFile;
  try{
    nlohmann::json save;

    if(!spreadsheetData)
      {
	return;
      }
    
    //Save cells
    for(std::map<std::string,std::string>::iterator it = spreadsheetData->begin(); it != spreadsheetData->end(); it++)
      {
	std::string cell = it->first;
	std::string value = it->second;
	std::vector<CellState*> * cellHistory = (*revertStack)[cell];
	if(!cellHistory)
	  continue;
	
	save["cells"][cell]["value"]=value;
	save["cells"][cell]["cell"]=cell;
	for(std::vector<CellState*>::iterator hist = cellHistory->begin();hist!=cellHistory->end();hist++)
	  {
	    std::string newVal = (*hist)->getValue();
	    save["cells"][cell]["history"].push_back(newVal);
	  }
      }

    
    //Save dependencies
    std::map<std::string,std::vector<std::string>*> * dep = dependencyGraph->getDependents();
    std::map<std::string,std::vector<std::string>*> * dee = dependencyGraph->getDependees();

    if(!dep || !dee)
      return;

    for(std::map<std::string,std::vector<std::string>*>::iterator it = dep->begin();it!=dep->end();it++)
      {
	std::string cell = it->first;
	if(cell=="")
	  continue;
	save["dep"][cell]["cell"]=cell;
	std::vector<std::string> * cellDep = it->second;
	for(std::vector<std::string>::iterator i = cellDep->begin();i!=cellDep->end();i++)
	  {
	    std::string val = *i;
	    if(val!="")
	      save["dep"][cell]["dep"].push_back(val);
	  }
	
      }


    for(std::map<std::string,std::vector<std::string>*>::iterator it = dee->begin();it!=dee->end();it++)
      {
	std::string cell = it->first;
	if(cell=="")
	  continue;
	save["dee"][cell]["cell"]=cell;
	std::vector<std::string> * cellDee = it->second;

	if(!cellDee)
	  return;
	
	for(std::vector<std::string>::iterator i = cellDee->begin();i!=cellDee->end();i++)
	  {
	    std::string val = *i;
	    if(val!="")
	      save["dee"][cell]["dee"].push_back(val);
	  }
	
      }


    //Save undo

    for(std::vector<CellState*>::iterator it = undoStack->begin();it!=undoStack->end();it++)
      {
	CellState * current = *it;

	if(!current)
	  continue;
	
	std::string cell = current->getCell();
	std::string value = current->getValue();

	save["undo"].push_back({{"cell",cell},{"value",value}});
	
      }
      std::cout<<"afterUndo";
    std::ofstream o(pathToSaveFile);
    o<<save.dump(2);
    o.close();
  }catch(nlohmann::detail::parse_error e){}
  catch(nlohmann::detail::type_error){}
  catch(nlohmann::detail::out_of_range){}
  catch(...){}

  std::cout<<"... done.\n";
}


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

      bool edited = false;
      
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
		       edited = true;

		       std::vector<std::string> * dep = new std::vector<std::string>();
		       for(nlohmann::json::iterator di = echoMsg["dependencies"].begin();di!=echoMsg["dependencies"].end();di++)
			 {
			   dep->push_back(*di);
			 }

		       savingMtx->lock();
		       bool successfulEdit = edit((std::string)echoMsg["cell"],(std::string)echoMsg["value"],dep);
		       savingMtx->unlock();

		       if(std::find(dep->begin(),dep->end(),(std::string)echoMsg["cell"])!=dep->end())
			  successfulEdit = false;
		       
		       if(!successfulEdit)
			 {
			   nlohmann::json circularResponse;

			   circularResponse["type"]="error";
			   circularResponse["code"]="2";
			   circularResponse["source"]=(std::string)echoMsg["cell"];
			   sstate->socketSendData(circularResponse.dump(0));
			 }
		       
		       //SEND TO ALL CLIENTS
		       if(successfulEdit)
			 {
			   for(std::map<int,SocketState*>::iterator sendIter = connectedClients->begin();
			       sendIter!=connectedClients->end();
			       sendIter++)
			     {
			       sendIter->second->socketSendData(response.dump(0));
			     }
			   
			   
			   //		       sstate->socketSendData(response.dump(0));
			   std::cout<<"RESPONDED WITH: \n"<<response.dump(1)<<std::endl;}
		     }else if(echoMsg["type"]=="undo")
		     {
		       edited = true;
		       savingMtx->lock();
		       undo();
		       savingMtx->unlock();
		     }else if(echoMsg["type"]=="revert")
		     {
		       edited = true;
		       std::string cell = echoMsg["cell"];
		       savingMtx->lock();
		       revert(cell);
		       savingMtx->unlock();
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
	  savingMtx->lock();
	  connectedClients->erase(*removeIt);
	  savingMtx->unlock();
	}
      savingMtx->lock();
      if(edited)
	saveToDisk();
      savingMtx->unlock();
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

  
  (*spreadsheetData)[cell]=value;
  //Add in all of the dependencies to the Dependency Graph
  std::vector<std::string> *oldDependencies = dependencyGraph->GetDependents(cell); 

for(std::vector<std::string>::iterator it = oldDependencies->begin();it!=oldDependencies->end();it++)
	{
	  std::string current = *it;
	  dependencyGraph->RemoveDependency(cell,current);
	}
 
 for(std::vector<std::string>::iterator it = dependencies->begin();it!=dependencies->end();it++)
   {
     
     std::string current = *it;      
     dependencyGraph->AddDependency(cell, current);
     
   }
  
  //Check if Circular
  if(dependencyGraph->IsCircular(cell))
    {
      for(std::vector<std::string>::iterator it = dependencies->begin();it!=dependencies->end();it++)
	{
	  std::string current = *it;
	  dependencyGraph->RemoveDependency(cell, current);
	}
	(*spreadsheetData)[cell]=oldValue;

	for(std::vector<std::string>::iterator it = oldDependencies->begin();it!=oldDependencies->end();it++)
	{
	  std::string current = *it;
	  dependencyGraph->AddDependency(cell,current);
	}

	return false;

	
	
  /*dependencyGraph->ReplaceDependents(cell, dependencies);

  if(dependencyGraph->IsCircular(cell))
    {
      std::cout<<"CIRCULAR DEPENDENCY!"<<std::endl;
      dependencyGraph->ReplaceDependents(cell, oldDependencies);
      (*spreadsheetData)[cell]=oldValue;
      return false;
      }*/

  }


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

  //  delete dependencies;
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
  
