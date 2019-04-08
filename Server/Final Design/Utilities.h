
#ifndef SPREADSHEET_UTILITIES_H
#define SPREADSHEET_UTILITIES_H

#include <string>
#include <vector>



#include "Utilities.h"
#include <sys/socket.h>


class Utilities{
 public:

  void sendMessage(int socketID, std::string message);

 static std::vector<std::string>* Tokenize(std::string *input);



 
 static std::vector<std::string>* receiveMessage(int socketID, int *bytesRead, std::string * remainingMessage);




//Returns true if connection is still active, false otherwise
 static bool checkConnectionState(int socketID);

};

#endif
