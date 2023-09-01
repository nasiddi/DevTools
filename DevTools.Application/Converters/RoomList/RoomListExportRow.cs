using System;

namespace DevTools.Application.Converters.RoomList;

public class RoomListExportRow
{
    [RoomList("room No.", 1)]
    public int Index { get; set; }
    
    [RoomList("family name", 2)]
    public string? LastName { get; set; }
    
    [RoomList("first name", 3)]
    public string? FirstName { get; set; }
    
    [RoomList("MR/MRS", 4)]
    public string? Title { get; set; }

    [RoomList("birthday", 5, true)]
    public DateTime? DateOfBirth { get; set; }
    
    [RoomList("Passport # ", 6)]
    public string? PassportNumber { get; set; }
    
    [RoomList("Nationality", 7)]
    public string? Nationality { get; set; }
    
    [RoomList("Expiration Date ", 8, true)]
    public DateTime? ExpirationDate { get; set; }
    
    [RoomList("comments", 9)]
    public string? Comments { get; set; }
    
    [RoomList("departure date", 10, true)]
    public DateTime? DepartureDate { get; set; }
    
    [RoomList("age", 11)]
    public int? Age { get; set; }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class RoomListAttribute : Attribute
{
    public RoomListAttribute(
        string title,
        int position,
        bool isDate = false)
    {
        Title = title;
        Position = position;
        IsDate = isDate;
    }

    public string Title { get; }

    public int Position { get; }
    public bool IsDate { get; }
}