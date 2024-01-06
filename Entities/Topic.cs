using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotasWeb.Entities
{
    public class Topic
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        [ForeignKey("Folders")]
        public Guid FolderId { get; set; }
        [Required]
		public string UserCreationId { get; set; }

		[Required]
        [MaxLength(256)]
        [RegularExpression(@"^[a-zA-Z0-9 ]*$", ErrorMessage = "El {0} solo puede contener letras, números y espacios.")]
        public string Name { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreationDate { get; set; }
        public bool IsPinned { get; set; } = false;
        public int Order { get; set; }
        public List<Note> Notes { get; set; }



    }
}
