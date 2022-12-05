using System;
using System.ComponentModel.DataAnnotations;

namespace AppointmentApi.DTO
{
    public class CalendarDTO
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Notes { get; set; }
    }
}
