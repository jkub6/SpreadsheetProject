//Spreadsheet version 1 - Jacob Larkin
//Will be used in a gui eventually


using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using SpreadsheetUtilities;

namespace SS
{
    /// <summary>
    /// A Spreadsheet object represents the state of a simple spreadsheet.  A 
    /// spreadsheet consists of an infinite number of named cells.
    /// 
    /// A string is a valid cell name if and only if:
    ///   (1) its first character is an underscore or a letter
    ///   (2) its remaining characters (if any) are underscores and/or letters and/or digits
    /// Note that this is the same as the definition of valid variable from the PS3 Formula class.
    /// 
    /// For example, "x", "_", "x2", "y_15", and "___" are all valid cell  names, but
    /// "25", "2x", and "&" are not.  Cell names are case sensitive, so "x" and "X" are
    /// different cell names.
    /// 
    /// A spreadsheet contains a cell corresponding to every possible cell name.  (This
    /// means that a spreadsheet contains an infinite number of cells.)  In addition to 
    /// a name, each cell has a contents and a value.  The distinction is important.
    /// 
    /// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
    /// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
    /// of a cell in Excel is what is displayed on the editing line when the cell is selected.)
    /// 
    /// In a new spreadsheet, the contents of every cell is the empty string.
    ///  
    /// The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
    /// (By analogy, the value of an Excel cell is what is displayed in that cell's position
    /// in the grid.)
    /// 
    /// If a cell's contents is a string, its value is that string.
    /// 
    /// If a cell's contents is a double, its value is that double.
    /// 
    /// If a cell's contents is a Formula, its value is either a double or a FormulaError,
    /// as reported by the Evaluate method of the Formula class.  The value of a Formula,
    /// of course, can depend on the values of variables.  The value of a variable is the 
    /// value of the spreadsheet cell it names (if that cell's value is a double) or 
    /// is undefined (otherwise).
    /// 
    /// Spreadsheets are never allowed to contain a combination of Formulas that establish
    /// a circular dependency.  A circular dependency exists when a cell depends on itself.
    /// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
    /// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
    /// dependency.
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        public override bool Changed { get; protected set; }

        private Dictionary<string, Object> cells = new Dictionary<string, Object>();
        private Dictionary<string, Object> cellValues = new Dictionary<string, Object>();
        private DependencyGraph dGraph = new DependencyGraph();


        public Spreadsheet() : this(s => true, s => s, "default") { }

        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {
            this.IsValid = isValid;
            this.Normalize = normalize;
            this.Version = version;
        }

        public Spreadsheet(String filepath, Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {
            this.IsValid = isValid;
            this.Normalize = normalize;
            this.Version = version;
            Load(filepath);
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
            => cells.Keys;

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        public override object GetCellContents(string name)
            => cells.TryGetValue(BeValid(name), out object obj) ? obj : ""; //return value if known, otherwise ""

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, double number)
            => SetCellContents(name, number);

        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, string text)
            => SetCellContents(name, text);

        /// <summary>
        /// If the formula parameter is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.  (No change is made to the spreadsheet.)
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, Formula formula)
            => SetCellContents(name, formula);

        /// <summary>
        /// private method to set the cell contents to the given contents
        /// </summary>
        private ISet<string> SetCellContents(string name, object contents)
        {
            name = BeValid(name);
            if (contents == null) throw new ArgumentNullException();

            IEnumerable<string> new_deps = new HashSet<string>();
            var old_contents = GetCellContents(name); //used for resetting in case of circular error

            if (contents is Formula f)
                new_deps = f.GetVariables();

            dGraph.ReplaceDependents(name, new_deps);

            if (contents is String s && s == "")
                cells.Remove(name); //no need to store if value is ""
            else
                cells[name] = contents;
            try
            {
                var done = new HashSet<string>(GetCellsToRecalculate(name));
                if (old_contents != contents)
                    Changed = true;
                return done;
            }
            catch (CircularException)
            {
                bool old_changed = Changed;
                SetCellContents(name, old_contents); //reset to previous state if circular error
                Changed = old_changed;
                throw new CircularException();
            }
        }

        /// <summary>
        /// assures that string name is a valid variable and is not null. Throws an InvalidNameException if not.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string BeValid(string name)
        {
            if (name == null || !Regex.IsMatch(name, "^[A-Za-z][0-9]+$"))
                throw new InvalidNameException();
            if (!IsValid(Normalize(name)))
                throw new InvalidNameException();
            return Normalize(name);
        }

        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            if (name == null)
                throw new ArgumentNullException();
            return dGraph.GetDependees(BeValid(name));
        }

        // ADDED FOR PS5
        /// <summary>
        /// Returns the version information of the spreadsheet saved in the named file.
        /// If there are any problems opening, reading, or closing the file, the method
        /// should throw a Spreadsheet
        public override string GetSavedVersion(string filepath)
        {

            try
            {
                using (XmlReader reader = XmlReader.Create(filepath))
                {
                    reader.Read();
                    reader.Read();
                    reader.Read();
                    return reader.GetAttribute("version");
                }
            }
            catch (Exception)
            {
                throw new SpreadsheetReadWriteException("Error reading file version");
            }
        }

        // ADDED FOR PS5
        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using an XML format.
        /// The XML elements should be structured as follows:
        /// 
        /// <spreadsheet version="version information goes here">
        /// 
        /// <cell>
        /// <name>
        /// cell name goes here
        /// </name>
        /// <contents>
        /// cell contents goes here
        /// </contents>    
        /// </cell>
        /// 
        /// </spreadsheet>
        /// 
        /// There should be one cell element for each non-empty cell in the spreadsheet.  
        /// If the cell contains a string, it should be written as the contents.  
        /// If the cell contains a double d, d.ToString() should be written as the contents.  
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override void Save(string filename)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("  ");
            //settings.OmitXmlDeclaration = true; //maybe??

            try
            {
                using (XmlWriter writer = XmlWriter.Create(filename, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("spreadsheet");
                    writer.WriteAttributeString("version", Version);

                    foreach (KeyValuePair<String, object> cell in cells)
                    {
                        string name = cell.Key;
                        string contents = (cell.Value is Formula ? "=" : "") + cell.Value.ToString(); //prepend "=" if a formula

                        writer.WriteStartElement("cell");
                        writer.WriteElementString("name", name);
                        writer.WriteElementString("contents", contents);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    Changed = false;
                }
            }
            catch (Exception)
            {
                throw new SpreadsheetReadWriteException("Error writing file");
            }
        }

        /// <summary>
        /// loads the data from the given file into itself. Throws if there is a version mismatch
        /// </summary>
        /// <param name="filepath"></param>
        private void Load(string filepath)
        {
            if (GetSavedVersion(filepath) != Version)
                throw new SpreadsheetReadWriteException("File versions do not match");
            cells = new Dictionary<string, Object>();
            dGraph = new DependencyGraph();
            try
            {
                using (XmlReader reader = XmlReader.Create(filepath))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement() && reader.Name == "cell")
                        {
                            reader.Read();
                            reader.Read();
                            reader.Read();
                            string name = reader.Value;
                            reader.Read();
                            reader.Read();
                            reader.Read();
                            reader.Read();
                            string contents = reader.Value;

                            SetContentsOfCell(name, contents);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new SpreadsheetReadWriteException("Error reading file");
            }
        }

        /// <summary>
        /// private lookup function to pass to Formula's execute()
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private double Lookup(string name)
        {
            object value = cellValues[name];
            if (value is double d)
                return d;
            throw new Exception();
        }

        // If name is null or invalid, throws an InvalidNameException.
        public override object GetCellValue(string name)
        {
            object value = GetCellContents(name);
            if (value is Formula f)
                return f.Evaluate(Lookup);
            return value;
        }

        // ADDED FOR PS5
        /// <summary>
        /// If content is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a set consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<string> SetContentsOfCell(string name, string content)
        {
            name = BeValid(name);

            if (content == null)
                throw new ArgumentNullException();

            ISet<string> toUpdate;

            if (double.TryParse(content, out double value))
                toUpdate = SetCellContents(name, value);
            else if (content.StartsWith("="))
                toUpdate = SetCellContents(name, new Formula(content.Remove(0, 1), Normalize, IsValid));
            else
                toUpdate = SetCellContents(name, content);

            foreach (string cell in toUpdate)
                cellValues[cell] = GetCellValue(cell);

            return toUpdate;
        }

        public IEnumerable<string> GetDeps(string content)
        {
            List<string> l = new List<string>();

            Formula f = new Formula(content.Remove(0, 1), Normalize, IsValid);

            return f.GetVariables();

            //return l;
        }

        /// <summary>
        /// A convenience method for invoking the other version of GetCellsToRecalculate
        /// with a singleton set of names.  See the other version in AbstractSpreadsheet for details.
        /// </summary>
        protected IEnumerable<String> GetCellsToRecalculate(String name)
        {
            return GetCellsToRecalculate(new HashSet<String>() { name });
        }
    }
}
