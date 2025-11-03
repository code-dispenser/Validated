namespace Validated.TenantValidators.ConsoleClient.Common.Models;

public record class ContactMethodDto(string MethodType, string MethodValue);
public record class AddressDto
{
    public string AddressLine { get; set; } = default!;
    public string TownCity { get; set; } = default!;
    public string County { get; set; } = default!;
    public string? Postcode { get; set; }
}
public record class ContactDto
{
    public string Title { get; set; } = default!;
    public string GivenName { get; set; } = default!;
    public string FamilyName { get; set; } = default!;
    public DateOnly DOB { get; set; } = default!;
    public DateOnly CompareDOB { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Mobile { get; set; }
    public int? NullableAge { get; set; }
    public int Age { get; set; }

    public string? NullableStringUrl { get; set; } = null!;

    public string StringAmount { get; set; } = "0.00";

    public List<string> Entries { get; set; } = [];

    public AddressDto Address { get; set; } = default!;

    public AddressDto? NullableAddress { get; set; }

    public List<ContactMethodDto> ContactMethods { get; set; } = [];

}