namespace NotasWebApi.Models
{
	public class ResponseRefreshAccessToken
	{
		public string AccessToken { get; set; }
		public DateTime ExpirationAccessToken { get; set; }
	}
}
