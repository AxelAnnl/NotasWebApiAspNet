using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NotasWebApi.DTOs;
using NotasWebApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NotasWebApi.Services
{
	public class TokenService : ITokenService
	{
		private readonly UserManager<IdentityUser> userManager;
		private readonly IConfiguration configuration;
		private readonly HttpContext httpContext;
		private readonly TimeSpan AccessTokenDuration = TimeSpan.FromSeconds(30);
		private readonly TimeSpan RefreshTokenDuration = TimeSpan.FromHours(1);

		public TokenService(UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccesor, IConfiguration configuration) {
			this.userManager = userManager;
			this.configuration = configuration;
			this.httpContext = httpContextAccesor.HttpContext;
		}
		public async Task<ResponseGenerateAccesToken> GenerateAccessTokenAsync(string email)
		{
			//creacion de una lista de claims con un claim llmado email
			var claims = new List<Claim>(){
							new Claim(ClaimTypes.Email,email.ToLowerInvariant())
			};

			var usuario = await userManager.FindByEmailAsync(email);
			var claimsDB = await userManager.GetClaimsAsync(usuario);
			claims.AddRange(claimsDB);

			var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["NombreDeLlave"]));

			var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);
			var expiracion = DateTime.UtcNow.Add(AccessTokenDuration);

			var issuer = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
			var audience = httpContext.Request.Headers["Referer"].ToString(); // Puedes cambiar esto según tus necesidades

			Console.WriteLine(issuer);
			var token = new JwtSecurityToken(
				issuer: issuer,
				audience: audience,
				claims: claims,
				expires: expiracion,
				signingCredentials: creds);


			return new ResponseGenerateAccesToken()
			{
				Token = new JwtSecurityTokenHandler().WriteToken(token),
				Expiration = expiracion
			};

		}
		public ResponseGenerateToken GenerateRefreshToken()
		{

			var expiracion = DateTime.UtcNow.Add(RefreshTokenDuration);

			var randomNumber = new byte[32];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(randomNumber);

				ResponseGenerateToken responseGenerateToken = new ResponseGenerateToken
				{
					Token = Convert.ToBase64String(randomNumber),
					Expiration = expiracion
				};

				return responseGenerateToken;
			}
		}
		public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
		{
			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
				ValidateIssuer = false,
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["NombreDeLlave"])),
				ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
			};
			var tokenHandler = new JwtSecurityTokenHandler();
			SecurityToken securityToken;
			var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
			var jwtSecurityToken = securityToken as JwtSecurityToken;
			if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
				throw new SecurityTokenException("Invalid token");


			Console.WriteLine(principal);
			return principal;
		}
	}
}
