namespace Validated.RuleConfigurations.ConsoleClient.Common.Models;
/*
    * Just a reduced model to keep things simple for our configs 
*/ 

public record class ContactMethodDto(string MethodType, string MethodValue);

public record class ContactDto
{
    public string   Title       { get; set; } = default!;
    public string   GivenName   { get; set; } = default!;
    public string   FamilyName  { get; set; } = default!;
    public int      Age         { get; set; }

    public List<ContactMethodDto> ContactMethods  { get; set; } = [];

}

