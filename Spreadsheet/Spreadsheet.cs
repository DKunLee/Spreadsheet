/// <summary>
/// Author:    DK Lee
/// Partner:   None
/// Date:      Feb 6, 2024
/// Course:    CS 3500, University of Utah, School of Computing
/// Copyright: CS 3500 and DK Lee - This work may not 
///            be copied for use in Academic Coursework.
///
///     I, DK, certify that I wrote this code from scratch and
///     did not copy it in part or whole from another source.  All 
///     references used in the completion of the assignments are cited 
///     in my README file.
/// </summary>
/// <summary>
///   File Contents (In summary):
///     <para>
///     This file is the internals of the spreadsheet program. This file is also
///     inherit from AbstractSpreadsheet.cs and implements it's abstract methods.
/// </summary>
/// File Contents (In details):
/// <summary>
///   <para>
///       An AbstractSpreadsheet object represents the state of a simple spreadsheet.  A 
///       spreadsheet consists of an infinite number of named cells.
///   </para>
///   <para>
///       A string is a valid cell name if and only if:
///   </para>
///   <list type="number">
///        <item> The first "half" starts with one or more letters</item>
///        <item> The second "half" ends with one or more numbers (digits)</item>
///   </list>   
/// 
///   <example>
///       For example, "A15", "a15", "XY032", and "BC7" are cell names so long as they
///       satisfy IsValid.  On the other hand, "Z", "X_", and "hello" are not cell names,
///       regardless of IsValid.
///   </example>
///
///   <para>
///       Any valid incoming cell name, whether passed as a parameter or embedded in a formula,
///       must be normalized with the Normalize method before it is used by or saved in 
///       this spreadsheet.  For example, if Normalize is s => s.ToUpper(), then
///       the Formula "x3+a5" should be converted to "X3+A5" before use.
///   </para>
/// 
///   <para>
///       A spreadsheet contains a cell corresponding to every possible cell name.  (This
///       means that a spreadsheet contains an infinite number of cells.)  In addition to 
///       a name, each cell has a contents and a value.  The distinction is important.
///   </para>
/// 
///   <para>
///       The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
///       contents is an empty string, we say that the cell is empty.  (By analogy, the contents
///       of a cell in Excel is what is displayed on the editing line when the cell is selected.)
///   </para>
/// 
///   <para>
///       In a new spreadsheet, the contents of every cell is the empty string. Note: 
///       this is by definition (it is IMPLIED, not stored).
///   </para>
/// 
///   <para>
///       The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
///       (By analogy, the value of an Excel cell is what is displayed in that cell's position
///       in the grid.)
///   </para>
/// 
///   <list type="number">
///     <item>If a cell's contents is a string, its value is that string.</item>
/// 
///     <item>If a cell's contents is a double, its value is that double.</item>
/// 
///     <item>
///       <para>
///        If a cell's contents is a Formula, its value is either a double or a FormulaError,
///        as reported by the Evaluate method of the Formula class.  The value of a Formula,
///        of course, can depend on the values of variables.  The value of a variable is the 
///        value of the spreadsheet cell it names (if that cell's value is a double) or 
///        is undefined (otherwise).
///      </para>
///     </item>
/// 
///   </list>
/// 
///   <para>
///       Spreadsheets are never allowed to contain a combination of Formulas that establish
///       a circular dependency.  A circular dependency exists when a cell depends on itself.
///       For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
///       A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
///       dependency.
///   </para>
/// </summary>

using System.Globalization;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using SpreadsheetUtilities;

namespace SS;
public class Spreadsheet : AbstractSpreadsheet
{
    private Dictionary<string, Cell> cells;
    private DependencyGraph dependencies;
    public override bool Changed { get; protected set; }
    private bool Saved;

    /// <summary>
    ///   <para>
    ///   Create the spreadsheet with default version which is "1.0"
    ///   isValid is true for this Spreadsheet
    ///   normalize doen't convert any cell name
    ///   </para>
    /// </summary>
    public Spreadsheet() : base(s => true, s => s, "default")
    {
        cells = new();
        dependencies = new();
        Changed = false;
        Saved = false;
    }

    /// <summary>
    ///   <para>
    ///   Create the spreadsheet with given version.
    ///   isValid is given by programmer who wants to use this API
    ///   normalize is given by programmer who wants to use this API
    ///   </para>
    /// </summary>
    public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version)
        : base(isValid, normalize, version)
    {
        cells = new();
        dependencies = new();
        Changed = false;
        Saved = false;
    }

    /// <summary>
    ///   <para>
    ///   Create the spreadsheet with given file in the path.
    ///   isValid is given by programmer who wants to use this API
    ///   normalize is given by programmer who wants to use this API
    ///   </para>
    /// </summary>
    public Spreadsheet(string pathToFile, Func<string, bool> isValid, Func<string, string> normalize, string version)
        : base(isValid, normalize, version)
    {
        cells = new();
        dependencies = new();
        Changed = false;
        buildSpreadSheetFromXML(pathToFile);
        Saved = true;
    }

    /// <summary>
    ///   Returns the names of all non-empty cells.
    /// </summary>
    /// <returns>
    ///     Returns an Enumerable that can be used to enumerate the names of all
    ///     the non-empty cells in the spreadsheet. If all cells are empty then
    ///     an IEnumerable with zero values will be returned.
    /// </returns>
    public override IEnumerable<string> GetNamesOfAllNonemptyCells()
    {
        return cells.Keys;
    }

    /// <summary>
    ///  Set the contents of the named cell to the given number.  
    /// </summary>
    /// <requires> 
    ///   The name parameter must be valid: non-empty/not ""
    /// </requires>
    /// <remark>
    ///   <para>
    ///   Set the contents of the named cell to the given number. The method
    ///   returns a set consisting of name plus the names of all other cells
    ///   whose value depends, directly or indirectly, on the named cell.
    ///   For example, if name is A1 and set number as 5, B1 contains A1*2,
    ///   and C1 contains B1+A1, the set {A1, B1, C1} is returned.
    ///   </para>
    /// </remark>
    /// <exception cref="InvalidNameException"> 
    ///   If the name is invalid, throw an InvalidNameException
    /// </exception>
    /// <param name="name"> The name of the cell </param>
    /// <param name="number"> The new contents/value </param>
    /// <returns>
    ///   <para>
    ///       This method returns a LIST consisting of the passed in name followed by the names of all 
    ///       other cells whose value depends, directly or indirectly, on the named cell.
    ///   </para>
    ///   <para>
    ///       The order must correspond to a valid dependency ordering for recomputing
    ///       all of the cells, i.e., if you re-evaluate each cell in the order of the list,
    ///       the overall spreadsheet will be consistently updated.
    ///   </para>
    ///   <para>
    ///       For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
    ///       set {A1, B1, C1} is returned, i.e., A1 was changed, so then A1 must be 
    ///       evaluated, followed by B1 re-evaluated, followed by C1 re-evaluated.
    ///   </para>
    /// </returns>
    protected override IList<string> SetCellContents(string name, double number)
    {
        if (!IsValidVariable(name)) throw new InvalidNameException();

        if (cells.ContainsKey(name))
        {
            if (cells[name].Contents is Formula)
            {
                foreach (string var in dependencies.GetDependents(name))
                {
                    dependencies.RemoveDependency(name, var);
                }
            }
            
            cells[name] = new Cell(name, number);
        }
        else
        {  
            cells.Add(name, new Cell(name, number));
        }

        Changed = true;
        Saved = false;
        return GetCellsToRecalculate(name).ToList();
    }

    /// <summary>
    /// The contents of the named cell becomes the text.  
    /// </summary>
    /// <requires> 
    ///   The name parameter must be valid/non-empty ""
    /// </requires>
    /// <remark>
    ///   <para>
    ///   The contents of the named cell becomes the text. Set the contents of
    ///   the named cell to the given number.The method returns a set consisting
    ///   of name plus the names of all other cells whose value depends,
    ///   directly or indirectly, on the named cell. For example, if name is A1
    ///   and set text as "TEXT", B1 contains A1*2, and C1 contains B1+A1, the
    ///   set {A1, B1, C1} is returned.
    ///   returned.
    ///   </para>
    /// </remark>
    /// <exception cref="InvalidNameException"> 
    ///   If the name is invalid, throw an InvalidNameException
    /// </exception>   
    /// <param name="name"> The name of the cell </param>
    /// <param name="text"> The new content/value of the cell</param>
    /// <returns>
    ///   <para>
    ///       This method returns a LIST consisting of the passed in name followed by the names of all 
    ///       other cells whose value depends, directly or indirectly, on the named cell.
    ///   </para>
    ///
    ///   <para>
    ///       The order must correspond to a valid dependency ordering for recomputing
    ///       all of the cells, i.e., if you re-evaluate each cell in the order of the list,
    ///       the overall spreadsheet will be consistently updated.
    ///   </para>
    ///
    ///   <para>
    ///     For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
    ///     set {A1, B1, C1} is returned, i.e., A1 was changed, so then A1 must be 
    ///     evaluated, followed by B1 re-evaluated, followed by C1 re-evaluated.
    ///   </para>
    /// </returns>
    protected override IList<string> SetCellContents(string name, string text)
    {
        if (!IsValidVariable(name)) throw new InvalidNameException();

        if (cells.ContainsKey(name))
        {
            if (cells[name].Contents is Formula)
            {
                foreach (string var in dependencies.GetDependents(name))
                {
                    dependencies.RemoveDependency(name, var);
                }
            }
            
            cells[name] = new Cell(name, text);
        }
        else
        {  
            cells.Add(name, new Cell(name, text));
        }

        Changed = true;
        Saved = false;
        return GetCellsToRecalculate(name).ToList();
    }

    /// <summary>
    /// Set the contents of the named cell to the formula.  
    /// </summary>
    /// <requires> 
    ///   The name parameter must be valid/non-empty
    /// </requires>
    /// <remark>
    ///   <para>
    ///   Set the contents of the named cell to the formula. The method returns
    ///   a Set consisting of name plus the names of all other cells whose
    ///   value depends, directly or indirectly, on the named cell. For example,
    ///   if name is A1 and set formula as "A5 + 5", B1 contains A1*2, and
    ///   C1 contains B1+A1, the set {A1, B1, C1} is returned.
    ///   </para>
    /// </remark>
    /// <exception cref="InvalidNameException"> 
    ///   If the name is invalid, throw an InvalidNameException
    /// </exception>
    /// <exception cref="CircularException"> 
    ///   If changing the contents of the named cell to be the formula would 
    ///   cause a circular dependency, throw a CircularException.  
    ///   (NOTE: No change is made to the spreadsheet.)
    /// </exception>
    /// <param name="name"> The cell name</param>
    /// <param name="formula"> The content of the cell</param>
    /// <returns>
    ///   <para>
    ///       This method returns a LIST consisting of the passed in name followed by the names of all 
    ///       other cells whose value depends, directly or indirectly, on the named cell.
    ///   </para>
    ///   <para>
    ///       The order must correspond to a valid dependency ordering for recomputing
    ///       all of the cells, i.e., if you re-evaluate each cell in the order of the list,
    ///       the overall spreadsheet will be consistently updated.
    ///   </para>
    ///   <para>
    ///     For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
    ///     set {A1, B1, C1} is returned, i.e., A1 was changed, so then A1 must be 
    ///     evaluated, followed by B1 re-evaluated, followed by C1 re-evaluated.
    ///   </para>
    /// </returns>
    protected override IList<string> SetCellContents(string name, Formula formula)
    {
        if (!IsValidVariable(name)) throw new InvalidNameException();

        bool newCell;
        object cellContent = GetCellContents(name);

        if (cells.ContainsKey(name))
        {
            newCell = false;
            dependencies.ReplaceDependents(name, formula.GetVariables());
            cells[name] = new Cell(name, formula, Lookup);
        }
        else
        {
            newCell = true;
            cells.Add(name, new Cell(name, formula, Lookup));
            foreach (string var in formula.GetVariables())
                dependencies.AddDependency(name, var);
        }
        
        try
        {
            List<string> rtn = GetCellsToRecalculate(name).ToList();
            Changed = true;
            Saved = false;
            return rtn;
        }
        catch (CircularException e)
        {
            if (newCell) 
            {
                cells.Remove(name);

                foreach (string var in formula.GetVariables())
                    dependencies.RemoveDependency(name, var);
            }
            else
            {
                if (cellContent is Formula) SetCellContents(name, (Formula)cellContent);
                else if (cellContent is double) SetCellContents(name, (double)cellContent);
                else SetCellContents(name, (string)cellContent);
            }

            throw e;
        }
    }

    /// <summary>
    ///     <para>
    ///         Sets the contents of the named cell to the appropriate value.
    ///     </para>
    ///     <para>
    ///         First, if the content parses as a double, the contents of the named
    ///         cell becomes that double.
    ///     </para>
    ///     <para>
    ///         Otherwise, if content begins with the character '=', an attempt is made
    ///         to parse the remainder of content into a Formula.  
    ///         There are then three possible outcomes:
    ///     </para>
    ///     <list type="number">
    ///         <item>
    ///             If the remainder of content cannot be parsed into a Formula, a 
    ///             SpreadsheetUtilities.FormulaFormatException is thrown.
    ///         </item>
    ///         <item>
    ///             If changing the contents of the named cell to be f
    ///             would cause a circular dependency, a CircularException is thrown,
    ///             and no change is made to the spreadsheet.
    ///         </item>
    ///         <item>
    ///             Otherwise, the contents of the named cell becomes f.
    ///         </item>
    ///     </list>
    ///     <para>
    ///         Finally, if the content is a string that is not a double and does not
    ///         begin with an "=" (equal sign), save the content as a string.
    ///     </para>
    /// </summary>
    /// <exception cref="InvalidNameException"> 
    ///     If the name parameter is invalid, throw an InvalidNameException
    /// </exception>
    /// <exception cref="SpreadsheetUtilities.FormulaFormatException"> 
    ///     If the content is "=XYZ" where XYZ is an invalid formula, throw a FormulaFormatException.
    /// </exception>
    /// <exception cref="CircularException"> 
    ///     If changing the contents of the named cell to be the formula would 
    ///     cause a circular dependency, throw a CircularException.  
    ///     (NOTE: No change is made to the spreadsheet.)
    /// </exception>
    /// <param name="name"> The cell name that is being changed</param>
    /// <param name="dContent"> The new content of the cell</param>
    /// <returns>
    ///     <para>
    ///         This method returns a list consisting of the passed in cell name,
    ///         followed by the names of all other cells whose value depends, directly
    ///         or indirectly, on the named cell. The order of the list MUST BE any
    ///         order such that if cells are re-evaluated in that order, their dependencies 
    ///         are satisfied by the time they are evaluated.
    ///     </para>
    ///     <para>
    ///         For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
    ///         list {A1, B1, C1} is returned.  If the cells are then evaluate din the order:
    ///         A1, then B1, then C1, the integrity of the Spreadsheet is maintained.
    ///     </para>
    /// </returns>
    public override IList<string> SetContentsOfCell(string name, string content)
    {
        if (!IsValid(Normalize(name)))
            throw new InvalidNameException();

        IList<string> needsToBeUpdated;

        if (content.Equals("") || content == null)
            return new List<string>();

        if (double.TryParse(content, out double number))
            needsToBeUpdated = SetCellContents(name, number);
        else if (isFor(content))
        {
            if (content.StartsWith("="))
                needsToBeUpdated = SetCellContents(name, new Formula(content.Remove(0, 1), Normalize, IsValid));
            else
                needsToBeUpdated = SetCellContents(name, new Formula(content, Normalize, IsValid));
        }
        else
            needsToBeUpdated = SetCellContents(name, content);

        foreach (string cell in needsToBeUpdated)
        {
            string normC = Normalize(cell);
            if (!normC.Equals(name))
            {
                if (cells.ContainsKey(normC))
                {
                    object temp = cells[normC].Contents;
                    cells.Remove(cell);
                    cells.Add(cell, new Cell(cell, temp, Lookup));
                }
            }
        }

        return needsToBeUpdated;
    }

    /// <summary>
    ///   Returns the contents (as opposed to the value) of the named cell.
    /// </summary>
    /// <remark>
    ///   <para>
    ///   This method allows to get the contents of cell named 'name'. Not
    ///   the value of the 'name' cell.
    ///   </para>
    /// </remark>
    /// <exception cref="InvalidNameException"> 
    ///   Thrown if the name is invalid: blank/empty/""
    /// </exception>
    /// <param name="name">The name of the spreadsheet cell to query</param>
    /// <returns>
    ///   The return value should be either a string, a double, or a Formula.
    ///   See the class header summary 
    /// </returns>
    public override object GetCellContents(string name)
    {
        string normalizedName = Normalize(name);
        if (!IsValidVariable(normalizedName) || !IsValid(normalizedName)) throw new InvalidNameException();

        if (cells.ContainsKey(normalizedName))
            return cells[normalizedName].Contents;
        else
            return "";
    }

    /// <summary>
    ///   Returns an enumeration, without duplicates, of the names of all cells whose
    ///   values depend directly on the value of the named cell. 
    /// </summary>
    /// <required>
    ///   The name must be valid upon entry to the function.
    /// </required>
    /// <param name="name">The name of the cell</param>
    /// <returns>
    ///   Returns an enumeration, without duplicates, of the names of all cells that contain
    ///   formulas containing name.
    ///   <para>For example, suppose that: </para>
    ///   <list type="bullet">
    ///      <item>A1 contains 3</item>
    ///      <item>B1 contains the formula A1 * A1</item>
    ///      <item>C1 contains the formula B1 + A1</item>
    ///      <item>D1 contains the formula B1 - C1</item>
    ///   </list>
    ///   <para>The direct dependents of A1 are B1 and C1</para>
    /// </returns>
    protected override IEnumerable<string> GetDirectDependents(string name)
    {
        return dependencies.GetDependees(name);
    }

    /// <summary>
    /// If name is invalid, throws an InvalidNameException.
    /// </summary>
    /// <exception cref="InvalidNameException"> 
    ///   If the name is invalid, throw an InvalidNameException
    /// </exception>
    /// <param name="name"> The name of the cell that we want the value of (will be normalized)</param>
    /// <returns>
    ///   Returns the value (as opposed to the contents) of the named cell. The return
    ///   value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
    /// </returns>
    public override object GetCellValue(string name)
    {
        string normalizedName = Normalize(name);
        if (!IsValidVariable(normalizedName) || !IsValid(normalizedName))
            throw new InvalidNameException();
        if (cells.ContainsKey(normalizedName))
            return cells[normalizedName].Value;
        else
            return "";
    }

    /// <summary>
    /// Writes the contents of this spreadsheet to the named file using an XML format.
    /// The XML elements should be structured as follows:
    /// 
    /// <spreadsheet version="version information goes here">
    ///     <cell>
    ///         <name>cell name goes here</name>
    ///         <contents>cell contents goes here</contents>    
    ///     </cell>
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
        if (!Saved || Changed) 
        {
            if (!checkingDirectory(filename)) throw new SpreadsheetReadWriteException("");
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings{Indent = true, IndentChars = "  "};

                using (XmlWriter writer = XmlWriter.Create(filename, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("spreadsheet");
                    writer.WriteAttributeString("version", Version);
                    writer.WriteStartElement("cells");

                    foreach (Cell cell in cells.Values)
                        cell.WriteXml(writer);

                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                Changed = false;
                Saved = true;
            }
            catch
            {
                throw new SpreadsheetReadWriteException("There was an error " +
                    "saving the spreadsheet.");
            }
        }
    }

    /// <summary>
    ///   Look up the version information in the given file. If there are any problems opening, reading, 
    ///   or closing the file, the method should throw a SpreadsheetReadWriteException with an explanatory message.
    /// </summary>
    /// <remarks>
    ///   In an ideal world, this method would be marked static as it does not rely on an existing SpreadSheet
    ///   object to work; indeed it should simply open a file, lookup the version, and return it.  Because
    ///   C# does not support this syntax, we abused the system and simply create a "regular" method to
    ///   be implemented by the base class.
    /// </remarks>
    /// <exception cref="SpreadsheetReadWriteException"> 
    ///   1Thrown if any problem occurs while reading the file or looking up the version information.
    /// </exception>
    /// <param name="filename"> The name of the file (including path, if necessary)</param>
    /// <returns>Returns the version information of the spreadsheet saved in the named file.</returns>
    public override string GetSavedVersion(string filename)
    {
        try
        {
            using (var reader = XmlReader.Create(filename))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        if (reader.Name == "spreadsheet")
                        {
                            return reader.GetAttribute("version");
                        }
                    }
                }

                throw new SpreadsheetReadWriteException("No version found in " +
                    "spreadsheet.");
            }
        }
        catch
        {
            throw new SpreadsheetReadWriteException("There was an error accessing " +
                "the version of the given file.");
        }
    }

    /// <summary>
    ///   Return an XML representation of the spreadsheet's contents
    /// </summary>
    /// <returns> contents in XML form </returns>
    public override string GetXML()
    {
        StringBuilder stringBuilder = new();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        settings.IndentChars = "  ";

        using (XmlWriter writer = XmlWriter.Create(stringBuilder, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("spreadsheet");

            writer.WriteAttributeString("version", Version);

            writer.WriteStartElement("cells");

            foreach (Cell cell in cells.Values)
                cell.WriteXml(writer);

            writer.WriteEndElement(); 
            writer.WriteEndElement(); 
            writer.WriteEndDocument();
        }

        return stringBuilder.ToString();
    }

    // Helper method------------------------------------------------------------------------------------
    private void buildSpreadSheetFromXML(string filename)
    {
        try
        {
            using (var reader = XmlReader.Create(filename))
            {
                while (true)
                {
                    if (!reader.Read())
                        break;
                    if (reader.IsStartElement())
                    {
                        if (reader.Name == "spreadsheet")
                        {
                            if (reader.GetAttribute("version") != Version)
                                throw new SpreadsheetReadWriteException("" +
                                    "The version of this spreadsheet is invalid.");
                        }

                        if (reader.Name == "cell")
                        {
                            // Reads to the name
                            reader.Read();
                            reader.Read();
                            reader.Read();
                            string name = reader.Value;
                            //Reads to the contents
                            reader.Read();
                            reader.Read();
                            reader.Read();
                            reader.Read();
                            string content = reader.Value;
                            SetContentsOfCell(name, content);
                        }
                    }
                }
            }
        }
        catch
        {
            throw new SpreadsheetReadWriteException("There was an error reading " +
                "the given file");
        }
    }

    /// <summary>
    ///   <para>
    ///   Check if the given 'name' is the valid variable name or not.
    ///   </para>
    /// </summary>
    /// <param name="name">Variable name</param>
    /// <returns>True if the 'name' is valid, False if the 'name' is invalid</returns>
    private static bool IsValidVariable(string name)
    {
        Match match = Regex.Match(name, @"^[a-zA-Z_]+[0-9]+$");
        if (!match.Success)
            return false;
        return true;
    }

    /// <summary>
    ///   <para>
    ///   The function that finds the value of a cell given its string name.
    ///   </para>
    /// </summary>
    /// <param name="name"> The name of a given cell.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private double Lookup(string name)
    {
        if (!cells.ContainsKey(name))
            throw new ArgumentException();
        object value = cells[name].Value;
        if (!(value is double))
            throw new ArgumentException();
        return (double)value;
    }

    private bool checkingDirectory(string filename)
    {
        if (Regex.IsMatch(filename, @"\\{1}"))
        {
            string directWOfilename = Regex.Replace(filename, @"[^\\]+$", "");
            string folderDirect = directWOfilename.Substring(0, directWOfilename.Length - 2);
            return Directory.Exists(folderDirect);
        }
        else
        {
            return true;
        }
    }
    private bool isFor(string content)
    {
        if (content.StartsWith("="))
        {
            return true;
        }
        else
        {
            Formula temp = new Formula(content.Replace(" ", ""), Normalize, IsValid);

            foreach (string var in temp.GetVariables())
                if (!IsValidVariable(var))
                    return false;
            return true;
        }
    }
}
