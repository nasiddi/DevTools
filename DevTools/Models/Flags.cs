using System.ComponentModel.DataAnnotations;

namespace DevTools.Models
{
    public class Flags
    {
        [Key]
        public string Name { get; set; }
        public bool Flag { get; set; }
    }
}