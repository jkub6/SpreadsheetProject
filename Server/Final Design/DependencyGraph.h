
#ifndef DEPENDENCY_GRAPH_H
#define DEPENDENCY_GRAPH_H

#include <string>
#include <map>
#include <iostream>
#include <bits/stdc++.h> 


class DependencyGraph
{
public:
DependencyGraph();
~DependencyGraph();

//other methods add here
int Size();
void AddDependency(std::string first, std::string second);
bool HasDependents(std::string);
bool HasDependees(std::string);
std::vector<std::string> *GetDependents(std::string s);
std::vector<std::string> *GetDependees(std::string s);
void RemoveDependency(std::string s, std::string t);
  
private:
int size;
std::map<std::string,std::vector<std::string>* > *dependents;
std::map<std::string,std::vector<std::string>* > *dependees;
};


#endif
