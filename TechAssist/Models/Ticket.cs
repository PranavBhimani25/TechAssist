using System.ComponentModel.DataAnnotations;

namespace TechAssist.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required, MaxLength(250)]
        public string Title { get; set; }

        public string Description { get; set; }

        public int CreatorId { get; set; }
        public User Creator { get; set; }

        public int? ProductId { get; set; }
        public Product Product { get; set; }

        public int? AssignedEngineerId { get; set; }
        public User AssignedEngineer { get; set; }

        public string Priority { get; set; } = "Medium"; // Could be enum
        public string Status { get; set; } = "Open"; // Open/InProgress/Resolved/Closed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<TicketReply> Replies { get; set; }

    }
}
