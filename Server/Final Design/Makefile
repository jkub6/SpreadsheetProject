
all: ConnectionListener.o DependencyGraph.o MasterController.o SocketState.o SpreadsheetController.o SpreadsheetInstance.o Utilities.o
	g++ -g -o server -pthread -std=c++11 -I ./include/json ConnectionListener.o DependencyGraph.o MasterController.o SocketState.o SpreadsheetController.o SpreadsheetInstance.o Utilities.o
ConnectionListener.o: ConnectionListener.cpp ConnectionListener.h
	g++ -pthread -std=c++11 -I ./include/json -c -o ConnectionListener.o ConnectionListener.cpp
DependencyGraph.o: DependencyGraph.cpp DependencyGraph.h 
	g++ -pthread -std=c++11  -I ./include/json -c -o DependencyGraph.o DependencyGraph.cpp
MasterController.o: MasterController.cpp MasterController.h 
	g++ -pthread -std=c++11 -I ./include/json -c -o MasterController.o MasterController.cpp
SocketState.o:	SocketState.cpp SocketState.h
	g++ -pthread -std=c++11 -I ./include/json -c -o SocketState.o SocketState.cpp
SpreadsheetController.o: SpreadsheetController.cpp SpreadsheetController.h
	g++ -pthread -std=c++11 -I ./include/json -c -o SpreadsheetController.o SpreadsheetController.cpp
SpreadsheetInstance.o: SpreadsheetInstance.cpp SpreadsheetInstance.h
	g++ -pthread -std=c++11 -I ./include/json -c -o SpreadsheetInstance.o SpreadsheetInstance.cpp
Utilities.o: Utilities.cpp Utilities.h
	g++ -pthread -std=c++11 -I ./include/json -c -o Utilities.o Utilities.cpp
clean:
	rm -f *.o
	rm -f server
