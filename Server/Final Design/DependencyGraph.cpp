

#include "DependencyGraph.h"
#include <algorithm>

using namespace std;

/*
 *Constructs a DependencyGraph
 */
DependencyGraph::DependencyGraph()
{
  //TODO
  dependents = new std::map<std::string,std::vector<string>* >;
  dependees = new std::map<std::string,std::vector<string>* >;
  size = 0;
}

/*
 *Destructor for the Dependency Graph
 */
DependencyGraph::~DependencyGraph()
{
  //TODO
  delete dependents;
  delete dependees;
}

/*
 *Return the number of ordered pairs in the Dependency Graph
 */
int DependencyGraph::Size()
{
  return this->size;
}

/*
 *Adds the ordered pair (first, second) if it doesn't exist
 *to the Dependencty Graph
 */
void DependencyGraph::AddDependency(string s, string t)
{
  if((*dependents).count(s) == 0)
    {
      (*dependents)[s] = new vector<string>();
    }
  if((*dependees).count(t) == 0)
    {
      (*dependees)[t] = new vector<string>();
    }
  std::vector<std::string> *dep = (*dependents)[s];
  std::vector<std::string> *dee = (*dependees)[t];
  
  if((std::find(dep->begin(),dep->end(),t)==dep->end())&&(std::find(dee->begin(),dee->end(),s)==dee->end()))
    {
      dep->push_back(t);
      dee->push_back(s);
      size++;
    }
  
}

/*
 *Reports whether dependents(s) is non-empty
 */
bool DependencyGraph::HasDependents(string s)
{
  if((*dependents).count(s) == 1 && (*dependents)[s]->size()!=0)
    return true;
  else
    return false;
}

/*
 *Reports whether dependees(s) is non-empty
 */
bool DependencyGraph::HasDependees(string s)
{
  if((*dependees).count(s) == 1 && (*dependees)[s]->size()!=0)
    return true;
  else
    return false;
}

/*
 *Enumerates dependents(s)
 */
vector<string> *DependencyGraph::GetDependents(string s)
{
  vector<string> *toReturn = new vector<string>();
  if(HasDependents(s))
    {
      for(string s: *(*dependents)[s])
	toReturn->push_back(s);
      return toReturn;
    }
  else
    return toReturn;
}

/*
 *Enumerates dependees(s)
 */
vector<string> *DependencyGraph::GetDependees(string s)
{
  vector<string> *toReturn = new vector<string>();
  if(HasDependees(s))
    {
      for(string s: *(*dependees)[s])
	toReturn->push_back(s);
      return toReturn;
    }
  else
    return toReturn;
}

/*
int main()
{
  DependencyGraph DG();
  std::cout<<"DependencyGraph is working \n";
}
*/
