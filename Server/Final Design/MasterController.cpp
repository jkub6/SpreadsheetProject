
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
  this->threadpool = new std::map<int,std::thread*>();
  this->connectionListener = new ConnectionListener(port,this);
  this->spreadsheetController = new SpreadsheetController(this->threadpool);

  this->running = true;
}
MasterController::~MasterController()
{
  delete connectionListener;
  delete spreadsheetController;
  delete threadpool;
  std::cout<<"MasterController Deconstructed"<<std::endl;
}

void MasterController::sendJsonError(SocketState * sstate)
{
  nlohmann::json badLoginResponse;
  badLoginResponse["type"]="error";
  badLoginResponse["code"]=1;
  badLoginResponse["source"]="";
  
  sstate->socketSendData(badLoginResponse.dump(0));
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

  
  std::vector<std::string> * sdata;

  //********************
  // Get Username and password and validate
  //*********************

  bool userValidated = false;

  std::string desiredSheet = "";
  
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
	    std::cout<<"JSON:\n"<<newCommand.dump()<<std::endl;
	    if(newCommand.at("type") == "open")
	      {
		std::cout<<"TYPE "<<newCommand["type"]<<std::endl;
		std::string username = newCommand["username"];
		std::string password = newCommand["password"];
		desiredSheet= newCommand["name"];
		
		std::string::size_type usernameSpaceLocation = username.find(" ");
		std::string::size_type passwordSpaceLocation = username.find(" ");
		
		if(username=="" || password=="" ||usernameSpaceLocation!=std::string::npos||passwordSpaceLocation!=std::string::npos)
		  {
		    std::cout<<"Username or Password is empty or contains spaces."<<std::endl;
		    sendJsonError(sstate);
		    continue;
		  }

		if(Utilities::validateUser(username,password)&&desiredSheet!="")
		  {
		    userValidated=true;
		  }else{
		  //generate failed response
		  std::cout<<"Failed to Validate '"<<username<<"'. Sending Error Code...";
		  sendJsonError(sstate);
		}
		
	      }else if(newCommand.at("type")=="Login")
	      {
		std::cout<<"ADMIN CONNECTED"<<std::endl;
		//todo
		sstate->socketSendData("success");
	      }else if(newCommand.at("type")=="UserList")
	      {
		std::map<std::string,std::string> *userList = Utilities::getUserList();
		nlohmann::json response;
		response["type"]="user";

		for(std::map<std::string,std::string>::iterator it = userList->begin(); it!=userList->end();it++)
		  {
		    response["sheets"].push_back(it->first);
		  }
		
		std::cout<<"SPREAD\n"<<response.dump(1);
		
		sstate->socketSendData(response.dump(0));
	      }else if(newCommand["type"]=="CreateUser")
	      {
		std::cout<<"CREATE USER"<<std::endl;
	      }else if(newCommand["type"]=="DeleteUser")
	      {
		std::cout<<"DELETE USER"<<std::endl;
	      }else if(newCommand["type"]=="CreateSpread")
	      {
		std::cout<<"CREATE SPREAD"<<std::endl;
	      }else if(newCommand["type"]=="DeleteSpread")
	      {
		std::cout<<"DELETE SPREAD"<<std::endl;	
	      }else if(newCommand["type"]=="SpreadsheetList")
	      {
		std::cout<<"SPREADSHEET LIST"<<std::endl;
		std::vector<std::string> * list = Utilities::getSpreadsheetList();

		nlohmann::json response;
		response["type"]="spread";

		for(std::vector<std::string>::iterator it = list->begin();it!=list->end();it++)
		  {
		    response["Sheets"].push_back(*it);
		  }

		std::cout<<"Spread List:\n"<<response.dump(1);
		sstate->socketSendData(response.dump(0));
	      }
	    
	  }catch (nlohmann::detail::parse_error e)
	    {
	      std::cout<<"JSON Parse Error"<<std::endl;
	      sendJsonError(sstate);
	    }catch(nlohmann::detail::type_error)
	    {
	      std::cout<<"Bad Token in JSON"<<std::endl;
	      sendJsonError(sstate);
	    }catch(nlohmann::detail::out_of_range)
	    {
	      std::cout<<"Bad Token in JSON"<<std::endl;
	      sendJsonError(sstate);
	    }
	}

      std::this_thread::sleep_for(std::chrono::milliseconds(50));

      
    }

  

  
  //********************
  //TRANSFER SOCKETSTATE TO SPREADSHEETCONTROLLER
  //**************************

  this->spreadsheetController->connectedClient(sstate,desiredSheet);
  
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

  delete masterController;
  
  std::cout<<"\nSERVER SUCCESSFULLY SHUTDOWN\n-------------------------------\n"<<std::endl;
  
  return 0;
}

 
