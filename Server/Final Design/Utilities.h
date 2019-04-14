
#ifndef SPREADSHEET_UTILITIES_H
#define SPREADSHEET_UTILITIES_H

#include <string>
#include <vector>



#include "Utilities.h"
#include <mutex>
#include <sys/socket.h>
#include "SocketState.h"


class Utilities{

 public:
  static std::string hash(std::string input);
  
  static void sendMessage(SocketState * sstate, std::string message);
  
  static std::vector<std::string>* Tokenize(std::string *input);
  
  
  static std::string receiveMessage(SocketState * sstate);
  
  static std::vector<std::string> *getSpreadsheetList();

  static void shutdown();
  
  static void newSpreadsheetInList(std::string name);
 private:
  static std::vector<std::string> *spreadsheetList;
  static std::mutex *spreadSheetListMtx;
};

#endif
