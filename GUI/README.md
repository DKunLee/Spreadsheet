```
Author:     Nikiel Meeks
Partner:    DK Lee
Course:     CS 3500, University of Utah, School of Computing
GitHub ID:  Nikiel-M03
Repo:       https://github.com/uofu-cs3500-spring24/assignment-six-gui-functioning-spreadsheet-jimdestgermaingoat.git
Date:       March 1, 2024
Project:    GUI
Copyright:  CS 3500, Nikiel Meeks, DK Lee - This work may not be copied for use in
Academic Coursework.
```

# Comments to Evaluators:
For opening a file from a file explorer our spreadhsheet had issues. We talked to
Professor De St. Germain and together we couldn't figure out a solution. We implemented a
work around by adding a pop-up menu that you can type the file path of the file that
wants to be loaded. Another issue that we had run into is that the file picker
did not wait during saving. Our work around was allowing the user to provide a name
and a file path for the save destination. It is a known bug that for MAUI on mac that,
for some tools, the await keyword never stops the execution leading to code that shouldn't
run yet run. It does this for FilePicker and DisplayActionSheet. We are also submiting
with a number of rows = 25 in order to help running times as our comuters were struggling
to run the program.

# Assignment Specific Topics

    Partnership

        Nikiel: Basic cell editing and Selected Cell Bar
        DK: Top-Label scrolling, the start of file opening and alert displaying.
        Pair-Programmed: Everything Else
        We have a decent amount of work done not in pair programming because of
        injuries that made it hard to get together early during the project.

    Brancking

        We made no extra branches as a team. We communicated when we were pushing and pulling
        while being specific what we were working on while we were not pair programming.

    Additional Features

        The additional features we made were an undo and a redo button. If changes were made and
        the undo button is clicked, the changes will be reverted. If the user has clicked undo and
        not made any further changes, the redo button can be clicked to bring back the changes. If no
        changes were made to the spreadsheet, nothing will happen. If the redo button is clicked when
        nothing has been undone, nothing will happen.

    Time Tracking

        Tools/Techniques :                      4
        Progession:                             8
        Debugging Errors:                       7
        Maui/Mac Problems (needed workarounds): 5

        Our estimates were getting better as we understood c# and Visual Studio on mac.
        However, for this assignment we were way of because we underestimated the limitations
        of our lack of MAUI knowledge and that having macs for visual studios is not
        the best. From this we learned that knowledge of our abilities along with the 
        the complexity of the task is what will give us our best estimates. For our
        first assignments we were more familiar with normal C# API. For this assignment,
        our lack understanding of MAUI lead us to give inaccurate estimates of our time
        expenditure. 

    Best Practices

        
        During a particularly effective collaboration, my partner and I found ourselves seated in the library,
        working on solutions face-to-face. This proximity allowed for immediate and open communication,
        fostering a quick exchange of ideas and solutions. We could discuss code snippets, troubleshoot
        challenges together, and make real-time decisions. The close collaboration in this scenario
        significantly expedited problem-solving, making the coding process faster and leading to a more
        coherent and error-free codebase.

        Conversely, we faced a challenge when working on a spreadsheet project while physically separated.
        Attempting to push and pull changes simultaneously resulted in conflicts and code discrepancies,
        impeding progress. It became evident that a lack of synchronization was hindering our efficiency.
        From this experience, we recognized the importance of adopting pair programming practices even
        when physically apart. By learning from our mistakes, we implemented a more structured approach,
        embracing tools that facilitated collaborative coding and prevented conflicts. This adjustment not
        only addressed the issue at hand but also reinforced the notion that effective partnership
        involves not just coordination but also adopting the right collaborative methodologies for
        different scenarios.
        

# Consulted Peers:

    1 Montader
    2 Didar
    3 Micah
    4 Adam
    5 Milo
    6 Quinn
    7 Mark (former Student)

# References:

    1 Microsoft C# API
    2 Regex101 https://regex101.com/
    3 ChatGPT
    4 StackOverflow
    5 Github
