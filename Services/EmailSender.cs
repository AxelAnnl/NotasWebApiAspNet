
using Microsoft.AspNetCore.Identity.UI.Services;
using Newtonsoft.Json;
using System.Text;

namespace NotasWebApi.Services
{
	public class EmailSender: IEmailSender
	{
		private readonly string _mailjetApiKey;
		private readonly string _mailjetApiSecret;
		private readonly string _senderEmail;
		private readonly IConfiguration configuration;

		public EmailSender(IConfiguration configuration)
		{
			_mailjetApiKey = configuration["MailjetApiKey"];
			_mailjetApiSecret = configuration["MailjetSecretKey"];
			_senderEmail = "sender";
			this.configuration = configuration;
		}

		public async Task SendEmailAsync(string email, string subject, string message)
		{
			// Otras configuraciones...

			var payload = new
			{
				Messages = new[]
				{
			new
			{
				From = new
				{
					Email = "axelfuentes2802@gmail.com",
					Name = "Axel"
				},
				To = new[]
				{
					new
					{
						Email = email,
						Name = "Pepito"
					}
				},
				Subject = subject,
				TextPart = message,
                // HTMLPart si deseas incluir contenido HTML
            }
		}
			};

			var payloadJson = JsonConvert.SerializeObject(payload);

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
					"Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_mailjetApiKey}:{_mailjetApiSecret}"))
				);

				var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

				var response = await client.PostAsync("https://api.mailjet.com/v3.1/send", content);

				if (response.IsSuccessStatusCode)
				{
					Console.WriteLine($"Correo electrónico enviado exitosamente a {email} con asunto '{subject}' y mensaje: {message}");
				}
				else
				{
					Console.WriteLine($"Error al enviar el correo electrónico a {email}. Código de estado: {response.StatusCode}");

					// Imprimir el contenido de la respuesta para obtener más detalles sobre el error
					var responseContent = await response.Content.ReadAsStringAsync();
					Console.WriteLine($"Contenido de la respuesta: {responseContent}");
				}
			}
		}

	}
}
