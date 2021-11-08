using System;

namespace Application.Models
{
    public record Commit(string Hash, string Message, DateTime Date);
}