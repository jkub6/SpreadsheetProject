

#include "SpreadsheetController.h"
#include "SpreadsheetInstance.h"
#include <vector>
#include <string>
#include <iostream>
#include "SocketState.h"
#include <map>
#include <thread>
#include "nlohmann/json.hpp"
#include "Utilities.h"


SpreadsheetController::SpreadsheetController(std::map<int,std::thread*> *threadpool)
{
  this->running = true;
  this->threadpool = threadpool;
  this->spreadsheets = new std::map<std::string, SpreadsheetInstance*>();
  this->spreadsheetTitles = Utilities::getSpreadsheetList();

  loadSpreadsheets();
}

SpreadsheetController::~SpreadsheetController()
{
  std::cout<<"SpreadsheetController deconstructed..."<<std::endl;
  if(this->spreadsheets)
    delete this->spreadsheets;
  //TODO
}

void SpreadsheetController::loadSpreadsheets()
{
  for(std::string str : *spreadsheetTitles)
    (*spreadsheets)[str]=new SpreadsheetInstance("./save/"+str);
}

void SpreadsheetController::connectedClient(SocketState * sstate, std::string desiredSpreadsheet)
{
  //*********
  //See if spreadsheet requested exists
  //****************

  spreadsheetTitles = Utilities::getSpreadsheetList();
  
  bool exists = false;

  for(std::map<std::string,SpreadsheetInstance *>::iterator it = spreadsheets->begin();it!=spreadsheets->end();it++)
    {
      if(it->first == desiredSpreadsheet)
	{
	  exists = true;
	  break;
	}
    }

  //***********************
  //Place client in spreadsheet if exists, or create new spreadsheet
  //**************************
  
  
  if(exists)
    {
      (*spreadsheets)[desiredSpreadsheet]->newClientConnected(sstate);
    }else
    {
      Utilities::newSpreadsheetInList(desiredSpreadsheet);
      (*spreadsheets)[desiredSpreadsheet]=new SpreadsheetInstance("./save/"+desiredSpreadsheet);
      (*spreadsheets)[desiredSpreadsheet]->newClientConnected(sstate);
    }
}
void SpreadsheetController::shutdown()
{
  running = false;

  for(std::map<std::string,SpreadsheetInstance *>::iterator it = spreadsheets->begin();it!=spreadsheets->end();it++)
    {
      it->second->shutdown();
      delete it->second;
    }

  std::cout<<"SPREADSHEETCONTROLLER SUCCESSFULLY SHUTDOWN...\n"<<std::endl;
  //TODO
}

void SpreadsheetController::loadSpreadsheet()
{
  //TODO
}
