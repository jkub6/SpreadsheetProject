
#include <iostream>
#include <map>
#include <vector>

#ifndef CELL_STATE_H
#define CELL_STATE_H

class CellState
{
 public:
  CellState(std::string cell, std::string value);
  CellState(std::string cell, std::string value, std::vector<std::string> * dependencies);
  ~CellState();

  std::string getCell();
  std::string getValue();
  std::vector<std::string> * getDependencies();
  
 private:
  std::vector<std::string> * dependencies;
  std::string cell;
  std::string value;
  
};


#endif
