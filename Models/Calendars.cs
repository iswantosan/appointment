using System;
using System.Collections.Generic;

namespace AppointmentApi.Models
{
    public partial class Calendars
    {
        public int CalendarId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
