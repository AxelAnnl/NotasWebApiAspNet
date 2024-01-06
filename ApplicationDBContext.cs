using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NotasWeb.Entities;
using NotasWebApi.Entities;
using System.Threading;

namespace NotasWeb
{
	public class ApplicationDBContext : IdentityDbContext
	{
		public ApplicationDBContext(DbContextOptions options) : base(options)
		{
		}

		public DbSet<Folder> Folders { get; set; }
		public DbSet<Note> Notes { get; set; }
		public DbSet<NoteTag> NotesTags { get; set; }
		public DbSet<Tag> Tags { get; set; }
		public DbSet<Topic> Topics { get; set; }
		public DbSet<Colaborator> Colaborators { get; set; }
		public DbSet<UserRefreshToken> UsersRefreshTokens { get; set; }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);


			modelBuilder.Entity<NoteTag>()
				.HasKey(c => new { c.NotesId, c.TagsId });


			// Configuración de la relación entre Note y User
			modelBuilder.Entity<Note>()
				.HasOne<IdentityUser>()
				.WithMany()
				.HasForeignKey(n => n.UserCreationId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Topic>()
				.HasOne<IdentityUser>()
				.WithMany()
				.OnDelete(DeleteBehavior.Restrict); // Cambiado de Restrict a Cascade


			modelBuilder.Entity<Folder>()
				.HasOne<IdentityUser>()
				.WithMany()
				.HasForeignKey(n => n.UserCreationId);

			modelBuilder.Entity<UserRefreshToken>()
				.HasOne<IdentityUser>()
				.WithOne()
				.HasForeignKey<UserRefreshToken>(urt => urt.UserId); 

		}
	}
}
