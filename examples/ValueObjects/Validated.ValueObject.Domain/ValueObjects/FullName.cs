using Validated.Core.Extensions;
using Validated.Core.Types;

namespace Validated.ValueObject.Domain.ValueObjects;

public record class FullName
{
    public string GivenName  { get; }
    public string FamilyName { get; }

    private FullName(string givenName, string familyName)

        => (GivenName, FamilyName) = (givenName, familyName);

    /*
        * Simplified way using the combine extension. 
        * Returns either a valid Validated<FullName> or one that has the invalid entries
        * We only pass in validated items for the create method to check.
        * We use internal so only the domain can do this or delegate it to a trusted domain service.
    */ 
    internal static Validated<FullName> Create(Validated<string> validatedGivenName, Validated<string> validatedFamilyName)
    
        => (validatedGivenName, validatedFamilyName).Combine((givenName, familyName) => new FullName(givenName, familyName)); 

    /*
        * The Applicative Functor way using a curried function 
    */
    internal static Validated<FullName> CreateUsingApplicative(Validated<string> validatedGivenName, Validated<string> validatedFamilyName)
    {
        Func<string, Func<string, FullName>> curriedFunc = given => family => new FullName(given, family);

        return Validated<Func<string, Func<string, FullName>>>.Valid(curriedFunc)
                    .Apply(validatedGivenName)
                        .Apply(validatedFamilyName);
    }
}
