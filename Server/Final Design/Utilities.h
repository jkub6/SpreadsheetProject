
#ifndef SPREADSHEET_UTILITIES_H
#define SPREADSHEET_UTILITIES_H

#include <string>
#include <vector>



#include "Utilities.h"
#include <mutex>
#include <sys/socket.h>


class Utilities{

 public:
  
  static void sendMessage(int socketID, std::string message);
  
  static std::vector<std::string>* Tokenize(std::string *input);
  
  
  static std::vector<std::string>* receiveMessage(int socketID, int *bytesRead, std::string * remainingMessage);
  
  static std::vector<std::string> *getSpreadsheetList();

    static void newSpreadsheetInList(std::string name);
 private:
  static std::vector<std::string> *spreadsheetList;
  static std::mutex *spreadSheetListMtx;

  
};

#endif
