using System.Linq;

namespace DevTools.Application.Converters.FaM;

using System.Text.RegularExpressions;

public static class StaffParser
{
    public static Staff? Parse(string input, string firstName)
    {
        var personSections = Regex.Split(input.Split("***").Last(), @"\*(.*?)\*");

        var sections = personSections.Skip(1).Chunk(2).Select(e => new Section(e[0], e[1]));
        
        var remarksParticipantsMatch = Regex.Match(input, @"\*\*\*Reisende\*\*\*(.*?)\*\*\*Zusatzdaten Reisende\*\*\*",
            RegexOptions.Singleline);

        var remarksParticipants = remarksParticipantsMatch.Groups[1].Value.Trim();

        var remarksBookingMatch = Regex.Match(input, @"\*\*\*Buchung\*\*\*(.*?)\*\*\*Reisende\*\*\*",
            RegexOptions.Singleline);

        var remarksBooking = remarksBookingMatch.Groups[1].Value.Trim();

        foreach (var (name, content) in sections)
        {
            if (!name.Contains(firstName))
            {
                continue;
            }
            
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            var teamMatch = Regex.Match(content, @"Ich melde mich an für:(.*?)Mein Beruf:", RegexOptions.Singleline);
            Team? team = teamMatch.Groups[1].Value.Trim() switch
            {
                "Erwachsene" => Team.Adults,
                "Youth" => Team.Youth,
                "Kids" => Team.Kids,
                _ => null,
            };

            if (team is null)
            {
                continue;
            }

            var professionMatch =
                Regex.Match(content, @"Mein Beruf:(.*?)Besondere Fähigkeiten:", RegexOptions.Singleline);
            var skillsMatch = Regex.Match(content, @"Besondere Fähigkeiten:(.*?)Kleingruppenleiter:",
                RegexOptions.Singleline);
            var smallGroupLeaderMatch = Regex.Match(content, @"Kleingruppenleiter:(.*?)Mein Dienst in der GvC:",
                RegexOptions.Singleline);
            var gvcServiceMatch = Regex.Match(content, @"Mein Dienst in der GvC:(.*?)Fahrausweis und Kategorie:",
                RegexOptions.Singleline);
            var driversLicenceMatch = Regex.Match(content,
                @"Fahrausweis und Kategorie:(.*?)Falls Fahrausweis vorhanden  seit wann\?:", RegexOptions.Singleline);
            var licenceDateMatch = Regex.Match(content, @"Falls Fahrausweis vorhanden  seit wann\?:(.*?)Ich würde fahren:",
                RegexOptions.Singleline);
            var wouldDriveMatch =
                Regex.Match(content, @"Ich würde fahren:(.*?)Auto vorhanden\?:", RegexOptions.Singleline);
            var haveCarMatch = Regex.Match(content, @"Auto vorhanden\?:(.*?)Marke  Typ und Kontrollschild:",
                RegexOptions.Singleline);
            var carDetailsMatch = Regex.Match(content, @"Marke  Typ und Kontrollschild:(.*?)Helfer Auswahl:",
                RegexOptions.Singleline);
            var serviceHelpMatch = Regex.Match(content, @"Helfer Auswahl:(.*?)Band:", RegexOptions.Singleline);
            var bandMatch = Regex.Match(content, @"Band:(.*?)Theater:", RegexOptions.Singleline);
            var theaterMatch = Regex.Match(content, @"Theater:(.*?)Technik:", RegexOptions.Singleline);
            var technikMatch = Regex.Match(content, @"Technik:(.*?)Singen:", RegexOptions.Singleline);
            var singenMatch = Regex.Match(content, @"Singen:(.*?)Worshipdance:", RegexOptions.Singleline);
            var worshipDanceMatch = Regex.Match(content, @"Worshipdance:(.*?)Kinderhüte:", RegexOptions.Singleline);
            var kinderhüteMatch = Regex.Match(content, @"Kinderhüte:(.*?)Musikinstrument:", RegexOptions.Singleline);
            var instrumentsMatch = Regex.Match(content, @"Musikinstrument:\s+(.*?)(?=\s*\*|$)", RegexOptions.Singleline);

            var profession = professionMatch.Groups[1].Value.Clean();
            var skills = skillsMatch.Groups[1].Value.Clean();
            var smallGroupLeader = smallGroupLeaderMatch.Groups[1].Value.Clean();
            var gvcService = gvcServiceMatch.Groups[1].Value.Clean();
            var driversLicence = driversLicenceMatch.Groups[1].Value.Clean();
            var licenceDate = licenceDateMatch.Groups[1].Value.Clean();
            var wouldDrive = wouldDriveMatch.Groups[1].Value.Trim().ToLower() == "ja";
            var haveCar = haveCarMatch.Groups[1].Value.Trim().ToLower() == "ja";
            var carDetails = carDetailsMatch.Groups[1].Value.Clean();
            var serviceHelp = serviceHelpMatch.Groups[1].Value.Clean();
            var band = bandMatch.Groups[1].Value.Trim().ToLower() == "true";
            var theater = theaterMatch.Groups[1].Value.Trim().ToLower() == "true";
            var technik = technikMatch.Groups[1].Value.Trim().ToLower() == "true";
            var singen = singenMatch.Groups[1].Value.Trim().ToLower() == "true";
            var worshipdance = worshipDanceMatch.Groups[1].Value.Trim().ToLower() == "true";
            var kinderhüte = kinderhüteMatch.Groups[1].Value.Trim().ToLower() == "true";
            var instruments = instrumentsMatch.Groups[1].Value.Clean();

            var staff = new Staff(
                RemarksParticipants: remarksParticipants,
                RemarksBooking: remarksBooking,
                Name: name,
                Team: team.Value,
                Profession: profession,
                Skills: skills,
                SmallGroupLeader: smallGroupLeader,
                GvCService: gvcService,
                DriversLicence: driversLicence,
                LicenceDate: licenceDate,
                WouldDrive: wouldDrive,
                HaveCar: haveCar,
                CarDetails: carDetails,
                ServiceHelp: serviceHelp,
                Band: band,
                Theater: theater,
                Technic: technik,
                Singing: singen,
                WorshipDance: worshipdance,
                ChildCare: kinderhüte,
                Instruments: instruments
            );

            return staff;
        }

        return null;
    }

    private static string Clean(this string value)
    {
        var str = value.Trim().Replace("  ", ", ");
        return str.Length == 1 ? str.Replace("-", string.Empty) : str;
    }

    private record Section(string Name, string Content);
}