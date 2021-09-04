using System.ComponentModel.DataAnnotations;

namespace DevTools.Models
{
    public class HueColor
    {
        [Key]
        public int HueId { get; set; }
        public string Color { get; set; }
        public string DefaultColor { get; set; }
        public string Name { get; set; }
    }
}