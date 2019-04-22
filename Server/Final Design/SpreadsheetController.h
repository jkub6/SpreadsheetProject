
#ifndef SPREADSHEET_CONTROLLER_H
#define SPREADSHEET_CONTROLLER_H

#include "SpreadsheetInstance.h"
#include "Utilities.h"
#include <string>
#include <vector>
#include "SocketState.h"
#include <map>
#include <thread>

class SpreadsheetController
{
 public:
  SpreadsheetController(std::map<int,std::thread*> *threadpool);
  ~SpreadsheetController();

  std::vector<SpreadsheetInstance*> spreadSheets;

  void connectedClient(SocketState * sstate, std::string desiredSpreadsheet);
  void shutdown();
  void createSheet(std::string name);
  void deleteSheet(std::string name);
  
 private:
  std::vector<std::string> *spreadsheetTitles;
  std::map<int,std::thread*> * threadpool;
  std::map<std::string,SpreadsheetInstance *> *spreadsheets;
  bool running;
  void loadSpreadsheets();

  
};


#endif
