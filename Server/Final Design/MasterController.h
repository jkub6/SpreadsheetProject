
#ifndef MASTER_CONTROLLER_H
#define MASTER_CONTROLLER_H

#include "ConnectionListener.h"
#include "SpreadsheetController.h"
#include "Utilities.h"
#include <string>

class MasterController
{
 public:
  MasterController(int port);
  ~MasterController();
  
  void startServer();
  void shutdown();
  static int newClientConnected(int socketID);

  
 private:
  ConnectionListener *connectionListener;
  SpreadsheetController *spreadsheetController;
  int port;
};



#endif
