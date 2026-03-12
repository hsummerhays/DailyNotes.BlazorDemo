using System;
using DailyNotes.Shared.Interfaces;

namespace DailyNotes.Shared.Models
{
    public class Assignment : IHasTenantUser
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int TenantId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = "pending"; // 'pending' | 'submitted' | 'graded'

        public decimal? Grade { get; set; }
        public decimal? MaxGrade { get; set; } = 100;
        public decimal? Weight { get; set; }        // percentage weight in final grade

        public int? TopicId { get; set; } // optional link to KB topic

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
