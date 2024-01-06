using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NotasWeb.Services;
using NotasWeb;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.DependencyInjection;
using System;
using NotasWebApi.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

var politicaUsuariosAutenticados = new AuthorizationPolicyBuilder()
	.RequireAuthenticatedUser()
	.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme) // Especificar el esquema aquí
	.Build();



// Add services to the container.----------------------------------
builder.Services.AddControllers(opciones =>
{
	opciones.Filters.Add(new AuthorizeFilter(politicaUsuariosAutenticados));
});


builder.Services.AddDbContext<ApplicationDBContext>(opciones => 
	opciones.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); //DB context configurtion

// configuration of identity for entityFM
builder.Services.AddIdentity<IdentityUser, IdentityRole>(opciones =>
{
	opciones.SignIn.RequireConfirmedAccount = false;


}).AddEntityFrameworkStores<ApplicationDBContext>()
.AddDefaultTokenProviders();

//builder.Services
//	.AddIdentityApiEndpoints<IdentityUser>()
//	.AddEntityFrameworkStores<ApplicationDBContext>().AddDefaultTokenProviders();


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(opciones =>
	{
		opciones.Events = new JwtBearerEvents
		{
			OnTokenValidated = context =>
			{
				// Acceder al token validado y extraer el issuer
				var securityToken = context.SecurityToken as JwtSecurityToken;
				var issuer = securityToken?.Issuer;

				// Imprimir el issuer
				Console.WriteLine($"Token validado con éxito. Issuer: {issuer}");

				Console.WriteLine("Token validado con éxito");
				return Task.CompletedTask;
			},
			OnAuthenticationFailed = context =>
			{
				// Acceder a la información del token en caso de fallo de autenticación
				var securityToken = context.Exception?.Data["SecurityToken"] as SecurityToken;
				var issuer = (securityToken as JwtSecurityToken)?.Issuer;

				// Imprimir el issuer
				Console.WriteLine($"Falla crítica. Issuer: {issuer}");

				// Lógica personalizada en caso de falla de autenticación
				Console.WriteLine("falla critica");
				return Task.CompletedTask;
			},
			OnForbidden = context =>
			{
				Console.WriteLine("onForbiden");
				return Task.CompletedTask;
			},
			OnMessageReceived = context =>
			{
				Console.WriteLine("mensaje");
				return Task.CompletedTask;
			}
		};

		opciones.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = false,
			ValidateAudience = false,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["NombreDeLlave"])),
			ClockSkew = TimeSpan.Zero
		};
	});

//builder.Services.AddAuthentication().AddMicrosoftAccount(opciones =>
//{
//	opciones.ClientId = builder.Configuration["MicrosoftClientId"];
//	opciones.ClientSecret = builder.Configuration["MicrosoftSecretId"];

//});

builder.Services.AddAuthorization();

builder.Services.AddTransient<IUserServices, UserServices>();
builder.Services.AddSingleton<IEmailSender,EmailSender>();
builder.Services.AddTransient<ITokenService, TokenService>();

//---------------------------------------------------------------
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
	
builder.Services.AddCors(options =>
{
	var frontendUrl = builder.Configuration.GetValue<string>("frontend-url");

	options.AddDefaultPolicy(builder =>
	{
		builder.WithOrigins(frontendUrl).AllowAnyMethod().AllowAnyHeader();
	});
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

var app = builder.Build();
// Configurar Kestrel para utilizar $PORT

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}



app.UseHttpsRedirection();

app.UseRouting(); 

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();



//var authGroup = app.MapGroup("api/users");
//authGroup.MapIdentityApi<IdentityUser>();


app.MapControllers();

app.Run($"http://0.0.0.0:{port}");