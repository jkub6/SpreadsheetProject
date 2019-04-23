
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
 void ReplaceDependents(std::string s, std::vector<std::string> *newDependents);
 void ReplaceDependees(std::string s, std::vector<std::string> *newDependees);
 bool IsCircular(std::string s);
 void Visit(std::string start, std::string name, std::vector<std::string> *visited);

 std::map<std::string,std::vector<std::string>*> * getDependents();
 std::map<std::string,std::vector<std::string>*> * getDependees();
 

 
private:
int size;
std::map<std::string,std::vector<std::string>* > *dependents;
std::map<std::string,std::vector<std::string>* > *dependees;

};


#endif
