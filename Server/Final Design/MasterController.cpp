
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
	    if(newCommand.at("type") == "open")
	      {
		std::string username = newCommand["username"];
		std::string password = newCommand["password"];
		desiredSheet= newCommand["name"];
		
		std::string::size_type usernameSpaceLocation = username.find(" ");
		std::string::size_type passwordSpaceLocation = username.find(" ");
		
		if(username=="" || password=="" ||usernameSpaceLocation!=std::string::npos||passwordSpaceLocation!=std::string::npos)
		  {
		    std::cout<<"Username or Password is empty or contains spaces."<<std::endl;
		    nlohmann::json badLoginResponse;
		    badLoginResponse["type"]="error";
		    badLoginResponse["code"]=1;
		    badLoginResponse["source"]="";
		    
		    sstate->socketSendData(badLoginResponse.dump(0));
		    //send error message
		    continue;
		  }

		if(Utilities::validateUser(username,password)&&desiredSheet!="")
		  {
		    userValidated=true;
		  }else{
		  //generate failed response
		  std::cout<<"Failed to Validate '"<<username<<"'. Sending Error Code...";
		  nlohmann::json badLoginResponse;
		  badLoginResponse["type"]="error";
		  badLoginResponse["code"]=1;
		  badLoginResponse["source"]="";
		  
		  sstate->socketSendData(badLoginResponse.dump(0));
		  std::cout<<"DONE\n";
		}
		
	      }
	    
	  }catch (nlohmann::detail::parse_error e)  {
	    //send bad response again. sloppy code
	    nlohmann::json badLoginResponse;
	    std::cout<<"BAD PARSE"<<std::endl;
	    badLoginResponse["type"]="error";
	    badLoginResponse["code"]=1;
	    badLoginResponse["source"]="";
	    
	    sstate->socketSendData(badLoginResponse.dump(0));
	  }catch(nlohmann::detail::type_error){
	    std::cout<<"Bad Token in JSON"<<std::endl;
	    nlohmann::json badLoginResponse;
	    badLoginResponse["type"]="error";
	    badLoginResponse["code"]=1;
	    badLoginResponse["source"]="";
	    
	    sstate->socketSendData(badLoginResponse.dump(0));
	  }
	}
    }

  

  
  //********************
  //TRANSFER SOCKETSTATE TO SPREADSHEETCONTROLLER
  //**************************

  this->spreadsheetController->connectedClient(sstate,desiredSheet);

  /*
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
	  catch(nlohmann::detail::type_error){}
	  
	}
      
      //free sdata MUST CALL
      delete sdata;
      //sleep thread


      //chrono sleep, prevents 100% processor utilization
      std::this_thread::sleep_for(std::chrono::milliseconds(50));
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
      
      
      }
  std::cout<<"Client: "<<sstate->getID()<<" disconnected."<<std::endl;
  
  threadpool->erase(sstate->getID());

*/
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

 
