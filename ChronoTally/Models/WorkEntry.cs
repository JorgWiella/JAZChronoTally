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
        public TimeSpan FinishTime { get; set; }
        public string Description { get; set; }

        public double HoursWorked => (FinishTime - StartTime).TotalHours;
    }
}
