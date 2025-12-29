namespace TechAssist.DOT
{
    public class TicketResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // Creator Info (User who created the ticket)
        public UserDTO Creator { get; set; }

        // Assigned Engineer Info (if any)
        public UserDTO AssignedEngineer { get; set; }

        // Product Info (if any)
        public ProductDTO Product { get; set; }

        // Nested DTOs
        public class UserDTO
        {
            public int Id { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
        }

        public class ProductDTO
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}

