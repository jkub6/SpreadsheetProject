


#include "Utilities.h"
#include <string>
#include <sys/socket.h>
#include <unistd.h>  
#include <netinet/in.h>
#include <vector>
#include <iostream>
#include <fstream>
#include <mutex>
#include "SocketState.h"
#include "sha256.h"
#include <map>
#include <algorithm>

std::vector<std::string> *Utilities::spreadsheetList;

std::mutex * Utilities::spreadSheetListMtx;

std::mutex * Utilities::userListMtx;

std::map<std::string,std::string> *Utilities::userList;

void Utilities::disconnectSocket(int socketID)
{
  close(socketID);
}

void Utilities::shutdown()
{
  //TODO
  if(Utilities::spreadSheetListMtx)
    delete Utilities::spreadSheetListMtx;

  if(Utilities::userListMtx)
    delete Utilities::userListMtx;

  if(Utilities::userList)
    delete Utilities::userList;

  if(Utilities::spreadsheetList)
    delete Utilities::spreadsheetList;
  
  std::cout<<"\nUTILITIES SUCCESSFULLY SHUTDOWN..\n"<<std::endl;
}

std::string Utilities::hash(std::string input)
{
  SHA256 sha256;

  return sha256(input);
}

void Utilities::sendMessage(SocketState * sstate, std::string message)
{
  int socketID = sstate->getID();

  std::string newMsg = message+'\n'+'\n';

  send(socketID, newMsg.c_str(), newMsg.length() , 0 );  
}

std::string Utilities::receiveMessage(SocketState * sstate)
{
  int socketID = sstate->getID();
  int bufferLength = 1024;
  char receiveBuffer[bufferLength];

  int bytesRead = read(socketID,receiveBuffer,bufferLength);


  if(bytesRead<=0)//client disconnected
    {
      sstate->setConnected(false);
      return "";
    }

  
  std::string rawString((char*)receiveBuffer,bytesRead);
  
  return rawString;
}


bool Utilities::validateUser(std::string username, std::string password)
{
  Utilities::getUserList();

  Utilities::userListMtx->lock();

  bool validated = false;
  
  //user exists already
  if(Utilities::userList->find(username)!=Utilities::userList->end())
    {

      if((*Utilities::userList)[username]==Utilities::hash(password))
	{
	  std::cout<<"Existing User: '"<<username<<"' validated."<<std::endl;
	  validated = true;
	}
      
    }else//user doesn't exist... create them
    {
      std::cout<<"New User: '"<<username<<"'."<<std::endl;
      Utilities::newUser(username,password);
      validated = true;
    }
  
  Utilities::userListMtx->unlock();
  return validated;
}

std::map<std::string,std::string> * Utilities::getUserList()
{
  if(Utilities::userList==NULL)
    {
      Utilities::userListMtx = new std::mutex();
      Utilities::userListMtx->lock();

      Utilities::userList = new std::map<std::string,std::string>();
      std::ifstream stream("./data/users");
      std::string word;

      while(stream>>word)
	{
	  std::string username = word;
	  stream>>word;
	  std::string password = word;
	  (*userList)[username]=password;
	}
      
      stream.close();
      Utilities::userListMtx->unlock();
    }

  return userList;
}

void Utilities::removeUser(std::string user)
{
  Utilities::getUserList();

  Utilities::userList->erase(user);
  Utilities::saveUsers();
  
}

void Utilities::newUser(std::string name, std::string password)
{
    Utilities::getUserList();


  //if not in list, add it
  if(Utilities::userList->find(name)==Utilities::userList->end())
    {
      (*Utilities::userList)[name]=Utilities::hash(password);
    }

  std::cout<<"Saving user information to disk...";
  Utilities::saveUsers();
  std::cout<<"DONE!\n";
}

void Utilities::saveUsers()
{
    Utilities::getUserList();

  std::ofstream stream("./data/users");
  
  for(std::map<std::string,std::string>::iterator it = Utilities::userList->begin();it!=Utilities::userList->end();it++)
    {
      stream << it->first<<" "<<it->second<<std::endl;
    }
  
  stream.close();
    
}

void Utilities::saveSpreadsheetList()
{

  if(!Utilities::spreadSheetListMtx)
    Utilities::getSpreadsheetList();

  std::ofstream stream("./data/spreadsheetList");

  for(int i = 0;i<Utilities::spreadsheetList->size();i++)
    {
      stream<<(*Utilities::spreadsheetList)[i]<<"\n";
    }
  
  stream.close();
}

//singleton
std::vector<std::string>* Utilities::getSpreadsheetList()
{
  if(Utilities::spreadsheetList == NULL)
    {
     Utilities::spreadSheetListMtx = new std::mutex();
      Utilities::spreadSheetListMtx->lock();
      Utilities::spreadsheetList = new std::vector<std::string>();
 
      
      std::ifstream stream("./data/spreadsheetList");
      std::string line;
      while(getline(stream,line))
	{
	  Utilities::spreadsheetList->push_back(line);
	}
      Utilities::spreadSheetListMtx->unlock();

      stream.close();
      
      return Utilities::spreadsheetList;
    }

        return Utilities::spreadsheetList;
}

void Utilities::removeSheet(std::string name)
{
  Utilities::getSpreadsheetList();

  Utilities::spreadSheetListMtx->lock();

  std::vector<std::string>::iterator it = std::find(spreadsheetList->begin(),spreadsheetList->end(),name);
  
  Utilities::spreadsheetList->erase(it);
  
  Utilities::spreadSheetListMtx->unlock();
  Utilities::saveSpreadsheetList();
}

void Utilities::newSpreadsheetInList(std::string name)
{
  if(Utilities::spreadsheetList==NULL)
    Utilities::getSpreadsheetList();

  
  Utilities::spreadSheetListMtx->lock();

  if(std::find(Utilities::spreadsheetList->begin(),Utilities::spreadsheetList->end(),name)==Utilities::spreadsheetList->end())
    {
      Utilities::spreadsheetList->push_back(name);      
    }

  


  Utilities::saveSpreadsheetList();
  
  Utilities::spreadSheetListMtx->unlock();
}
