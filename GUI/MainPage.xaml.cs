namespace GUI;
using System.Text.RegularExpressions;
using SS;
using SpreadsheetUtilities;
/// <summary>
/// Author:    Nikiel Meeks
/// Partner:   DK Lee
/// Date:      March 1, 2024
/// Course:    CS 3500, University of Utah, School of Computing
/// Copyright: CS 3500 and Nikiel Meeks - This work may not 
///            be copied for use in Academic Coursework.
///
/// I, Nikiel Meeks and DK Lee, certify that we wrote this code from scratch and
/// did not copy it in part or whole from another source.  All 
/// references used in the completion of the assignments are cited 
/// in my README file.
///
/// File Contents
///
/// This class represents a spreadsheet in a GUI format. This spreadsheet can take
/// input values into each entry. These unputs can be strings, doubles, or formulas.
/// When a cell is selected the Name, Value, and Contents of that cell are shown in
/// the top left (in that order).
/// 
/// </summary>
public partial class MainPage : ContentPage
{
    /// <summary>
    /// The number of rows in the spreadsheet
    /// </summary>
    private readonly int numRows = 25;

    /// <summary>
    /// The number of columns in the spreadsheet.
    /// </summary>
    private readonly int numCols = 26;

    /// <summary>
    /// The name of the file that represents the saved version of this spreadsheet.
    /// </summary>
    private string fileName;

    /// <summary>
    /// The path to the file that represents the saved version of this spreadsheet
    /// </summary>
    private string filePath;

    /// <summary>
    /// The storage of previous edits in order to undo them.
    /// </summary>
    private Stack<Cell> Undo;

    /// <summary>
    /// The storage of previous undone edits in order to redo them.
    /// </summary>
    private Stack<Cell> Redo;

    /// <summary>
    /// The background for the spreadsheet that manages the contents of each cell,
    /// each cells dependencies, and whether given inputs are valid.
    /// </summary>
    private AbstractSpreadsheet ss;

    /// <summary>
    /// The current cell that is selected.
    /// </summary>
    private Entry currentCell;

    /// <summary>
    /// A delegate event for clearing
    /// </summary>
    private delegate void Clear();


    /// <summary>
    /// This will be run when the spreadsheet app opened
    /// </summary>
    public class SpreadsheetEntry : Entry
    {
        public string Name { get; set; }

        public void Reset()
        { Text = ""; }
    }

    /// <summary>
    /// This is the main page of the GUI program. This build the actual
    /// grid and makes each cell can work properly and keep update the
    /// grid with the new cell values and contents.
    /// </summary>
    public MainPage()
    {
        InitializeComponent();
        initializeTheFieldVariable();

        GenerateHeaders();

        for (int i = 1; i <= numRows; i++)
        {
            HorizontalStackLayout row = new();

            for (int j = 0; j < numCols; j++)
            {
                Entry entry = new SpreadsheetEntry()
                {
                    Name = $"{(char)('A' + j)}" + i,
                    WidthRequest = 75,
                    HeightRequest = 40,
                };

                // Update the changes that made in spreadsheet
                entry.Completed += Refocused;
                entry.Focused += SelectedCell;
                entry.Unfocused += ChangedCell;
                ClearAll += ((SpreadsheetEntry)entry).Reset;
                row.Add(entry);
            }

            Grid.Add(row);
        }

        SetDefaultCell();
    }

    /// <summary>
    /// Clears every entry in the spreadsheet.
    /// </summary>
    private event Clear ClearAll;

    /// <summary>
    /// This event handler change refocuses the cell when it is completed
    /// </summary>
    /// <param name="sender">_</param>
    /// <param name="e">_</param>
    void Refocused(object sender, EventArgs e)
    {
        ((Entry)sender).Focus();
    }

    /// <summary>
    /// This event handler get the information of selected cell
    /// </summary>
    /// <param name="sender">_</param>
    /// <param name="e">_</param>
    void SelectedCell(object sender, EventArgs e)
    {
        SpreadsheetEntry entry = (SpreadsheetEntry)sender;
        currentCell = entry;
        string cellName = entry.Name;

        // Get the information of selected cell
        selectedCell.Text = cellName;
        cellValue.Text = getttingCellValue(cellName);
        cellContents.Text = ss.GetCellContents(cellName).ToString();
    }

    /// <summary>
    /// This event handler operates when the content of cell has
    /// been changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void ContentsChanged(object sender, EventArgs e)
    {
        Entry entry = (Entry)sender;
        string cellName = selectedCell.Text;


        settingCellContents(entry, cellName);
        string cellValue = getttingCellValue(cellName);
        this.cellValue.Text = cellValue;
        currentCell.Text = cellValue;


        int indexOfRow = int.Parse(cellName.Substring(1)) - 1;

        HorizontalStackLayout row = (HorizontalStackLayout)Grid[indexOfRow];
        SpreadsheetEntry cell = (SpreadsheetEntry)row[cellName[0] - 'A'];


        resetRedoStack();

        cell.Focus();

    }

    /// <summary>
    /// This updates the cell and its contents when it is unfocused.
    /// </summary>
    /// <param name="sender">_/param>
    /// <param name="e">_</param>
    void ChangedCell(object sender, EventArgs e)
    {
        SpreadsheetEntry entry = (SpreadsheetEntry)sender;
        string name = entry.Name;
        settingCellContents(entry, name);
        entry.Text = getttingCellValue(name);

        resetRedoStack();
    }

    /// <summary>
    /// This event handler gets the information of how the TopLabels scrolled.
    /// Since, the TopLabel supposed to scrolled left or right depends on how
    /// grid scrolled, this allows to make the TopLabel moved opposite way which
    /// is the same way of grid.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void TopLabels_Scroll(object sender, ScrolledEventArgs e)
    {
        TopLabels.TranslationX = -e.ScrollX;
    }

    // File menu event handler-------------------------------------------------------

    /// <summary>
    /// This file menu button allows user to make the new spreadsheet.
    /// If there are some changes but user clicked the 'New' button and it's not
    /// saved, it will ask user to save or not.
    /// </summary>
    /// <param name="sender">_</param>
    /// <param name="e">_</param>
    void FileMenuNew(object sender, EventArgs e)
    {
        askUserToSaveTheirWork(sender, e);

        fileName = null;
        filePath = null;

        ClearAll();
        Undo = new();
        Redo = new();
        ss = new Spreadsheet(IsValid, Normalize, "six");
        SetDefaultCell();
    }

    /// <summary>
    /// This file menu button allows user to open the existing spreadsheet with
    /// the name and path of the file.
    /// </summary>
    /// <param name="sender">_</param>
    /// <param name="e">_</param>
    async void FileMenuOpenAsync(object sender, EventArgs e)
    {
            askUserToSaveTheirWork(sender, e);

        try
        {

            string usersName = await DisplayPromptAsync("Type File Name", "File Name (including file type)");
            string usersPath = await DisplayPromptAsync("Type File Path", "File Path (excluding file name)");

            if (usersName != null && usersPath != null)
            {
                string result = usersPath + "/" + usersName;

                if (!result.EndsWith(".sprd")) throw new SpreadsheetReadWriteException("" +
                "The file must be of the type \".sprd\".");

                //fileName shouldn't include type
                fileName = usersName.Remove(usersName.Count() - 5);
                filePath = usersPath;

                ClearAll();
                Undo = new();
                Redo = new();
                ss = new Spreadsheet(result, IsValid, Normalize, "six");
                RebuildGridBasedOnCurrentSSInfo();
                SetDefaultCell();
            }
            else
            {
                if (await DisplayAlert("Alert", "No file was selected.", "TryAgain", "Cancel"))
                {
                    FileMenuOpenAsync(sender, e);
                }
            }
        }
        catch (SpreadsheetReadWriteException ex)
        {

            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "Continue");
        }
    }

    /// <summary>
    /// This file menu button allows user to save their spreadsheet to the
    /// privided path.
    /// * We could't use FilePicker on mac, so implemented different way
    /// * Wath README.md for more details.
    /// </summary>
    /// <param name="sender">_</param>
    /// <param name="e">_</param>
    void FileMenuSaveAsync(object sender, EventArgs e)
    {
        if (fileName != null)
        {
            ss.Save(filePath + "/" + fileName + ".sprd");
        }
        else
        {
            FileMenuSaveAsAsync(sender, e);
        }
    }

    /// <summary>
    /// This file button allows user to save the spreadsheet with different
    /// name and different path.
    /// * We could't use FilePicker on mac, so implemented different way
    /// * Wath README.md for more details.
    /// </summary>
    /// <param name="sender">_</param>
    /// <param name="e">_</param>
    async void FileMenuSaveAsAsync(object sender, EventArgs e)
    {
        try
        {
            string usersName = await DisplayPromptAsync("Type File Name", "File Name");
            string usersPath = await DisplayPromptAsync("Type File Path", "File Path");

            string finalPath = usersPath + "/" + usersName + ".sprd";

            if (File.Exists(finalPath))
            {
                if (!await DisplayAlert("Alert", "Are you sure you want to overwrite this file", "Yes", "No")) return;
            }

            if (usersName != null && usersPath != null)
            {
                fileName = usersName;
                filePath = usersPath;

                ss.Save(finalPath);
            }
            else
            {
                if (await DisplayAlert("Alert", "No file was selected.", "TryAgain", "Cancel"))
                {
                    FileMenuSaveAsync(sender, e);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "Continue");
        }
    }

    /// <summary>
    /// This button in menu help section provides the information about how to
    /// change the selection.
    /// </summary>
    /// <param name="sender">_</param>
    /// <param name="e">_</param>
    async void ChangingSelectionsHelp(object sender, EventArgs e)
    {
        await DisplayAlert("Changing Selections", "In order to change cells, the user" +
            " can click on the grid cell they want to select. For users on a computer," +
            " tab can also be used to move to the cell to the right.", "Ok");
    }

    /// <summary>
    /// This button in menu help section provides the information about how to
    /// change the content of cell.
    /// </summary>
    /// <param name="sender">_</param>
    /// <param name="e">_</param>
    async void ChangingCellContentsHelp(object sender, EventArgs e)
    {
        await DisplayAlert("Changing Cell Contents", "There are two ways to change cell" +
            " contents. Firstly, the user can type into the cell in the grid to change the" +
            " contents. Secondly, the user can edit the contents bar to change the cells contents.", "Ok");
    }

    /// <summary>
    /// This button in menu help section provides the information about how to
    /// save the spreadsheet.
    /// </summary>
    /// <param name="sender">_</param>
    /// <param name="e">_</param>
    async void SavingHelp(object sender, EventArgs e)
    {
        await DisplayAlert("Saving", "When saving the spreadsheet for the first time or" +
            " saving the spreadhseet as a certain file, the user needs to put the name of" +
            " the file and the path of the file. The user will also be prompted if they" +
            " are overwriting a file. If the user is saving after a path is given, then" +
            " the spreadsheet will be saved to the already given path.", "Ok");
    }

    /// <summary>
    /// This button in menu help section provides the information about how to
    /// create new spreadsheet.
    /// </summary>
    /// <param name="sender">_</param>
    /// <param name="e">_</param>
    async void CreatingNewSpreasheetsHelp(object sender, EventArgs e)
    {
        await DisplayAlert("Creating a New Spreadsheet", "The user has two options to create or" +
            " open a new spreadsheet. The user can create a brand new spreadsheet with no data." +
            " The user can also open a spreadsheet from an existing file. This file must fit the" +
            " proper XML format and the file type must be \".sprd\". Before the user creates a" +
            " new spreadsheet, the user is asked if they want to save.", "Ok");
    }

    /// <summary>
    /// This button in menu help section provides the information about what is
    /// the formula that available in this spreadsheet.
    /// </summary>
    /// <param name="sender">_</param>
    /// <param name="e">_</param>
    async void FormulasHelp(object sender, EventArgs e)
    {
        await DisplayAlert("Formulas", "Formulas can be added to cells by first entering \"=\"" +
            " then a formula in the proper syntax. A formula can do operations that involve the" +
            " operators +, -, *, /, (, and ). A formula must also be complete (it must be in valid" +
            " math syntax). If it is not complete, a warning will be provided to the user and the formula" +
            " will not be added to the spreadsheet.", "Ok");
    }

    /// <summary>
    /// This event handler allows user to undo what just the user do.
    /// For example, if user clicks the undo after they write something
    /// on spreadsheet, it puts it back to previous status.
    /// </summary>
    /// <param name="sender">_</param>
    /// <param name="e">_</param>
    void ClickedUndo(object sender, EventArgs e)
    {
        if (Undo.Count == 0)
            return;

        Cell cell = Undo.Pop();
        string name = cell.Name;
        Cell oldCell = new Cell(name, ss.GetCellContents(name));
        Redo.Push(oldCell);
        ss.SetContentsOfCell(name, cell.Contents.ToString());
        HorizontalStackLayout row = (HorizontalStackLayout)Grid[int.Parse(name.Substring(1)) - 1];
        SpreadsheetEntry entry = (SpreadsheetEntry)row[name[0] - 'A'];
        string value = getttingCellValue(name);
        entry.Text = value;
        selectedCell.Text = name;
        cellContents.Text = ss.GetCellContents(name).ToString();
        cellValue.Text = value;
        entry.Focus();
    }

    /// <summary>
    /// This event handler allows user to redo and put the spreadsheet cell
    /// back which undo before. For example, if the user undo something but
    /// wants to rewind the thing, this allows user to rewind and put the
    /// cell value and content back to cell. However, if user changes somthing
    /// after they undo, then our spreadsheet wouldn't allow user to recall the
    /// cell, since the new history has been made.
    /// </summary>
    /// <param name="sender">_</param>
    /// <param name="e">_</param>
    void ClickedRedo(object sender, EventArgs e)
    {
        if (Redo.Count == 0)
            return;

        Cell cell = Redo.Pop();
        string name = cell.Name;
        Cell oldCell = new Cell(name, ss.GetCellContents(name));
        Undo.Push(oldCell);
        ss.SetContentsOfCell(name, cell.Contents.ToString());
        HorizontalStackLayout row = (HorizontalStackLayout)Grid[int.Parse(name.Substring(1)) - 1];
        SpreadsheetEntry entry = (SpreadsheetEntry)row[name[0] - 'A'];
        string value = getttingCellValue(name);
        entry.Text = value;
        selectedCell.Text = name;
        cellContents.Text = ss.GetCellContents(name).ToString();
        cellValue.Text = value;
        entry.Focus();
    }

    // Private helper methods------------------------------------------------------------------------------------

    /// <summary>
    /// When the spreadsheet opened, this method will initialize the variables
    /// in feild.
    /// Add more initialization, if the new functionality needs.
    /// </summary>
    private void initializeTheFieldVariable()
    {
        ss = new Spreadsheet(IsValid, Normalize, "six");
        Undo = new Stack<Cell>();
        Redo = new Stack<Cell>();
    }

    /// <summary>
    /// This private helper method is to check whether the name of
    /// variable is valid or not. In this spreadsheet, the only valid
    /// variable name is lower-letter or upper-letter or an underscore
    /// following by less then 3 digits number.
    /// </summary>
    /// <param name="name">Name of the variable</param>
    /// <returns>True, if it's valid name. False, if it's invalid name</returns>
    private bool IsValid(string name)
    {
        return Regex.IsMatch(name, @"^[a-zA-Z_]([0-9]|[0-9][0-9])$");
    }

    /// <summary>
    /// This private helper method is for spreadsheet normalizer.
    /// All cell names have to be stored as a upper letter status,
    /// so this helper method helps to convert the all cell name.
    /// Nomatter how user puts cell name like lower letter, it will
    /// convert the cell name.
    ///
    /// Examples: a1 => A1 , B1 => B1
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private string Normalize(string name)
    {
        return name.ToUpper();
    }

    /// <summary>
    /// Get the value of the cell
    /// Value might be double, string, and Formula Error.
    /// If it's double it will return the string type of double.
    /// </summary>
    /// <param name="name"></param>
    /// <returns>String version of value or FormulaError if there is error</returns>
    private string getttingCellValue(string name)
    {
        object value = ss.GetCellValue(name);
        if (ss.GetCellValue(name) is FormulaError)
        {
            return ((FormulaError)value).Reason;
        }
        else
        {
            return value.ToString();
        }
    }

    /// <summary>
    /// This private helper method reset the redo stack
    /// </summary>
    private void resetRedoStack()
    {
        if (Redo.Count != 0)
            Redo.Clear();
    }

    /// <summary>
    /// This private helper method will rebuild the grid of the
    /// spreadsheet with current information of ss. 
    /// </summary>
    private void RebuildGridBasedOnCurrentSSInfo()
    {
        for (int i = 0; i < numRows; i++)
        {
            HorizontalStackLayout row = (HorizontalStackLayout)Grid[i];

            for (int j = 0; j < numCols; j++)
            {
                SpreadsheetEntry entry = (SpreadsheetEntry)row[j];
                string cellName = $"{(char)('A' + j)}" + (i + 1); // Adjusted for 1-based indexing
                entry.Text = ss.GetCellValue(cellName).ToString();
            }
        }
    }

    /// <summary>
    /// This private helper method sets the current cell as 'A1' which
    /// is default status of spreadsheet.
    /// </summary>
    private void SetDefaultCell()
    {
        currentCell = (SpreadsheetEntry)((HorizontalStackLayout)Grid[0])[0];
        currentCell.Focus();
    }

    /// <summary>
    /// This helper method re_set the contents of cell after the cell has
    /// been changed.
    /// </summary>
    /// <param name="entry">_</param>
    /// <param name="name">Name of the cell</param>
    private async void settingCellContents(Entry entry, string name)
    {
        try
        {
            Cell cell = new Cell(name, ss.GetCellContents(name));
            Undo.Push(cell);
            IList<string> unupdatedCells = ss.SetContentsOfCell(name, entry.Text);
            unupdatedCells.Remove(name);
            updateCelldependenciesInGrid(unupdatedCells);
        }
        catch (FormulaFormatException ex)
        {
            await DisplayAlert("Error", "Formula Format Error: " + ex.Message, "Undo change");
        }
        catch (CircularException)
        {
            await DisplayAlert("Error", "The given formula causes a cycle", "Undo change");
        }
    }

    /// <summary>
    /// This private helper method pops up the alert message to user whether
    /// they want to save this spreadsheet or not.
    /// </summary>
    /// <param name="sender">_</param>
    /// <param name="e">_</param>
    private async void askUserToSaveTheirWork(object sender, EventArgs e)
    {
        if (ss.Changed)
        {
            if (await DisplayAlert("Alert", "Do you want to save?", "Yes", "No"))
            {
                if (fileName == null)
                {
                    FileMenuSaveAsAsync(sender, e);
                }
                else
                {
                    FileMenuSaveAsync(sender, e);
                }
            }
        }
    }

    /// <summary>
    /// This private helper method updates the dependency of given cell
    /// </summary>
    /// <param name="names">Names of cell have to be updated</param>
    private void updateCelldependenciesInGrid(IList<string> names)
    {
        foreach (string name in names)
        {
            HorizontalStackLayout row = (HorizontalStackLayout)Grid[int.Parse(name.Substring(1)) - 1];
            SpreadsheetEntry entry = (SpreadsheetEntry)row[name[0] - 'A'];
            entry.Text = ss.GetCellValue(name).ToString();
        }
    }

    /// <summary>
    /// This private helper method generate the headers of the
    /// spreadsheet when it called. Our spreadsheet always has
    /// to be same loolking. This will generate the top-label
    /// and the background of the grid.
    /// </summary>
    private void GenerateHeaders()
    {
        TopLabels.Add(new Border
        {
            Stroke = Color.FromRgb(0, 0, 0),
            StrokeThickness = 1,
            HeightRequest = 20,
            WidthRequest = 75,
            HorizontalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = $"",
                BackgroundColor = Color.FromRgb(200, 200, 250),
                HorizontalTextAlignment = TextAlignment.Center
            }
        });

        for (int i = 0; i < numCols; i++)
        {
            TopLabels.Add(new Border
            {
                Stroke = Color.FromRgb(0, 0, 0),
                StrokeThickness = 1,
                HeightRequest = 20,
                WidthRequest = 75,
                HorizontalOptions = LayoutOptions.Center,
                Content = new Label
                {
                    Text = $"{(char)('A' + i)}",
                    BackgroundColor = Color.FromRgb(200, 200, 250),
                    HorizontalTextAlignment = TextAlignment.Center
                }
            });

        }

        for (int i = 1; i <= numRows; i++)
        {
            HorizontalStackLayout row = new()
            {
                new Border
                {
                    Stroke = Color.FromRgb(0, 0, 0),
                    StrokeThickness = 1,
                    HeightRequest = 40,
                    WidthRequest = 75,
                    HorizontalOptions = LayoutOptions.Center,
                    Content = new Label
                              {
                                  Text = $"{i}",
                                  BackgroundColor = Color.FromRgb(200, 200, 250),
                                  HorizontalTextAlignment = TextAlignment.Center
                              }
                }
            };

            LeftLabels.Add(row);
        }
    }
}