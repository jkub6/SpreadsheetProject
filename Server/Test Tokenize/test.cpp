
#include <iostream>
#include <string>
#include <vector>


using namespace std;





int main()
{

  

  vector<string> tokens;
  string test = "ABC\n\nDEFGHI\n\nIJ\n\nK\n";

  string delimiter = "\n\n";


  
  cout<<"TOKENIZER, Delimiting on: "<<"\\n\\n"<<"\n----------------------------------"<<endl<<endl;
  cout<<"ORIGINAL STRING:\n{"<<test<<"}"<<endl;
  
  for(int index = test.find(delimiter); index > 0; )
    {
      string subString = test.substr(0,index);
      tokens.push_back(subString);
      index+=2;

      test = test.erase(0,index);
      index = test.find(delimiter);
      
    }
  

  
  cout<<"TOKENS:\n---------------------------------------\n"<<endl;
  for(int i = 0;i<tokens.size();i++)
    {
      cout<<"["<<tokens[i]<<"]"<<endl<<endl;
    }
  cout<<"\nRemaining:\n------------------------------------\n"<<endl;
  cout<<"["<<test<<"]"<<endl<<endl;
  
  return 0;
}
