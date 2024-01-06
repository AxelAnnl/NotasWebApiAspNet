using System.ComponentModel.DataAnnotations;

namespace NotasWebApi.DTOs
{
	public class UpdateNote
	{
        [Required]
        public string Content { get; set; }
        [Required]
        public string Id { get; set; }
    }
}
