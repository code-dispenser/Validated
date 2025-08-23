namespace Validated.Core.Tests.SharedDataFixtures.Common.Models;

public record class ContactDto
{
    public string   Title       { get; set; } = default!;
    public string  GivenName    { get; set; } = default!;
    public string   FamilyName  { get; set; } = default!;
    public DateOnly DOB         { get; set; } = default!;
    public DateOnly CompareDOB  { get; set; } = default!;
    public string   Email       { get; set; } = default!;
    public string?  Mobile      { get; set; }
    public int?     NullableAge { get; set; }
    public int      Age         { get; set; }

    public List<string> Entries { get; set; } = [];

    public AddressDto? Address { get; set; } 

    public List<ContactMethodDto> ContactMethods { get; set; } = [];

}
