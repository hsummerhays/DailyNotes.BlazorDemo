using System;
using DailyNotes.Shared.Interfaces;

namespace DailyNotes.Shared.Models
{
    public class Topic : IHasTenantUser
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Visibility { get; set; } = "private";
        public int? ParentTopicId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        // 'learning' | 'beginner' | 'novice' | 'intermediate' | 'advanced' | 'expert'
        public string Proficiency { get; set; } = "learning";

        // 1-5 numeric for quiz difficulty matching
        public int SkillLevel { get; set; } = 1;

        public bool IsPinned { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
