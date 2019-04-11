
#ifndef MASTER_CONTROLLER_H
#define MASTER_CONTROLLER_H

#include "ConnectionListener.h"
#include "SpreadsheetController.h"
#include "Utilities.h"
#include <string>
#include <map>
#include <thread>

class MasterController
{
 public:

  std::map<int,std::thread *> *threadpool;
  MasterController(int port);
  ~MasterController();
  
  void startServer();
  void shutdown();
  int newClientConnected(int socketID);

  
 private:
  ConnectionListener *connectionListener;
  SpreadsheetController *spreadsheetController;
  int port;
  bool running;
};



#endif
