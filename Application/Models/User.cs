using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public class User
    {
        public int Id { get; set; }
        [StringLength(30)]
        public string Username { get; set; }
        [StringLength(50)]
        public string PasswordHash { get; set; }
        public bool IsAdmin { get; set; }
    }
}