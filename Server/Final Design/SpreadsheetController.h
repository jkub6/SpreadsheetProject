
#ifndef SPREADSHEET_CONTROLLER_H
#define SPREADSHEET_CONTROLLER_H

#include "SpreadsheetInstance.h"
#include "Utilities.h"
#include <string>
#include <vector>

class SpreadsheetController
{
 public:
  SpreadsheetController();
  ~SpreadsheetController();

  std::vector<SpreadsheetInstance*> spreadSheets;

  void connectedClient(int socketID);
  void shutdown();

  
 private:

  std::vector<std::string> *spreadSheetNameList;

  void loadSpreadsheet();
  
};


#endif
