using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ChronoTally.Models
{
    public class WorkEntry
    {
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Description { get; set; }
        public bool IsBalanceEntry { get; set; }

        public double HoursWorked => IsBalanceEntry ? 0 : (EndTime - StartTime).TotalHours;
        public string Value => IsBalanceEntry ? Description : HoursWorked.ToString("F2");
    }
}
