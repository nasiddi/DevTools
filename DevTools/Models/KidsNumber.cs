using System.ComponentModel.DataAnnotations;

namespace DevTools.Models
{
    public class KidsNumber
    {
        [Key]
        public int Id { get; set; }
        public int Number { get; set; }
    }
}