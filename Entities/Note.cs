using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotasWeb.Entities
{
    public class Note
    {
        public Guid Id { get; set; }
        public Guid TopicId { get; set; }
        [Required]
		public string UserCreationId { get; set; }

		[MaxLength(50)]
        public string Title { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreationDate { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ModifiedDate { get; set;}
        public string Content { get; set; }
        [Required]
        public bool IsCompleted { get; set; }
        [Required]
        public bool IsPrivate { get; set; }
        public bool IsPinned { get; set; } = false;
        public int Order { get; set; }
        public List<NoteTag> NotesTags { get; set; }
        public List<Colaborator> Colaborators { get; set; }


    }
}
