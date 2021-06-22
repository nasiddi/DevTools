using System;

namespace DevTools.Models
{
    public record Commit(string Hash, string Message, DateTime Date);
}