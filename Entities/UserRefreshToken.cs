using System.ComponentModel.DataAnnotations;

namespace NotasWebApi.Entities
{
	public class UserRefreshToken
	{
        public string Id { get; set; }
        [Required]
        public string RefreshToken { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public DateTime Expiration { get; set; }

    }
}
