In this file, you should keep track of design decisions, external code resources, 
build guidelines, implementation notes, problems, etc. Make sure you put your author 
information in the file, as well as dates associated with entries.

Things that would be very useful in this README are: a short summary of your GUI's additional 
features, any special instructions for general use, any important design decisions...
------ Project ---------
10/12/2018
	Authors: Alexis Kooopmann & Jacob Larkin
	Project: Spreadsheet
------- External Code Resources --------
10/12/2018
	Spreadsheet.dll
		DependencyGraph.dll
		Formula.dll
	SpreadsheetPanel.dll

------- Build Guidelines ---------
Goal: Build an effective GUI for user interaction with spreadsheet.
	Entering data (strings, numbers, and formulas)
	Updating/displaying the results in a clear manner.

Basic Requirements:
	When application starts, there should be a single window, containing an empty spreadsheet.
	At any point in time, there will be one or more independent spreadsheet windows displayed.
	When the last window is closed, the application should terminate.
	The grid is 26 columns and 99 rows. 
	The A1 cell is in the upper left and Z99 in the lower right.
	Each cell in the grid has an associated content (string, double, or formula) and value (string, double, or error).
	The value of each cell is displayed in the grid.
	Cell names are case insensitive. For example, A2 and a2 are the same. 
	Any formula that references a cell not in the grid is invalid.
Versioning/Saving:
	We will use the version string: 'ps6' for these spreadsheets. 
Grid Functionality:
	There will always be one selected cell, highlighted in some fashion. Default is A1.
	You can use the mouse to select different cells. 
	There is a non-editable text box showing the cell name of the selected cell.
	There is a non-editable text box showing the cell value of the selected cell.
	There is an editable text box showing the contents of the selected cell.
	You can change the content of the cell by selecting it and typing or deleting text. 	
Errors:
	Invalid Formula results in useful message.
	Circular Exception results in an informative message. 
	Formula errors with divide by zero or invalid variables displays an informative message
File Menu and Saving:
	There is a File menu that allows the user create a New empty spreadsheet in its own window, 
		to Save the current spreadsheet to a file, 
		to Open a spreadsheet from a file replacing the current spreadsheet, 
		and to Exit the spreadsheet window.
	Spreadsheet defaults to the use of the extension ".sprd" for saving and reading spreadsheet files. 
	All file dialogs should allow the user to choose whether to display only ".sprd" files 
		or (2) to display all files. 
	Option 1: make sure that all files that are selected or entered have the ".sprd" extension 
		(e.g., your code should detect if the user doesn't add .sprd and append it if necessary).
	Option 2: No restriction implemented 
Safety Features:
	If an operation would result in the loss of unsaved data, a warning dialog is displayed asking to save.
	If an operation would result in a file being overwritten a warning dialog is displayed. 
	If any of these save options result in exceptions, a suitable error message is displayed.
Help Menu:
	The help menu provides helpful information about how to use the spreadsheet. 
Additional Functionality:
	Pressing the Enter key results in the same calculation as the calculate button and moves down.
	Exiting a cell will automatically calculate the value of it's contents. 
	Selecting a cell automatically allows you to enter it's contents. 
	Spreadsheet can be called from the command line with the specific file and the GUI can accomadate
		opening said file. 
	Tab calculates the value of a cell as well and moves right.
	Shift calculates the value of the cell and moves left. 
	If a file is unsaved an asterisk is displayed next to its name
	The default file name is Untitled.sprd. 
	You can change the color of the Top Part of the spreadsheet.
	You can clear all the cells in the spreadsheet. 
	Spreadsheet cheers you on everytime you save.


------- Implementation Notes -------
10/12/2018
	We decided to add arrow key functionality so that users can switch between cells with arrow keys. 
10/20/2018
	Removed Double Click Functionality.
10/20/2018
	Removed Arrow Key Functionality
10/21/2018
	Implemented basic save and open events handlers. 
10/22/2018
	Added Exception Catching for unknown symbols in formula
	Added Exception Catching for divide by zero
	Added Invalid Formula Dialog
10/22/2018
	Added Color Functionality
	Added Clear all Cells Functionality 
	Fixed all the Method Declarations
	Fixed the Cancel Dialog in the "Lose save data".
10/22/2018
	Fixed all circular dependency exception errors with a useful message. 
------- Problems --------

10/20/2018
	We found out that doubleClick functionality similar to Microsoft Excell would require the 
	modification of the SpreadsheetPanel.dll we were provided and could not find a work around. 
	Arrow key functionality had to be removed since it doesn't function without 
	double click functionatlity. 
---- To-Do ---
Handle Circular Exception Error
