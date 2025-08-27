using Validated.Core.Extensions;
using Validated.Core.Types;
using Validated.Core.Validators;

namespace Validated.ValueObject.Shared.Validators;

/*
     * Put all your shared validators in classes like this and use where appropriate, on the client on the server etc. 
 */ 
public static class CommonValidators
{
    public static MemberValidator<string> GivenNameValidator()

           => MemberValidators.CreateStringRegexValidator(@"^(?=.{2,50}$)[A-Z]+['\- ]?[A-Za-z]*['\- ]?[A-Za-z]+$", "GivenName", "First name", "Must start with a capital letter, no double spaces, dashes or apostrophes, and be between 2 and 50 characters in length");

    /*
        * I removed the length part of the regex to spilt the validation into two parts 
    */ 
    public static MemberValidator<string> FamilyNameValidator()

       => MemberValidators.CreateStringRegexValidator(@"^[A-Z]+['\- ]?[A-Za-z]*['\- ]?[A-Za-z]*$", "FamilyName", "Surname", "Must start with a capital letter, no double spaces, dashes or apostrophes")
            .AndThen(MemberValidators.CreateStringLengthValidator(2, 50, "FamilyName", "Surname", "Must be between 2 and 50 characters in length"));
}
