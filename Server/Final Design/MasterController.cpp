
#include "MasterController.h"
#include "ConnectionListener.h"
#include "SpreadsheetController.h"
#include "Utilities.h"
#include <string>
#include <thread>

MasterController::MasterController(int port, std::string pathToSaveDirectory)
{
  this->port = port;
  this->pathToSaveDirectory = pathToSaveDirectory;

  this->connectionListener = new ConnectionListener(port, newClientConnected);
  this->spreadsheetController = new SpreadsheetController(MasterController::pathToSaveDirectory);
  
}
MasterController::~MasterController()
{
  //TODO
}

void MasterController::startServer()
{
  this->connectionListener->beginListeningForClients();

  
  //TODO
}
void MasterController::shutdown(){
  //TODO
}
void MasterController::newClientConnected(int socketID)
{
  //TODO
}

//ENTRY POINT

int main(int argc, char ** argv)
{
  MasterController *masterController = new MasterController(2112,".");
  std::thread connectionThread(&MasterController::startServer,masterController);//implicit this parameter

  connectionThread.join();
  
  return 0;
}

 
