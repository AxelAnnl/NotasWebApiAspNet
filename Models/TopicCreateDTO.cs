using NotasWeb.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NotasWeb.Models
{
    public class TopicCreateDTO
    {

		[Required]
		[ForeignKey("Folders")]
		public Guid FolderId { get; set; }

		[Required]
		[MaxLength(256)]
		[RegularExpression(@"^[a-zA-Z0-9 ]*$", ErrorMessage = "El {0} solo puede contener letras, números y espacios.")]
		public string Name { get; set; }

	}
}
