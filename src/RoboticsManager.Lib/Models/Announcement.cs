using System;
using System.ComponentModel.DataAnnotations;

namespace RoboticsManager.Lib.Models
{
    public enum AnnouncementPriority
    {
        Info,
        Warning,
        Danger,
        Primary,
        Secondary
    }

    public class Announcement : BaseEntity
    {
        [Required]
        public string Body { get; set; }

        [Required]
        public AnnouncementPriority Priority { get; set; }

        public bool IsVisible { get; set; } = true;

        // For markdown rendering
        public string RenderedBody { get; set; }
    }
}
