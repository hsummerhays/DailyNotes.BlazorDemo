using System;
using DailyNotes.Shared.Interfaces;

namespace DailyNotes.Shared.Models
{
    public class Tag : IHasTenant
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#808080"; // Hex color code
    }
}
