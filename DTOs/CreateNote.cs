using Microsoft.AspNetCore.Identity;
using NotasWeb.Entities;
using System.ComponentModel.DataAnnotations;

namespace NotasWebApi.DTOs
{
	public class CreateNote
	{
		[Required]
		public Guid TopicId { get; set; }

		[MaxLength(50)]
		[Required]
		public string Title { get; set; }

	}
}
