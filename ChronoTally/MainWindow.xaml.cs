using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using OfficeOpenXml;

namespace ChronoTally
{
    public partial class MainWindow : Window
    {
        private double totalHours = 0; // Store the total hours balance
        private const string excelFilePath = "WorkReport.xlsx"; // Path to the Excel file

        public MainWindow()
        {
            InitializeComponent();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set the license context for EPPlus
        }

        private void BtnAddEntry_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Parse start and finish times
                TimeSpan startTime = TimeSpan.ParseExact(txtStartTime.Text, @"hh\:mm", CultureInfo.InvariantCulture);
                TimeSpan finishTime = TimeSpan.ParseExact(txtFinishTime.Text, @"hh\:mm", CultureInfo.InvariantCulture);

                // Calculate hours worked
                double hoursWorked = (finishTime - startTime).TotalHours;

                // Update total hours balance
                totalHours += hoursWorked;

                // Update the time balance display
                UpdateTimeBalanceDisplay();

                // Save entry to file
                SaveEntry(DateTime.Now, txtStartTime.Text, txtFinishTime.Text, txtDescription.Text, hoursWorked);

                // Optionally clear inputs for new entry
                txtStartTime.Clear();
                txtFinishTime.Clear();
                txtDescription.Clear();
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter valid times in HH:mm format.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveEntry(DateTime date, string startTime, string finishTime, string description, double hoursWorked)
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
                worksheet.Cells[rowCount + 1, 1].Value = date.ToString("dd/MM/yyyy");
                worksheet.Cells[rowCount + 1, 2].Value = startTime;
                worksheet.Cells[rowCount + 1, 3].Value = finishTime;
                worksheet.Cells[rowCount + 1, 4].Value = description;
                worksheet.Cells[rowCount + 1, 5].Value = hoursWorked;

                package.Save();
            }

            MessageBox.Show("Entry saved to Excel.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLoadFromExcel_Click(object sender, RoutedEventArgs e)
        {
            FileInfo fileInfo = new FileInfo(excelFilePath);
            if (!fileInfo.Exists)
            {
                MessageBox.Show("No Excel file found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            totalHours = 0; // Reset the total hours
            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++) // Start from row 2 to skip header
                {
                    totalHours += Convert.ToDouble(worksheet.Cells[row, 5].Value);
                }
            }

            UpdateTimeBalanceDisplay();
            MessageBox.Show("Data loaded from Excel.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnGenerateWeeklyReport_Click(object sender, RoutedEventArgs e)
        {
            GenerateReport(TimePeriod.Week);
        }

        private void BtnGenerateMonthlyReport_Click(object sender, RoutedEventArgs e)
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

        private void UpdateTimeBalanceDisplay()
        {
            lblTimeBalance.Text = $"Total Balance: {totalHours} hours";

            if (totalHours > 0)
            {
                lblTimeBalance.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Green);
            }
            else if (totalHours < 0)
            {
                lblTimeBalance.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Red);
            }
            else
            {
                lblTimeBalance.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }
    }

    enum TimePeriod
    {
        Week,
        Month
    }
}