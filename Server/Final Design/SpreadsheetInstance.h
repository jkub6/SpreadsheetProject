
#ifndef SPREADSHEET_INSTANCE_H
#define SPREADSHEET_INSTANCE_H

#include "DependencyGraph.h"
#include <string>
#include <map>
#include <thread>


class SpreadsheetInstance
{
 public:
  SpreadsheetInstance(std::string pathToSaveFile);
  ~SpreadsheetInstance();

  void newClientConnected(int socketID);
  void shutdown();
  
 private:

  std::map<int,std::thread> *socketThreads;//Maps socketID's to individual client threads
  DependencyGraph *dependencyGraph;
  std::string pathToSaveFile;
  
};



#endif
