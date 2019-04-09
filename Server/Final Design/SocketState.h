

#ifndef SOCKET_STATE_H
#define SOCKET_STATE_H


#include <vector>
#include <string>
#include <mutex>

class SocketState
{
 public:
  SocketState(int socketID);
  ~SocketState();

  std::string getBuffer();
  void appendMessage(std::string message);
  std::vector<std::string> * getCommandsToProcess();
  
  void socketAwaitData();
  void socketSendData(std::string msg);
  bool isConnected();
  void setConnected(bool con);
  
  int getID();
  
 private:
  bool connected;
  int socketID;
  std::string *buffer;
  std::vector<std::string> * tokenize();
  std::mutex *bufferMtx;
};


#endif
