#include <nlohmann/json.hpp>
#include <iostream>
#include <exception>

//Use this command to compile
// g++ -std=c++11 -I /home/ludwig/cs3505_assignments/finalSpreadsheet/SpreadsheetProject/Server/Server/json/include jsonTester.cpp

//EDIT: Actually use: g++ -std=c++11 -I Server/json jsonTester.cpp


using json = nlohmann::json;

int main()
{

  try
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
     }

}
