namespace TechAssist.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public int? ActorId { get; set; }
        public User Actor { get; set; }
        public string ActionType { get; set; } // e.g., "CreateTicket", "ReplyTicket", "AssignEngineer"
        public string Entity { get; set; }
        public int? EntityId { get; set; }
        public string Details { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
