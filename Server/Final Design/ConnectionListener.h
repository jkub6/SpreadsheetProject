
#ifndef CONNECTIONLISTENER_H
#define CONNECTIONLISTENER_H

#include <vector>
#include <thread>

class ConnectionListener
{
 
 private:
  int (*callBack)(int);
  int port;
  bool running;
    std::thread *t;
  
 public:
  ConnectionListener(int port, int (*callBack)(int));// the second argument is a function pointer to MasterController's newClientConnected method.
  //It is a callback for ConnectionListener to call whenever a new client connects
  void shutdownListener();
  int genesisSocket;
  ~ConnectionListener();
  void beginListeningForClients();
  
};


#endif
