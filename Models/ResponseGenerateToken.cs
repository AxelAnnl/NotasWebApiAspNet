namespace NotasWebApi.Models
{
	public class ResponseGenerateToken
	{
		public string Token { get; set; }
		public DateTime Expiration { get; set; }
	}
}
