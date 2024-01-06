using System.ComponentModel.DataAnnotations;

namespace NotasWeb.Models
{
	public class FolderCreateDTO
	{
		[Required]
		[RegularExpression(@"^[a-zA-Z0-9 ]*$", ErrorMessage = "El {0} de la carpeta solo puede contener letras, números y espacios.")]
		[StringLength(50, ErrorMessage = "El {0} de la carpeta no puede tener más de 50 caracteres.")]
		public string Name { get; set; }
	}
}
