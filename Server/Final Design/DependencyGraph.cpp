

#include "DependencyGraph.h"
#include <algorithm>

using namespace std;

/*
 *Constructs a DependencyGraph
 */
DependencyGraph::DependencyGraph()
{
  dependents = new std::map<std::string,std::vector<string>* >;
  dependees = new std::map<std::string,std::vector<string>* >;
  size = 0;
}

/*
 *Destructor for the Dependency Graph
 */
DependencyGraph::~DependencyGraph()
{
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

void DependencyGraph::RemoveDependency(string s, string t)
{
  if(!HasDependents(s))
    {
    return;
    }
  
  std::vector<std::string> *dep = (*dependents)[s];
  std::vector<std::string> *dee = (*dependees)[t];

  
  if((std::find(dep->begin(),dep->end(),s)==dep->end())&&(std::find(dee->begin(),dee->end(),t)==dee->end()))
    {
      //Finds the position of t and deletes it from dependents
      std::vector<string>::iterator position = std::find(dep->begin(), dep->end(), t);
      if (position != dep->end()) // == myVector.end() means the element was not found
	dep->erase(position);

      //Finds the position of s and deletes it from dependees
      position = std::find(dee->begin(), dee->end(), s);
      if (position != dee->end()) // == myVector.end() means the element was not found
	dee->erase(position);

      size--;
      return;
    }
}

void DependencyGraph::ReplaceDependents(string s, vector<string> *newDependents)
{
  if(HasDependents(s))
    {
      //Create a copy so that we can iterate through the copy and remove from original
      vector<string> *newVector = new vector<string>();
      *newVector = *(*dependents)[s];
      for(string t: *newVector)
	{
	  RemoveDependency(s,t);
	}
      for(string t: *newDependents)
	{
	  AddDependency(s,t);
	}
    }
}

void DependencyGraph::ReplaceDependees(string s, vector<string> *newDependees)
{
  if(HasDependees(s))
    {
      //Create a copy so that we can iterate through the copy and remove from original
      vector<string> *newVector = new vector<string>();
      *newVector = *(*dependees)[s];
      for(string t: *newVector)
	{
	  RemoveDependency(t,s);
	}
      for(string t: *newDependees)
	{
	  AddDependency(t,s);
	}
    }
}



/*
int main()
{
  DependencyGraph *DG = new DependencyGraph();
  std::cout<<"DependencyGraph is working \n";
  if(DG->Size() == 0)
    cout<<"SUCCESS"<<endl;
  DG->AddDependency("a", "b");
  if(DG->Size() == 1)
    cout<<"SUCCESS"<<endl;
  DG->RemoveDependency("b","a");
    if(DG->Size() == 1)
    cout<<"SUCCESS"<<endl;
    else
      cout<<"Fail"<<endl;
  DG->AddDependency("b", "c");
  if(DG->Size() == 2)
    cout<<"SUCCESS"<<endl;
  DG->AddDependency("a", "b");//Test adding a duplicate
  if(DG->Size() == 2)
    cout<<"SUCCESS"<<endl;

  //Test RemoveDependency
  DG->RemoveDependency("a", "b");
  if(DG->Size() == 1)
    cout<<"SUCCESS"<<endl;
  //Remove Something that was already removed
  DG->RemoveDependency("a", "b");
  if(DG->Size() == 1)
    cout<<"SUCCESS"<<endl;

  //TODO:Test ReplaceDependents and ReplaceDependees
  
  
}
*/


