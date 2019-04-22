#include <nlohmann/json.hpp>
#include <iostream>
#include <exception>
#include <map>
#include <fstream>

//Use this command to compile
// g++ -std=c++11 -I /home/ludwig/cs3505_assignments/finalSpreadsheet/SpreadsheetProject/Server/Server/json/include jsonTester.cpp

//EDIT: Actually use: g++ -std=c++11 -I Server/json jsonTester.cpp


using json = nlohmann::json;

int main()
{

  json test;
  test["hi"]="yo";
  test["two"].push_back("{\"A1\":\"=5\"}");
  test["two"].push_back("{\"A1\":\"WOW\"}");
  test["two"].push_back("{\"B69\":\"SUPER\"}");
  test["two"].push_back("{\"A1\":\"SUPERsdf\"}");
  
  std::cout<<test.dump()<<std::endl;

  std::ofstream o("test.json");
  o<<test<<std::endl;

  o.close();

  std::ifstream i("test.json");
  json input;
  i>>input;

  std::cout<<"INPUT: "<<input.dump(0)<<std::endl;


  std::string first = "32";
  std::string second = "=A3";
  
  json testSave;
  testSave["name"]="test1";
  testSave["A1"]["value"]="63";
  testSave["A1"]["history"]=json::array({first,second,"43"});
  testSave["dep"]["A3"]=json::array({"A1","A4"});
  testSave["dee"]["A1"]=json::array({"A3"});
  testSave["dee"]["A4"]=json::array({"A3"});
  testSave["undo"].push_back({"A1","56"});
  testSave["undo"].push_back({"A3","=A1"});
  
  std::cout<<"\nTEST SAVE: "<<testSave.dump(2)<<std::endl;

  std::ofstream of("save.json");
  of<<testSave;

  of.close();
  
  std::ifstream ins("save.json");

  json loaded;
  ins>>loaded;

  ins.close();

  std::cout<<"\n\nLOADED: \n"<<loaded.dump(3);

  
  /* for(json::iterator it = test["two"].begin();it!=test["two"].end();it++)
  {
    json element = json::parse((std::string)it.value());

    std::string key = element.begin().key();
    std::string value = element.begin().value();


    
    std::cout<<"ELEMENT: "<<json::parse((std::string)it.value()).dump()<<" | ";
    std::cout<<"KEY: "<<key<<" | VALUE: "<<value<<std::endl;

    
    //    std::cout<<json::parse((std::string)it.value()).begin().value()<<std::endl;;
    }*/

  
  /*try
    {
      json bad = json::parse("{\"hi\":5}");
    }
  catch(nlohmann::detail::parse_error e)
    {
      std::cout<<"EXCEPTION "<<e.what()<<std::endl;
    }
  //Creation of a JSON object
     json openObject =
     {
         {"type", "open"},
         {"name", "spreadsheet.sprd"},
         {"username", "Peter Jensen"},
	 {"password", "Doofus"}
     };

     //Can change elements with a certain key
     openObject.at("username") = "William Ludwig";

     //output of changed JSON object
     std::cout << openObject << '\n';

     //To read the type of the message
     openObject.at("type");

     //Ways to handle different JSON commands
     if(openObject.at("type")==("open")) //can set equal to the other types
     {
       std::cout<< "I am an open command" << '\n';
       }*/

}
