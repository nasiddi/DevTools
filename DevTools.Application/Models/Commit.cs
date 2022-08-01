using System;

namespace DevTools.Application.Models;

public record Commit(string Hash, string Message, DateTime Date);