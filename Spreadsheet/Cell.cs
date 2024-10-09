/// <summary>
///   <para>
///   This is the private Cell class that the spreadsheet has. The main
///   part of this class is to hold the Contents and Value.
///   </para>
///   <para>
///   The contents of a cell can be (1) a string, (2) a double, or (3) a
///   Formula. If the contents is an empty string, we say that the cell is
///   empty. (By analogy, the contents of a cell in Excel is what is
///   displayed on the editing line when the cell is selected.)
///   </para>
///   <para>
///   The value of a cell can be (1) a string, (2) a double, or (3) a
///   FormulaError. (By analogy, the value of an Excel cell is what is
///   displayed in that cell's position in the grid.)
///   </para>
///   <list type="number">
///     <item>If a cell's contents is a string, its value is that string.</item>
///     <item>If a cell's contents is a double, its value is that double.</item>
///     <item>
///        If a cell's contents is a Formula, its value is either a double or
///        a FormulaError, as reported by the Evaluate method of the Formula
///        class. The value of a Formula, of course, can depend on the values
///        of variables. The value of a variable is the value of the spreadsheet
///        cell it names (if that cell's value is a double) or is undefined
///        (otherwise).
///     </item>
///   </list>
/// </summary>

using SpreadsheetUtilities;
using System.Text.RegularExpressions;
using System.Xml;

namespace SS;

    
public class Cell
{
    public string Name { get; set; }
    public object Contents { get; set; }
    public object Value { get; set; }

    /// <summary>
    ///   <para>
    ///   When the cell called with input parameter, the Contents will be
    ///   a given input either double, string, Formula.
    ///   </para>
    /// </summary>
    public Cell(string name, object contents, Func<string, double> lookup)
    {
        if (isValidName(name))
        {
            Name = name;
            Contents = contents;

            if(contents is Formula) Value = ((Formula)contents).Evaluate(lookup);
            else Value = contents;
        }
        else
        {
            throw new InvalidNameException();
        }
    }

    public Cell(string name, object contents)
    {
        if (isValidName(name))
        {
            Name = name;
            Contents = contents;
            Value = contents;   
        }
        else
        {
            throw new InvalidNameException();
        }
    }

    /// <summary>
    /// Writes this Cell to an existing XmlWriter
    /// </summary>
    /// <param Name="writer">The existing Xml writer to print to.</param>
    public void WriteXml(XmlWriter writer)
    {
        writer.WriteStartElement("cell");
        writer.WriteElementString("name", Name);
        if (Contents is Formula)
            writer.WriteElementString("contents", "=" + Contents.ToString());
        else
            writer.WriteElementString("contents", Contents.ToString());
        writer.WriteEndElement(); 
    }

    private bool isValidName(string name)
    {
        Match match = Regex.Match(name, @"^[a-zA-Z_]+[0-9]+$");
        return match.Success;
    }
}