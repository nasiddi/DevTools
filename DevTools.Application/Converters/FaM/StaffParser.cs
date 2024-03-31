using System.Linq;

namespace DevTools.Application.Converters.FaM;

using System.Text.RegularExpressions;

public static class StaffParser
{
    public static Staff? Parse(string input, string firstName)
    {
        var personSections = Regex.Split(input.Split("***").Last(), @"\*(.*?)\*");

        var remarksParticipantsMatch = Regex.Match(input, @"\*\*\*Reisende\*\*\*(.*?)\*\*\*Zusatzdaten Reisende\*\*\*",
            RegexOptions.Singleline);

        var remarksParticipants = remarksParticipantsMatch.Groups[1].Value.Trim();

        var remarksBookingMatch = Regex.Match(input, @"\*\*\*Buchung\*\*\*(.*?)\*\*\*Reisende\*\*\*",
            RegexOptions.Singleline);

        var remarksBooking = remarksBookingMatch.Groups[1].Value.Trim();


        foreach (var section in personSections)
        {
            if (string.IsNullOrWhiteSpace(section))
            {
                continue;
            }

            var teamMatch = Regex.Match(input, @"Ich melde mich an für:(.*?)Mein Beruf:", RegexOptions.Singleline);
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

            var nameMatch = Regex.Match(input, @"\*\*\*Zusatzdaten Reisende\*\*\*(.*?)Ich melde mich an für:",
                RegexOptions.Singleline);

            var name = nameMatch.Groups[1].Value.Trim().Trim('*');

            if (!name.Contains(firstName))
            {
                return null;
            }

            var professionMatch =
                Regex.Match(input, @"Mein Beruf:(.*?)Besondere Fähigkeiten:", RegexOptions.Singleline);
            var skillsMatch = Regex.Match(input, @"Besondere Fähigkeiten:(.*?)Kleingruppenleiter:",
                RegexOptions.Singleline);
            var smallGroupLeaderMatch = Regex.Match(input, @"Kleingruppenleiter:(.*?)Mein Dienst in der GvC:",
                RegexOptions.Singleline);
            var gvcServiceMatch = Regex.Match(input, @"Mein Dienst in der GvC:(.*?)Fahrausweis und Kategorie:",
                RegexOptions.Singleline);
            var driversLicenceMatch = Regex.Match(input,
                @"Fahrausweis und Kategorie:(.*?)Falls Fahrausweis vorhanden  seit wann\?:", RegexOptions.Singleline);
            var licenceDateMatch = Regex.Match(input, @"Falls Fahrausweis vorhanden  seit wann\?:(.*?)Ich würde fahren:",
                RegexOptions.Singleline);
            var wouldDriveMatch =
                Regex.Match(input, @"Ich würde fahren:(.*?)Auto vorhanden\?:", RegexOptions.Singleline);
            var haveCarMatch = Regex.Match(input, @"Auto vorhanden\?:(.*?)Marke  Typ und Kontrollschild:",
                RegexOptions.Singleline);
            var carDetailsMatch = Regex.Match(input, @"Marke  Typ und Kontrollschild:(.*?)Helfer Auswahl:",
                RegexOptions.Singleline);
            var serviceHelpMatch = Regex.Match(input, @"Helfer Auswahl:(.*?)Band:", RegexOptions.Singleline);
            var bandMatch = Regex.Match(input, @"Band:(.*?)Theater:", RegexOptions.Singleline);
            var theaterMatch = Regex.Match(input, @"Theater:(.*?)Technik:", RegexOptions.Singleline);
            var technikMatch = Regex.Match(input, @"Technik:(.*?)Singen:", RegexOptions.Singleline);
            var singenMatch = Regex.Match(input, @"Singen:(.*?)Worshipdance:", RegexOptions.Singleline);
            var worshipDanceMatch = Regex.Match(input, @"Worshipdance:(.*?)Kinderhüte:", RegexOptions.Singleline);
            var kinderhüteMatch = Regex.Match(input, @"Kinderhüte:(.*?)Musikinstrument:", RegexOptions.Singleline);
            var instrumentsMatch = Regex.Match(input, @"Musikinstrument:\s+(.*?)(?=\s*\*|$)", RegexOptions.Singleline);

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
}