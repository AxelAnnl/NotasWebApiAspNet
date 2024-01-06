using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotasWeb.Entities;
using NotasWeb.Services;
using NotasWebApi.DTOs;
using System.Runtime.CompilerServices;

namespace NotasWeb.Controllers
{
	[Route("api/folders")]
	public class FoldersController : ControllerBase
	{
		private readonly IUserServices userServices;
		private readonly ApplicationDBContext context;
		private readonly UserManager<IdentityUser> userManager;

		public FoldersController(IUserServices userServices, ApplicationDBContext context, UserManager<IdentityUser> userManager)
		{
			this.userServices = userServices;
			this.context = context;
			this.userManager = userManager;
		}

		[HttpPost]
		public async Task<ActionResult> Post([FromBody] postFolder model)
		{

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var userId = userServices.GetUserId(); //get id from actual http context

			var user = await userManager.FindByIdAsync(userId); // get user from db

			if (user == null)
			{
				return NotFound();
			}

			//create new Folder
			var folder = new Folder
			{
				Id = Guid.NewGuid(),
				Name = model.Name,
				UserCreationId = userId,
				CreationDate = DateTime.Now
			};

			context.Add(folder);

			await context.SaveChangesAsync();

			return Ok(folder);
		}

		[HttpGet]
		public async Task<ActionResult> Get()
		{
			var userId = userServices.GetUserId(); //get id from actual http context
			var user = await userManager.FindByIdAsync(userId); // get user from db

			if (user == null)
			{

				return Forbid();
			}

			var folders = await context.Folders
				.Where(f => f.UserCreationId == userId)
				.Select(folder => new
				{
					id = folder.Id,
					name = folder.Name,
					creationDate = folder.CreationDate,
					isPinned = folder.IsPinned

				})
				.ToListAsync();


			return Ok(folders);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var userId = userServices.GetUserId(); //get id from actual http context
			var user = await userManager.FindByIdAsync(userId); // get user from db

			if (user == null)
			{

				return Unauthorized();
			}

			try
			{
				var folder = await context.Folders.FindAsync(id);

				if (folder == null)
				{
					return NotFound(); // Indicar que no se encontró el folder
				}

				if (folder.UserCreationId != userId)
				{
					return Forbid();
				}

				context.Folders.Remove(folder);
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

