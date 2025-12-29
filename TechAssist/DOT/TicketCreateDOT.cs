using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TechAssist.DOT
{
    public class TicketCreateDOT
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int? ProductId { get; set; }

        [DefaultValue("Medium")]
        public string Priority { get; set; }
    }
}
