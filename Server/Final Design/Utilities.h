
#ifndef SPREADSHEET_UTILITIES_H
#define SPREADSHEET_UTILITIES_H

#include <string>
#include <vector>



#include "Utilities.h"
#include <mutex>
#include <sys/socket.h>
#include "SocketState.h"
#include <map>


class Utilities{

 public:
  static std::string hash(std::string input);
  
  static void sendMessage(SocketState * sstate, std::string message);
 
  
  static std::string receiveMessage(SocketState * sstate);
  
  static std::vector<std::string> *getSpreadsheetList();

  static bool validateUser(std::string username, std::string password);
  
  static void newUser(std::string name, std::string password);

  static void shutdown();
  
  static void newSpreadsheetInList(std::string name);
  
 private:
  static std::map<std::string,std::string> * getUserList();

  static void saveUsers();

  static void saveSpreadsheetList();

  static std::vector<std::string>* Tokenize(std::string *input);

  static std::vector<std::string> *spreadsheetList;

  static std::map<std::string,std::string> *userList;
  
  static std::mutex *spreadSheetListMtx;

  static std::mutex *userListMtx;
};

#endif
