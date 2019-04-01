

#include "SpreadsheetInstance.h"


SpreadsheetInstance::SpreadsheetInstance(std::string pathToSaveFile)
  {
    this->pathToSaveFile = pathToSaveFile;
    dependencyGraph = new DependencyGraph();
  }
SpreadsheetInstance::~SpreadsheetInstance()
{
  //TODO
}

void SpreadsheetInstance::newClientConnected(int socketID)
{
  //TODO
}
void SpreadsheetInstance::shutdown()
{
  //TODO
}
  
