using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public class HueColor
    {
        [Key]
        public int HueId { get; set; }
        [StringLength(20)]
        public string Color { get; set; }
        [StringLength(20)]
        public string DefaultColor { get; set; }
        [StringLength(20)]
        public string Name { get; set; }
    }
}