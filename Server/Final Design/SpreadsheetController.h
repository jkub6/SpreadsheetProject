
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

  
 private:
  std::map<int,std::thread*> * threadpool;
  void loadSpreadsheet();
  bool running;
};


#endif
