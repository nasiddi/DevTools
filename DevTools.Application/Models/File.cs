using System.ComponentModel.DataAnnotations;

namespace DevTools.Application.Models;

public class File
{
    public int Id { get; set; }
    [StringLength(32)]
    public string Guid { get; init; }
    [StringLength(150)]
    public string FileName { get; init; }
    public int Bytes { get; set; }
}