
#include "CellState.h"
#include <vector>
#include <string>


CellState::CellState(std::string cell, std::string value)
{
  dependencies = new std::vector<std::string>();
  this->cell = cell;
  this->value = value;
}

CellState::CellState(std::string cell, std::string value, std::vector<std::string> * dependencies)
{
  this->dependencies = dependencies;
  this->cell = cell;
  this->value = value;
}

CellState::~CellState()
{
  delete dependencies;
}

std::string CellState::getCell()
{
  return cell;
}

std::string CellState::getValue()
{
  return value;
}

std::vector<std::string> * CellState::getDependencies()
{
  return dependencies;
}
