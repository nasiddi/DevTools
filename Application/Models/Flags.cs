using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public class Flags
    {
        public int Id { get; set; }
        [StringLength(30)]
        public string Name { get; set; }
        public bool Flag { get; set; }
    }
}