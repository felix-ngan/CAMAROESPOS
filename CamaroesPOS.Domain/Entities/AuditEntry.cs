using System;

namespace CamaroesPOS.Domain.Entities
{
    public class AuditEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Actor { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string Severity { get; set; } = "Info"; // Info, Warning, Critical
    }
}
