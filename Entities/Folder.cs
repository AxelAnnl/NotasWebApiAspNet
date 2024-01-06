using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotasWeb.Entities
{
    public class Folder
    {
       
        public Guid Id { get; set; }


        [Required]
        public string UserCreationId { get; set; }

		[Required]
        [RegularExpression(@"^[a-zA-Z0-9 ]*$", ErrorMessage = "El {0} de la carpeta solo puede contener letras, números y espacios.")]
        [StringLength(50, ErrorMessage = "El {0} de la carpeta no puede tener más de 50 caracteres.")]
        public string Name { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsPinned { get; set; } = false;
        public int Order { get; set; }
        public List<Topic> Topics { get; set; }
    }
}
