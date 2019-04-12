
#ifndef SPREADSHEET_CONTROLLER_H
#define SPREADSHEET_CONTROLLER_H

#include "SpreadsheetInstance.h"
#include "Utilities.h"
#include <string>
#include <vector>
#include "SocketState.h"


class SpreadsheetController
{
 public:
  SpreadsheetController();
  ~SpreadsheetController();

  std::vector<SpreadsheetInstance*> spreadSheets;

  void connectedClient(SocketState * sstate, std::string desiredSpreadsheet);
  void shutdown();

  
 private:

  void loadSpreadsheet();
  
};


#endif
