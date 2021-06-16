using System;

namespace SpaDeployment.Models
{
    public record Commit(string Hash, string Message, DateTime Date);
}