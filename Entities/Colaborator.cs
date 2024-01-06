using System.ComponentModel.DataAnnotations.Schema;

namespace NotasWeb.Entities
{
    public class Colaborator
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        [ForeignKey("NotesId")]
        public Guid NoteId { get; set; }
    }
}
