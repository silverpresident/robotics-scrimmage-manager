using System.ComponentModel.DataAnnotations;

namespace RoboticsManager.Lib.Models
{
    public class Announcement : BaseEntity
    {
        [Required]
        public required string Body { get; set; }

        public string? RenderedBody { get; set; }

        [Required]
        public AnnouncementPriority Priority { get; set; }

        public bool IsVisible { get; set; }

        public Announcement()
        {
            Priority = AnnouncementPriority.Info;
            IsVisible = true;
        }
    }
}
