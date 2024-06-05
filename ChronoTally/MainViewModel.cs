﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Windows;

namespace ChronoTally.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<WorkEntry> WorkEntries { get; set; } = new ObservableCollection<WorkEntry>();

        private double _totalHours;
        public double TotalHours
        {
            get => _totalHours;
            set
            {
                _totalHours = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TimeBalance));
            }
        }

        public string TimeBalance => $"Total Balance: {TotalHours} hours";

        public ICommand AddEntryCommand { get; }
        public ICommand SaveToExcelCommand { get; }
        public ICommand LoadFromExcelCommand { get; }
        public ICommand GenerateWeeklyReportCommand { get; }
        public ICommand GenerateMonthlyReportCommand { get; }

        private const string excelFilePath = "WorkReport.xlsx";

        public MainViewModel()
        {
            AddEntryCommand = new RelayCommand(AddEntry);
            SaveToExcelCommand = new RelayCommand(SaveToExcel);
            LoadFromExcelCommand = new RelayCommand(LoadFromExcel);
            GenerateWeeklyReportCommand = new RelayCommand(GenerateWeeklyReport);
            GenerateMonthlyReportCommand = new RelayCommand(GenerateMonthlyReport);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        private void AddEntry(object parameter)
        {
            if (parameter is WorkEntry entry)
            {
                WorkEntries.Add(entry);
                TotalHours += entry.HoursWorked;
            }
        }

        private void SaveToExcel(object parameter)
        {
            FileInfo fileInfo = new FileInfo(excelFilePath);
            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Count == 0 ? package.Workbook.Worksheets.Add("WorkReport") : package.Workbook.Worksheets[0];

                if (worksheet.Dimension == null)
                {
                    worksheet.Cells[1, 1].Value = "Date";
                    worksheet.Cells[1, 2].Value = "Start Time";
                    worksheet.Cells[1, 3].Value = "Finish Time";
                    worksheet.Cells[1, 4].Value = "Description";
                    worksheet.Cells[1, 5].Value = "Hours Worked";
                }

                int rowCount = worksheet.Dimension?.Rows ?? 0;
                foreach (var entry in WorkEntries)
                {
                    worksheet.Cells[rowCount + 1, 1].Value = entry.Date.ToString("dd/MM/yyyy");
                    worksheet.Cells[rowCount + 1, 2].Value = entry.StartTime.ToString(@"hh\:mm");
                    worksheet.Cells[rowCount + 1, 3].Value = entry.FinishTime.ToString(@"hh\:mm");
                    worksheet.Cells[rowCount + 1, 4].Value = entry.Description;
                    worksheet.Cells[rowCount + 1, 5].Value = entry.HoursWorked;
                    rowCount++;
                }

                package.Save();
            }

            MessageBox.Show("Entries saved to Excel.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadFromExcel(object parameter)
        {
            FileInfo fileInfo = new FileInfo(excelFilePath);
            if (!fileInfo.Exists)
            {
                MessageBox.Show("No Excel file found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            WorkEntries.Clear();
            TotalHours = 0;

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    var date = DateTime.ParseExact(worksheet.Cells[row, 1].Text, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    var startTime = TimeSpan.ParseExact(worksheet.Cells[row, 2].Text, @"hh\:mm", CultureInfo.InvariantCulture);
                    var finishTime = TimeSpan.ParseExact(worksheet.Cells[row, 3].Text, @"hh\:mm", CultureInfo.InvariantCulture);
                    var description = worksheet.Cells[row, 4].Text;

                    var entry = new WorkEntry
                    {
                        Date = date,
                        StartTime = startTime,
                        FinishTime = finishTime,
                        Description = description
                    };

                    WorkEntries.Add(entry);
                    TotalHours += entry.HoursWorked;
                }
            }

            MessageBox.Show("Data loaded from Excel.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GenerateWeeklyReport(object parameter)
        {
            GenerateReport(TimePeriod.Week);
        }

        private void GenerateMonthlyReport(object parameter)
        {
            GenerateReport(TimePeriod.Month);
        }

        private void GenerateReport(TimePeriod period)
        {
            FileInfo fileInfo = new FileInfo(excelFilePath);
            if (!fileInfo.Exists)
            {
                MessageBox.Show("No Excel file found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                var entries = worksheet.Cells[2, 1, worksheet.Dimension.End.Row, 5]
                                .Select(cell => new
                                {
                                    Date = DateTime.ParseExact(worksheet.Cells[cell.Start.Row, 1].Text, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    HoursWorked = Convert.ToDouble(worksheet.Cells[cell.Start.Row, 5].Value)
                                })
                                .ToList();

                var groupedEntries = period == TimePeriod.Week
                    ? entries.GroupBy(e => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(e.Date, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                    : entries.GroupBy(e => new { e.Date.Year, e.Date.Month });

                string reportFilePath = period == TimePeriod.Week ? "WeeklyReport.xlsx" : "MonthlyReport.xlsx";
                FileInfo reportFile = new FileInfo(reportFilePath);
                using (ExcelPackage reportPackage = new ExcelPackage(reportFile))
                {
                    ExcelWorksheet reportSheet = reportPackage.Workbook.Worksheets.Count == 0 ? reportPackage.Workbook.Worksheets.Add("Report") : reportPackage.Workbook.Worksheets[0];

                    int row = 1;
                    foreach (var group in groupedEntries)
                    {
                        reportSheet.Cells[row, 1].Value = period == TimePeriod.Week ? $"Week {group.Key}" : $"{group.Key.Year}-{group.Key.Month}";
                        reportSheet.Cells[row, 2].Value = group.Sum(e => e.HoursWorked);
                        row++;
                    }

                    reportPackage.Save();
                }

                MessageBox.Show($"{(period == TimePeriod.Week ? "Weekly" : "Monthly")} report generated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum TimePeriod
    {
        Week,
        Month
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}