using NotasWebApi.DTOs;
using NotasWebApi.Models;
using System.Security.Claims;

namespace NotasWebApi.Services
{
	public interface ITokenService
	{
		Task<ResponseGenerateAccesToken> GenerateAccessTokenAsync(string email);
		ResponseGenerateToken GenerateRefreshToken();
		ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
	}
}
