
#ifndef SPREADSHEET_UTILITIES_H
#define SPREADSHEET_UTILITIES_H

#include <string>


class Utilities{
  static void sendMessage(int socketID, std::string message);

  static void receiveMessage(int socketID);


//Returns true if connection is still active, false otherwise
  static bool checkConnectionState(int socketID);

};

#endif
