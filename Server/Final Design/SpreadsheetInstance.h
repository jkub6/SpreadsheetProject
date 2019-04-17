
#ifndef SPREADSHEET_INSTANCE_H
#define SPREADSHEET_INSTANCE_H

#include "DependencyGraph.h"
#include <string>
#include <map>
#include <thread>
#include "SocketState.h"

class SpreadsheetInstance
{
 public:
  SpreadsheetInstance(std::string pathToSaveFile);
  ~SpreadsheetInstance();

  void newClientConnected(SocketState * sstate);
  void shutdown();
  
 private:

  std::map<int,SocketState *> *connectedClients;
  DependencyGraph *dependencyGraph;
  std::string pathToSaveFile;
  bool running;
  
};



#endif
