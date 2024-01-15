using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NotasWeb.Models;
using NotasWebApi.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using NotasWebApi.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using NotasWebApi.Models;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Identity.Data;
using NotasWebApi.Entities;
using Microsoft.EntityFrameworkCore;
using NotasWeb.Services;


namespace NotasWeb.Controllers
{
	[Route("api/users")]
	public class UsersController : Controller
	{
		private readonly UserManager<IdentityUser> userManager;
		private readonly SignInManager<IdentityUser> signInManager;
		private readonly IConfiguration configuration;
		private readonly IAuthenticationSchemeProvider authenticationSchemeProvider;
		private readonly IEmailSender emailSender;
		private readonly ITokenService tokenService;
		private readonly ApplicationDBContext dBContext;
		private readonly IUserServices userServices;
		private readonly HttpContext httpContext;

		public UsersController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
			IConfiguration configuration, IHttpContextAccessor httpContextAccesor,
			IAuthenticationSchemeProvider authenticationSchemeProvider, IEmailSender emailSender, ITokenService tokenService,
			ApplicationDBContext dBContext, IUserServices userServices)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
			this.configuration = configuration;
			this.authenticationSchemeProvider = authenticationSchemeProvider;
			this.emailSender = emailSender;
			this.tokenService = tokenService;
			this.dBContext = dBContext;
			this.userServices = userServices;
			this.httpContext = httpContextAccesor.HttpContext;
		}

		[HttpPost("signin")]
		[AllowAnonymous]
		public async Task<ActionResult> Post([FromBody] CredencialesUsuario model) //create an account
		{
			if (!ModelState.IsValid)
			{
				return Forbid();
			}
			//map user model to identityUser
			var usuario = new IdentityUser()
			{
				Email = model.Email,
				UserName = model.Email,
			};

			var resultado = await userManager.CreateAsync(usuario, password: model.Password);

			if (resultado.Succeeded)
			{
				// Obtener el usuario recién creado con FindByEmailAsync para obtener su Id
				var usuarioRecienCreado = await userManager.FindByEmailAsync(model.Email);


				// Agregar claims al usuario
				await userManager.AddClaimAsync(usuarioRecienCreado, new Claim(ClaimTypes.NameIdentifier, usuarioRecienCreado.Id));
				await userManager.AddClaimAsync(usuario, new Claim(ClaimTypes.Email, usuario.Email));

				// Genera un nuevo token de confirmación de correo electrónico
				var token = await userManager.GenerateEmailConfirmationTokenAsync(usuario);
				
				// Envía el nuevo token de confirmación de correo electrónico por correo electrónico
				var emailSubject = "Reenvío de Confirmación de Correo Electrónico";
				var emailMessage = $"Por favor, confirma tu correo electrónico haciendo clic en este enlace: {token}";
				await emailSender.SendEmailAsync(usuario.Email, emailSubject, emailMessage);


				//manejar accesToken y refresh token
				var accesTokenAndExpireDate = await tokenService.GenerateAccessTokenAsync(model.Email);
				var refreshTokenAndExpireDate = tokenService.GenerateRefreshToken();

				//Guardar el refresh token en la bd
				UserRefreshToken refreshToken = new UserRefreshToken
				{
					Id = Guid.NewGuid().ToString(),
					RefreshToken = refreshTokenAndExpireDate.Token,
					UserId = usuario.Id,
					Expiration = refreshTokenAndExpireDate.Expiration,
				};

				dBContext.UsersRefreshTokens.Add(refreshToken);


				ResponseLogin response = new ResponseLogin
				{
					AccessToken = accesTokenAndExpireDate.Token,
					ExpirationAccessToken = accesTokenAndExpireDate.Expiration,
					ExpirationRefreshToken = refreshToken.Expiration,
					RefreshToken = refreshTokenAndExpireDate.Token

				};

				await dBContext.SaveChangesAsync();

				return Ok(response);
			}
			else
			{
				foreach (var error in resultado.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}

				return Forbid();
			}
		}
		

		[HttpPost("login")]
		[AllowAnonymous]
		public async Task<ActionResult> Login([FromBody] CredencialesUsuario credenciales)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			

			var resultado = await signInManager.PasswordSignInAsync(credenciales.Email,
				credenciales.Password, isPersistent: false, lockoutOnFailure: false);


		
			if (resultado.Succeeded)
			{
				var userId = userServices.GetUserId();
				var refreshTokenBd = await dBContext.UsersRefreshTokens.FirstOrDefaultAsync(urt => urt.UserId == userId);
				var accesTokenAndExpireDate = await tokenService.GenerateAccessTokenAsync(credenciales.Email);

				if (refreshTokenBd is null) //si el token no existe en la bd
				{
					
					var refreshTokenAndExpireDate = tokenService.GenerateRefreshToken();
					//Guardar el refresh token en la bd

					var usuarioAutenticado = await userManager.FindByEmailAsync(credenciales.Email);

					UserRefreshToken refreshToken = new UserRefreshToken
					{
						Id = Guid.NewGuid().ToString(),
						RefreshToken = refreshTokenAndExpireDate.Token,
						UserId = usuarioAutenticado.Id,
						Expiration = refreshTokenAndExpireDate.Expiration,
					};
					dBContext.UsersRefreshTokens.Add(refreshToken);

					ResponseLogin responseLogin = new ResponseLogin
					{
						AccessToken = accesTokenAndExpireDate.Token,
						RefreshToken = refreshTokenAndExpireDate.Token,
						ExpirationAccessToken = accesTokenAndExpireDate.Expiration,
						ExpirationRefreshToken = refreshTokenAndExpireDate.Expiration

					};
					await dBContext.SaveChangesAsync();
					return Ok(responseLogin);
				}
				else  //si existe en la bd 
				{
					var refreshTokenExpire = await dBContext.UsersRefreshTokens
						.Where(urt => urt.UserId == userId)  // Filtrar por UserId
						.Select(urt => urt.Expiration)
						.FirstOrDefaultAsync();  // Asumiendo que quieres obtener la primera fecha de expiración que coincida

					ResponseLogin responseLogin = new ResponseLogin
					{
						AccessToken = accesTokenAndExpireDate.Token,
						RefreshToken = refreshTokenBd.RefreshToken.ToString(),
						ExpirationAccessToken = accesTokenAndExpireDate.Expiration,
						ExpirationRefreshToken = refreshTokenExpire

					};
					return Ok(responseLogin);
				}


				
				
			}
			else
			{
				return BadRequest("Credenciales incorrectas");
			}

		}



		[HttpPost("logout")]
		public async Task<IActionResult> Logout()
		{
			try
			{
				// Obtiene el identificador del usuario actualmente autenticado
				var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

				if (string.IsNullOrEmpty(userId))
				{
					return BadRequest("Usuario no autenticado.");
				}

				// Busca y elimina el token de refresco de la base de datos
				var refreshToken = await dBContext.UsersRefreshTokens.FirstOrDefaultAsync(urt => urt.UserId == userId);
				if (refreshToken != null)
				{
					dBContext.UsersRefreshTokens.Remove(refreshToken);
					await dBContext.SaveChangesAsync();
				}

				// Aquí puedes añadir cualquier otra lógica de logout que necesites

				return Ok("Logout exitoso.");
			}
			catch (Exception ex)
			{
				// Log the exception (optional but recommended)
				// _logger.LogError(ex, "Error during logout.");

				// Devuelve una respuesta de error genérica al cliente
				return StatusCode(500, "Error interno del servidor.");
			}
		}




		[HttpPost("confirmEmail")]
		public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest();
			}

			var user = await userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				return BadRequest("El usuario no existe.");
			}

			var result = await userManager.ConfirmEmailAsync(user, model.Token);
			if (result.Succeeded)
			{
				return Ok("Correo electrónico confirmado exitosamente.");
			}
			else
			{
				return BadRequest("Error al confirmar el correo electrónico.");
			}
		}

		[HttpPost("resendEmailConfirmation")]
		public async Task<IActionResult> ResendEmailConfirmation([FromBody] SendEmailConfirmationViewModel model)
		{
			var user = await userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				return BadRequest("El usuario no existe.");
			}

			// Verifica si el correo electrónico ya ha sido confirmado
			if (await userManager.IsEmailConfirmedAsync(user))
			{
				return BadRequest("El correo electrónico ya ha sido confirmado.");
			}

			// Genera un nuevo token de confirmación de correo electrónico
			var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

			// Envía el nuevo token de confirmación de correo electrónico por correo electrónico
			var emailSubject = "Reenvío de Confirmación de Correo Electrónico";
			var emailMessage = $"Por favor, confirma tu correo electrónico haciendo clic en este enlace: {token}";
			await emailSender.SendEmailAsync(user.Email, emailSubject, emailMessage);

	
			return Ok("Correo electrónico de confirmación reenviado exitosamente.");

		}

		[HttpPost("forgotPassword")]
		[AllowAnonymous]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest model)
		{
			var user = await userManager.FindByEmailAsync(model.Email);
			if (user == null || !(await userManager.IsEmailConfirmedAsync(user)))
			{
				// Devuelve un error si el usuario no existe o el correo electrónico no está confirmado
				return BadRequest("El correo electrónico no existe o no está confirmado.");
			}

			// Genera un token de restablecimiento de contraseña y lo envía al usuario (por correo electrónico, SMS, etc.)
			var token = await userManager.GeneratePasswordResetTokenAsync(user);


			// Envía el token de restablecimiento de contraseña por correo electrónico
			var emailSubject = "Restablecimiento de Contraseña";
			var emailMessage = $"Tu token de restablecimiento de contraseña es: {token}";
			await emailSender.SendEmailAsync(user.Email, emailSubject, emailMessage);

			return Ok("Token de restablecimiento de contraseña generado exitosamente.");
		}

		[HttpPost("resetPassword")]
		[AllowAnonymous]
		public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
		{
			var user = await userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				return BadRequest();
			}

			var result = await userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
			if (result.Succeeded)
			{
				return Ok("Contraseña restablecida exitosamente.");
			}
			else
			{
				return BadRequest("Error al restablecer la contraseña.");
			}
		}

		[HttpPost]
		[Route("refresh")]
		[AllowAnonymous]
		public async Task<ActionResult> Refresh([FromBody] TokenApiModel tokenApiModel)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest("Invalid model state.");
			}
			var principal = tokenService.GetPrincipalFromExpiredToken(tokenApiModel.AccessToken);

			if (principal == null)
			{
				return NotFound();
			}

			var usernameClaim = principal.FindFirst(ClaimTypes.Email);
			if (usernameClaim == null)
			{
				return NotFound();
			}

			string username = usernameClaim.Value;

			var user = await dBContext.Users.FirstOrDefaultAsync(u => u.UserName == username);

			if (user == null)
			{
				return NotFound("usuario no encontrado");
			}

			var refreshTokenBd = await dBContext.UsersRefreshTokens.FirstOrDefaultAsync(urt => urt.UserId == user.Id);
			if (refreshTokenBd == null)
			{
				return BadRequest("Invalid refresh token.");
			}
			if (refreshTokenBd.RefreshToken.ToString() != tokenApiModel.RefreshToken)
			{
				return BadRequest("el token no se reconoce");
			}
			if (refreshTokenBd.Expiration >= DateTime.UtcNow)
			{
				return BadRequest("El token aun no expira");
			}

			var newAccessTokenAndExpiration = await tokenService.GenerateAccessTokenAsync(user.Email);

			await dBContext.SaveChangesAsync();

			//// Agregar nuevo refresh token a la base de datos
			//dBContext.UsersRefreshTokens.Add(new UserRefreshToken
			//{
			//	Id = Guid.NewGuid().ToString(),
			//	UserId = userId,
			//	RefreshToken = newRefreshTokenAndExpiration.Token,
			//	Expiration = newRefreshTokenAndExpiration.Expiration
			//});
			//await dBContext.SaveChangesAsync();

			var response = new ResponseRefreshAccessToken
			{
				AccessToken = newAccessTokenAndExpiration.Token,
				ExpirationAccessToken = newAccessTokenAndExpiration.Expiration,
			};

			return Ok(response);

		}
		[HttpGet("probar")]
		[AllowAnonymous]
		public IActionResult ProveApi()
		{
			return Ok("La api funciona");
		}

		//[AllowAnonymous]
		//[HttpGet]
		//public ChallengeResult LoginExterno(string provedor, string returnUrl = null)
		//{
		//	var urlRedireccion = Url.Action("RegistrarUsuarioExterno", values: new { returnUrl });
		//	var propiedades = signInManager
		//		.ConfigureExternalAuthenticationProperties(provedor, urlRedireccion);
		//	return new ChallengeResult(provedor, propiedades);
		//}


		//[AllowAnonymous]
		//[HttpGet]
		//public async Task<ActionResult> RegistrarUsuarioExterno(string remoteError = null)
		//{
		//	// Mensaje de error
		//	var mensaje = "";

		//	// Manejo de errores remotos (si los hay)
		//	if (remoteError != null)
		//	{
		//		mensaje = $"Error del proveedor externo: {remoteError}";
		//		return StatusCode(500, mensaje);
		//	}

		//	// Obtención de la información de inicio de sesión externo
		//	var info = await signInManager.GetExternalLoginInfoAsync();
		//	if (info is null)
		//	{
		//		mensaje = "Error cargando la información de inicio de sesión externo";
		//		return StatusCode(500, mensaje);
		//	}

		//	// Intento de inicio de sesión externo
		//	var resultadoLoginExterno = await signInManager.ExternalLoginSignInAsync(info.LoginProvider,
		//		info.ProviderKey, isPersistent: true, bypassTwoFactor: true);

		//	// Obtención del email del usuario externo si está disponible
		//	string email = "";
		//	if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
		//	{
		//		email = info.Principal.FindFirstValue(ClaimTypes.Email);
		//	}
		//	else
		//	{
		//		mensaje = "Error leyendo el email del usuario del proveedor";
		//		return StatusCode(500, mensaje);
		//	}

		//	var usuario = new IdentityUser { Email = email, UserName = email };

		//	if (!resultadoLoginExterno.Succeeded)
		//	{
		//		// Creación de un nuevo usuario con el email obtenido

		//		var resultadoCrearUsuario = await userManager.CreateAsync(usuario);

		//		// Si no se pudo crear el usuario, se muestra un mensaje de error
		//		if (!resultadoCrearUsuario.Succeeded)
		//		{
		//			mensaje = resultadoCrearUsuario.Errors.First().Description;
		//			return StatusCode(500, mensaje);
		//		}
		//	}


		//	// Intento de agregar el inicio de sesión externo al usuario
		//	var resultadoAgregarLogin = await userManager.AddLoginAsync(usuario, info);

		//	// Si se agrega con éxito, se inicia sesión y se redirige a la URL de retorno
		//	if (resultadoAgregarLogin.Succeeded)
		//	{
		//		await signInManager.SignInAsync(usuario, isPersistent: true, info.LoginProvider);

		//		var token = await ConstruirToken(email);
		//		return Ok(token);

		//	}

		//	// Si ocurre un error al agregar el inicio de sesión externo, se muestra un mensaje de error
		//	mensaje = "Ha ocurrido un error agregando el inicio de sesión";
		//	return RedirectToAction("Login", routeValues: new { mensaje });
		//}
	}
}
