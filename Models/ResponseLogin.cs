namespace NotasWebApi.Models
{
	public class ResponseLogin
	{
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public DateTime ExpirationAccessToken { get; set; }
        public DateTime ExpirationRefreshToken { get; set; }

    }
}
