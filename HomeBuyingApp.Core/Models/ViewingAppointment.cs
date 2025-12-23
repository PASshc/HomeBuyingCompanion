using System;

namespace HomeBuyingApp.Core.Models
{
    public class ViewingAppointment
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public Property? Property { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
