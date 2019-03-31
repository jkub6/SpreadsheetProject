
#ifndef CONNECTIONLISTENER_H
#define CONNECTIONLISTENER_H

#include <vector>

class ConnectionListener
{
 
 private:
  void (*callBack)(int);
  void shutdown();
  int port;
  
 public:
  ConnectionListener(int port, void (*callBack)(int));// the second argument is a function pointer to MasterController's newClientConnected method.
  //It is a callback for ConnectionListener to call whenever a new client connects

  ~ConnectionListener();
  void beginListeningForClients();
  
};


#endif
