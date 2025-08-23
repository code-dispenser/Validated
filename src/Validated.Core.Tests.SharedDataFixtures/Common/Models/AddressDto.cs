namespace Validated.Core.Tests.SharedDataFixtures.Common.Models;

public record class AddressDto
{
    public string AddressLine { get; set; } = default!;
    public string TownCity    { get; set; } = default!;
    public string County      { get; set; } = default!;
    public string? Postcode   { get; set; }
}