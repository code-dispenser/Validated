using System.Collections;
using Validated.Core.Common.Constants;
using Validated.Core.Extensions;
using Validated.Core.Types;
using Validated.Core.Validators;

namespace Validated.Core.ConsoleDemo.Common.SharedValidators;


public static class GeneralFieldValidators
{
    /*
        * Lots of objects may share the same field names/data types and validation requirements. 
    */

    public static MemberValidator<string> TitleValidator()

     => MemberValidators.CreateStringRegexValidator("^(Mr|Mrs|Ms|Dr|Prof)$", "Title", "Title", "Must be one of Mr, Mrs, Ms, Dr, Prof");

    public static MemberValidator<string> GivenNameValidator()

           => MemberValidators.CreateStringRegexValidator(@"^(?=.{2,50}$)[A-Z]+['\- ]?[A-Za-z]*['\- ]?[A-Za-z]+$", "GivenName", "First name", "Must start with a capital letter and be between 2 and 50 characters in length");

    public static MemberValidator<string> FamilyNameValidator()

       => MemberValidators.CreateStringRegexValidator(@"^[A-Z]+['\- ]?[A-Za-z]*['\- ]?[A-Za-z]*$", "FamilyName", "Surname", "Must start with a capital letter")
            .AndThen(MemberValidators.CreateStringLengthValidator(2, 50, "FamilyName", "Surname", "Must be between 2 and 50 characters in length"));

    public static MemberValidator<int> AgeValidator()

        => MemberValidators.CreateRangeValidator(10, 50, "Age", "Age", "Must be between 10 and 50");

    public static MemberValidator<int> NullableAgeValidator()

        => MemberValidators.CreateRangeValidator(10, 50, "NullableAge", "NullableAge", "Must be between 10 and 50");

    public static MemberValidator<string> MobileValidator()

        => MemberValidators.CreateStringRegexValidator(@"^(?:\+[1-9]\d{1,3}[ -]?7\d{9}|07\d{9})$", "Mobile", "Mobile Tel", "Must be a valid UK mobile number format");

    public static MemberValidator<string> AddressLineValidator()

    => MemberValidators.CreateStringRegexValidator(@"^(?=.{5,250}$)(?!.* {2})(?!.*[,\-']{2})[A-Za-z0-9][A-Za-z0-9 ,\-\n']+[A-Za-z0-9]$", "AddressLine",
                                                        "Address Line", "Must start with a letter or number and be 5 to 250 characters in length.");

    public static MemberValidator<string> TownCityValidator()

        => MemberValidators.CreateStringRegexValidator(@"^(?=.{3,100}$)[A-Z](?!.* {2})(?!.*'{2})(?!.*-{2})[\-A-Za-z ']+[a-z]+$", "TownCity",
                                                    "Town / City", "Must start with a capital letter and be between 3 to 100 characters in length.");
    public static MemberValidator<string> CountyValidator()

           => MemberValidators.CreateStringRegexValidator(@"^(?=.{3,100}$)[A-Z](?!.* {2})(?!.*'{2})(?!.*-{2})[\-A-Za-z ']+[a-z]+$", "County",
                                                        "County", "Must start with a capital letter and be between 3 to 100 characters in length.");

    public static MemberValidator<string> UKPostcodeValidator()

        => MemberValidators.CreateStringRegexValidator(@"^(GIR 0AA)|((([ABCDEFGHIJKLMNOPRSTUWYZ][0-9][0-9]?)|(([ABCDEFGHIJKLMNOPRSTUWYZ][ABCDEFGHKLMNOPQRSTUVWXY][0-9][0-9]?)|(([ABCDEFGHIJKLMNOPRSTUWYZ][0-9][ABCDEFGHJKSTUW])|([ABCDEFGHIJKLMNOPRSTUWYZ][ABCDEFGHKLMNOPQRSTUVWXY][0-9][ABEHMNPRVWXY])))) [0-9][ABDEFGHJLNPQRSTUWXYZ]{2})$",
                                                "Postcode", "Postcode", "Must be a valid UK formatted postcode.");


    public static MemberValidator<string> EntryValidator()

        => MemberValidators.CreateNotNullOrEmptyValidator<string>("Entry", "Entry", "Required, cannot be missing, null or empty");

    public static MemberValidator<string> MethodValueValidator()

        => MemberValidators.CreateNotNullOrEmptyValidator<string>("MethodType", "Type", "Required, cannot be missing, null or empty");

    public static MemberValidator<string> MethodTypeValidator()

        => MemberValidators.CreateNotNullOrEmptyValidator<string>("MethodValue", "Value", "Required, cannot be missing, null or empty");

    public static MemberValidator<List<string>> EntryCountValidator()

        => MemberValidators.CreateCollectionLengthValidator<List<string>>(1, 3, "Entries", "Entries", $"Must contain between 1 and 3 items but the collection contained {FailureMessageTokens.ACTUAL_LENGTH} items");


            
}
