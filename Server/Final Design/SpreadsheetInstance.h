
#ifndef SPREADSHEET_INSTANCE_H
#define SPREADSHEET_INSTANCE_H

#include "DependencyGraph.h"
#include <string>
#include <map>
#include <thread>
#include "SocketState.h"
#include <thread>
#include <mutex>

class SpreadsheetInstance
{
 public:
  SpreadsheetInstance(std::string pathToSaveFile);
  ~SpreadsheetInstance();

  void newClientConnected(SocketState * sstate);
  void shutdown();
  
 private:
  std::mutex *savingMtx;

  std::thread * sheetThread;
  //  std::mutex *usersMtx;
  void saveToDisk();
  void loop();
  void load();
  std::map<int,SocketState *> *connectedClients;
  std::map<std::string,std::string> *data;
  DependencyGraph *dependencyGraph;
  std::string pathToSaveFile;
  bool running;
};



#endif
