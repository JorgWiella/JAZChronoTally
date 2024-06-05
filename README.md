ChronoTally

ChronoTally is a WPF application for tracking work hours and calculating time balance. It allows users to input their daily work hours and descriptions, automatically calculating the total balance of hours (overtime or debt). Additionally, the application can generate weekly and monthly reports in Excel format.

Features
Time Entry: Input start time, finish time, and description for each workday.
Time Balance Calculation: Automatically calculate the total hours and display whether the user has overtime or debt hours.
Excel Integration: Save and load work entries from an Excel file.
Reporting: Generate weekly and monthly reports of work hours.
Requirements
.NET Framework 4.8 or later
Visual Studio 2019 or later
EPPlus library for Excel operations
Installation
Clone the repository:
```
git clone https://github.com/your-username/chronotally.git
```

Open the solution in Visual Studio:
```
cd chronotally
start ChronoTally.sln
```


Restore NuGet packages:

In Visual Studio, go to Tools > NuGet Package Manager > Manage NuGet Packages for Solution, then click Restore to download the necessary packages.

Build the solution:

Press Ctrl + Shift + B to build the solution.

Usage
Run the application:

Press F5 to run the application in debug mode.

Enter your work hours:

Select the date.
Input the start and finish times.
Provide a description of the work done.
Click "Add Entry" to save the entry.
View the time balance:

The total time balance is displayed at the bottom of the application window.

Save and load from Excel:

Click "Save to Excel" to save the current entries to an Excel file.
Click "Load from Excel" to load entries from an existing Excel file.
Generate reports:

Click "Generate Weekly Report" to create a report for the current week.
Click "Generate Monthly Report" to create a report for the current month.
Project Structure
Models:

WorkEntry.cs: Represents a work entry with properties for date, start time, finish time, description, and hours worked.
ViewModels:

MainViewModel.cs: Contains the logic for managing work entries, calculating the total balance, and interacting with Excel.
Views:

MainWindow.xaml: Defines the user interface.
MainWindow.xaml.cs: Code-behind for the user interface.
Contributing
Contributions are welcome! Please feel free to submit a pull request or open an issue to discuss any changes or improvements.

License
This project is licensed under the MIT License. See the LICENSE file for details.

by Jorge Villarreal
