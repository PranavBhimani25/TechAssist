namespace TechAssist.Models
{
    public class TicketReply
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        public int AuthorId { get; set; }
        public User Author { get; set; }

        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
