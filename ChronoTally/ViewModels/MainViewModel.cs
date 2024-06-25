﻿using ChronoTally.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ChronoTally.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private DateTime _newEntryDate = DateTime.Today;
        public DateTime NewEntryDate
        {
            get => _newEntryDate;
            set
            {
                _newEntryDate = value;
                OnPropertyChanged();
            }
        }

        private string _startTimeInput = "08:40";
        public string StartTimeInput
        {
            get => _startTimeInput;
            set
            {
                _startTimeInput = value;
                OnPropertyChanged();
            }
        }

        private string _finishTimeInput = "17:20";
        public string FinishTimeInput
        {
            get => _finishTimeInput;
            set
            {
                _finishTimeInput = value;
                OnPropertyChanged();
            }
        }

        private string _descriptionInput = "wat u did";
        public string DescriptionInput
        {
            get => _descriptionInput;
            set
            {
                _descriptionInput = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<WorkEntry> _dailyEntries = new ObservableCollection<WorkEntry>();
        public ObservableCollection<WorkEntry> DailyEntries
        {
            get => _dailyEntries;
            set
            {
                _dailyEntries = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddEntryCommand { get; }

        public MainViewModel()
        {
            AddEntryCommand = new RelayCommand(AddEntry, CanAddEntry);
        }

        private bool CanAddEntry(object parameter) => true;

        private void AddEntry(object parameter)
        {
            if (!TimeSpan.TryParse(StartTimeInput, out TimeSpan startTime) || !TimeSpan.TryParse(FinishTimeInput, out TimeSpan endTime))
            {
                return;
            }

            startTime = new TimeSpan(startTime.Hours, startTime.Minutes, 0);
            endTime = new TimeSpan(endTime.Hours, endTime.Minutes, 0);

            var newEntry = new WorkEntry
            {
                Date = NewEntryDate,
                StartTime = startTime,
                EndTime = endTime,
                Description = DescriptionInput
            };

            DailyEntries.Add(newEntry);
            UpdateWeeklyAndMonthlyTotals();

            NewEntryDate = DateTime.Today;
            StartTimeInput = string.Empty;
            FinishTimeInput = string.Empty;
            DescriptionInput = string.Empty;
        }

        private void UpdateWeeklyAndMonthlyTotals()
        {
            var groupedByWeek = DailyEntries
                .Where(e => !e.IsBalanceEntry)
                .GroupBy(e => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(e.Date, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                .OrderBy(g => g.Key);

            var balanceEntries = new ObservableCollection<WorkEntry>();

            foreach (var group in groupedByWeek)
            {
                foreach (var entry in group)
                {
                    balanceEntries.Add(entry);
                }

                var weeklyHours = group.Sum(e => e.HoursWorked);
                balanceEntries.Add(new WorkEntry
                {
                    IsBalanceEntry = true,
                    Description = $"Weekly Balance: {weeklyHours:F2} hours"
                });
            }

            var monthlyHours = DailyEntries.Where(e => !e.IsBalanceEntry).Sum(e => e.HoursWorked);
            balanceEntries.Add(new WorkEntry
            {
                IsBalanceEntry = true,
                Description = $"Monthly Balance: {monthlyHours:F2} hours"
            });

            DailyEntries = balanceEntries;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
