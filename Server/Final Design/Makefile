all:
	g++ -o server -pthread -std=c++11 -I ./include/json ConnectionListener.cpp MasterController.cpp SpreadsheetController.cpp SpreadsheetInstance.cpp DependencyGraph.cpp Utilities.cpp -pthread

clean:
	rm -f server
