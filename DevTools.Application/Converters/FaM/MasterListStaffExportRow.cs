namespace DevTools.Application.Converters.FaM;

public class MasterListStaffExportRow
{
    public int RatioPersonNumber { get; set; }
    public string LastName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? RemarksBooking { get; set; }
    public string? RemarksTravelers { get; set; }
    public Team Team { get; set; }
    public string? Profession { get; set; }
    public string? SpecialSkills { get; set; }
    public string? SmallGroupLeader { get; set; }
    public string? ChurchService { get; set; }
    public string? DriversLicence { get; set; }
    public string? LicenceSince { get; set; }
    public bool WouldDrive { get; set; }
    public bool HasCar { get; set; }
    public string? CarDetails { get; set; }
    public string? HelpDuringService { get; set; }
    public bool Band { get; set; }
    public bool Theater { get; set; }
    public bool Technic     { get; set; }
    public bool Singing { get; set; }
    public bool WorshipDance { get; set; }
    public bool ChildCare { get; set; }
    public string? MusicInstrument { get; set; }
}