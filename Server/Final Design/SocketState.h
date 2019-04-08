

#ifndef SOCKET_STATE_H
#define SOCKET_STATE_H


#include <vector>
#include <string>

class SocketState
{
 public:
  SocketState(int socketID);
  ~SocketState();

  std::string getRemainingMessage();
  std::vector<std::string> * getCommandsToProcess();

  void socketAwaitData();
  void socketSendData(std::string msg);
  bool isConnected();
  
 private:
  bool connected;
  int socketID;
  std::string *remainingMessage;
  std::vector<std::string> *commandsToProcess;
  
};


#endif
