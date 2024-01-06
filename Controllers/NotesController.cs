using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotasWeb.Services;
using NotasWeb;
using Microsoft.EntityFrameworkCore;
using NotasWebApi.DTOs;
using NotasWeb.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace NotasWebApi.Controllers
{
	[Route("api/notes")]

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class NotesController : ControllerBase
	{
		private readonly IUserServices userServices;
		private readonly ApplicationDBContext context;
		private readonly UserManager<IdentityUser> userManager;

		public NotesController(IUserServices userServices, ApplicationDBContext context, UserManager<IdentityUser> userManager)
		{
			this.userServices = userServices;
			this.context = context;
			this.userManager = userManager;

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

			var notes = await context.Notes.Where(note => note.UserCreationId == userId)
				.Select(note => new
				{
					id = note.Id,
					topicId = note.TopicId,
					title = note.Title,
					creationDate = note.CreationDate,
					modifiedDate = note.ModifiedDate,
					content = note.Content,
					isCompleated = note.IsCompleted,
					isPrivate = note.IsPrivate,
					isPinned = note.IsPinned,
					noteTags = note.NotesTags,
					colaborators = note.Colaborators
				}).ToListAsync();

			return Ok(notes);

		}

		[HttpPost]
		public async Task<ActionResult> Post([FromBody] CreateNote note)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest("Invalid model state.");
			}
			var userId = userServices.GetUserId(); //get id from actual http context
			var user = await userManager.FindByIdAsync(userId); // get user from db

			if (user == null)
			{

				return Forbid();
			}


			Note nota = new Note
			{
				Id = new Guid(),
				TopicId = note.TopicId,
				UserCreationId = userId,
				Title = note.Title,
				CreationDate = DateTime.Now,
				ModifiedDate = DateTime.Now,
				Content = "",
				IsCompleted = true,
				IsPrivate = true,
				IsPinned = false,
				Order = 0

			};

			context.Notes.Add(nota);
			await context.SaveChangesAsync();

			return Ok(nota);


		}

		[HttpDelete("{noteId}")]
		public async Task<ActionResult> Delete(Guid noteId)
		{
			var userId = userServices.GetUserId(); //get id from actual http context
			var user = await userManager.FindByIdAsync(userId); // get user from db

			if (user == null)
			{

				return Unauthorized();
			}

			var note = await context.Notes
				.Where(note => note.UserCreationId == userId && note.Id == noteId)
				.FirstOrDefaultAsync();

			if (note is null)
			{
				return NotFound();
			}

			context.Notes.Remove(note);
			await context.SaveChangesAsync();

			return NoContent();




		}

		[HttpPost("updateNote")]
		public async Task<ActionResult> UpdateNote([FromBody] UpdateNote updatedNote)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest("Invalid model state.");
			}
			var userId = userServices.GetUserId(); //get id from actual http context
			var user = await userManager.FindByIdAsync(userId); // get user from db

			if (user == null)
			{

				return NotFound();
			}

			
			var note = await context.Notes.FirstOrDefaultAsync(n => n.Id.ToString().ToUpper() == updatedNote.Id.ToUpper());
			Console.WriteLine(updatedNote.Id);
			if (note is null)
			{
				return NotFound("nota no encontrada");
			}

			// Verificar si el usuario es el propietario de la nota (opcional)
			if (note.UserCreationId != userId)
			{
				return Forbid("No tienes permiso para editar esta nota");
			}

			note.Content = updatedNote.Content;

			// Guardar los cambios en la base de datos
			await context.SaveChangesAsync();

			return Ok("Nota actualizada exitosamente");

		}

	}
}
