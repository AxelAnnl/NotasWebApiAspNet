using System.ComponentModel.DataAnnotations;

namespace NotasWeb.Entities
{
    public class Tag
    {

        public Guid Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public List<NoteTag> NotesTags { get; set; }
    }
}
