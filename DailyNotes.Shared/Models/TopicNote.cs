using System;
using System.Text.Json;
using DailyNotes.Shared.Interfaces;

namespace DailyNotes.Shared.Models
{
    public class TopicNote : IHasTenantUser
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Visibility { get; set; } = "private";
        public int TopicId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = "{}"; // Storing as string for simplicity in SQLite for now
        public int TimeMinutes { get; set; } = 0; // Study time tracking

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
