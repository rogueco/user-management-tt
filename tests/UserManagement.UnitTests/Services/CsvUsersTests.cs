using UserManagement.Services.Services.ImportServices;

namespace UserManagement.UnitTests.Services;

public class CsvUsersTests
{
    [Fact]
    public void Parse_MustReadValidRowsAndReportBadOnesByRowNumber()
    {
        const string csv = """
            Forename,Surname,Email,DateOfBirth,IsActive
            Ada,Lovelace,ada@example.com,1985-12-10,true
            Alan,Turing,alan@example.com,23/06/1982,no
            Grace,Hopper,grace@example.com,not-a-date,true
            Dennis,Ritchie,dennis@example.com,1971-09-09,maybe
            """;

        var rows = CsvUsers.Parse(csv);

        rows.Should().HaveCount(4);
        rows[0].Request!.Should().BeEquivalentTo(new { Forename = "Ada", Email = "ada@example.com", DateOfBirth = new DateOnly(1985, 12, 10), IsActive = true });
        rows[1].Request!.Should().BeEquivalentTo(new { DateOfBirth = new DateOnly(1982, 6, 23), IsActive = false });
        rows[2].Should().BeEquivalentTo(new { Row = 4, Request = (object?)null }, options => options.ExcludingMissingMembers());
        rows[2].Error.Should().Contain("not-a-date");
        rows[3].Error.Should().Contain("maybe");
    }

    [Fact]
    public void Parse_WhenEmpty_MustReturnNoRows()
    {
        CsvUsers.Parse(string.Empty).Should().BeEmpty();
        CsvUsers.Parse("Forename,Surname,Email,DateOfBirth,IsActive").Should().BeEmpty();
    }
}
