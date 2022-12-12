using System;
using System.Text.Json.Serialization;
using DevTools.Application.Enums.Citadels;

namespace DevTools.Application.Models.Citadels;

public class Character
{
    public int Id { get; set; }
    public int CharacterNumber { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CharacterType CharacterType { get; set; }
    public DateTime ActivationDate { get; set; } = DateTime.UtcNow;
    public DateTime? DeactivationDate { get; set; }
}