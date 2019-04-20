
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
  void admin(SocketState * sstate);
  void sendJsonError(SocketState * sstate);
  void sendAdminSuccess(SocketState * sstate);
  int port;
  bool running;
};



#endif
