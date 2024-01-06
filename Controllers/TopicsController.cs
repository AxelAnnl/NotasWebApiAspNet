using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotasWeb.Entities;
using NotasWeb.Models;
using NotasWeb.Services;
using System.Runtime.CompilerServices;

namespace NotasWeb.Controllers
{
    [Route("api/topics")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class TopicsController : ControllerBase
    {
		private readonly IUserServices userServices;
		private readonly ApplicationDBContext context;
		private readonly UserManager<IdentityUser> userManager;

		public TopicsController(IUserServices userServices, ApplicationDBContext context, UserManager<IdentityUser> userManager)
		{
			this.userServices = userServices;
			this.context = context;
			this.userManager = userManager;
		}

		[HttpPost]
		public async Task<ActionResult> Post([FromBody] TopicCreateDTO model)
		{
			var userId = userServices.GetUserId(); //get id from actual http context

			var user = await userManager.FindByIdAsync(userId); // get user from db

			Topic topic = new Topic
			{
				Id = Guid.NewGuid(),
				FolderId = model.FolderId,
				UserCreationId = userId,
				Name = model.Name,
				CreationDate = DateTime.Now
			};

			context.Topics.Add(topic);

			await context.SaveChangesAsync();

			return Ok(topic);
		}

		[HttpGet]
		public async Task<ActionResult> Get()
		{
			var userId = userServices.GetUserId();
			var user = await userManager.FindByIdAsync(userId); // get user from db

			if (user == null)
			{
				return Forbid();
			}

			var topics = await context.Topics.
				Where(t => t.UserCreationId == userId).
				Select(t => new
				{
					id = t.Id,
					folderId = t.FolderId,
					name = t.Name,
					creationDate = t.CreationDate,
					isPinned = t.IsPinned
				})
				.ToListAsync();

			return Ok(topics);
		}

		[HttpDelete("{id}")]
		public async Task<ActionResult> Delete(Guid id)
		{
			var userId = userServices.GetUserId();
			var user = await userManager.FindByIdAsync(userId); // get user from db

			if (user == null)
			{
				return Forbid();
			}


			try
			{
				var topic = await context.Topics.FindAsync(id);

				if (topic == null)
				{
					return NotFound(); // Indicar que no se encontró el folder
				}

				context.Topics.Remove(topic);
				await context.SaveChangesAsync();

				return NoContent(); // Indicar éxito sin contenido
			}
			catch (Exception ex)
			{
				// Manejar errores y devolver un resultado apropiado
				return StatusCode(500, $"Error interno del servidor: {ex.Message}");
			}
		}
	}
}
