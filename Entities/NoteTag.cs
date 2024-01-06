using Azure;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotasWeb.Entities
{
    public class NoteTag
    {

     
        public Guid NotesId { get; set; }
        public Note Notes { get; set; }
 
        public Guid TagsId { get; set; }
 
        public Tag Tags { get; set; }
    }
}
