using System.Collections.Immutable;
using Validated.Core.Common.Constants;
using Validated.Core.Types;
using Validated.TenantValidationBuilder.ConsoleClient.Common.Models;

namespace Validated.TenantValidationBuilder.ConsoleClient.Common.Data;

internal class StaticData
{
    public static ContactDto CreateContactObjectGraph()
    {
        var dob      = new DateOnly(1980, 1, 1);
        var olderDob = new DateOnly(1980, 1, 2);

        var nullableAge = DateTime.Now.Year - dob.Year - (DateTime.Now.DayOfYear < dob.DayOfYear ? 1 : 0);
        var age         = DateTime.Now.Year - dob.Year - (DateTime.Now.DayOfYear < dob.DayOfYear ? 1 : 0);

        AddressDto address = new() { AddressLine = "AddressLine", County = "County", Postcode="PostCode", TownCity="Town" };
        List<ContactMethodDto> contactMethods = [new("MethodTypeOne", "MethodValueOne"), new("MethodTypeTwo", "MethodValueTwo")];

        return new() { Address = address, NullableAge = nullableAge, Age = age, ContactMethods = contactMethods, DOB = dob, CompareDOB = olderDob, Email = "john.doe@gmail.com", FamilyName="Doe", GivenName = "John", Mobile="123456789", Title="Mr" };
    }

    public static Node BuildNodeTree(int depthIncludingRoot)
    {
        depthIncludingRoot = depthIncludingRoot < 2 ? 2 : depthIncludingRoot;

        var root = new Node { Name = "Root" };
        var current = root;

        for (int index = 1; index < depthIncludingRoot; index++)
        {
            var childNode = new Node { Name = $"Child-{index:D2}" };
            current.Child = childNode;
            current       = childNode;
        }

        return root;
    }
    public static ImmutableList<ValidationRuleConfig> GetValidationRuleConfigs()

        => [
                new("Validated.TenantValidationBuilder.ConsoleClient.Common.Models.ContactDto", "Title", "Title", "RuleType_Regex", "MinMaxToValueType_String", @"^(Mr|Mrs|Ms|Dr|Prof)$", "Must be one of Mr, Mrs, Ms, Dr, Prof", 2, 4),
                new("Validated.TenantValidationBuilder.ConsoleClient.Common.Models.ContactDto", "GivenName", "First name", "RuleType_Regex", "MinMaxToValueType_String", @"^(?=.{2,50}$)[A-Z]+['\- ]?[A-Za-z]*['\- ]?[A-Za-z]+$", "Must start with a capital letter and be between 2 and 50 characters in length", 2, 50),
                new("Validated.TenantValidationBuilder.ConsoleClient.Common.Models.ContactDto", "FamilyName", "Surname", "RuleType_Regex", "MinMaxToValueType_String", @"^[A-Z]+['\- ]?[A-Za-z]*['\- ]?[A-Za-z]*$", "Must start with a capital letter (TenantOne)", 2, 50, "", "", "", "","", ValidatedConstants.TargetType_Item),
                new("Validated.TenantValidationBuilder.ConsoleClient.Common.Models.ContactDto", "FamilyName", "Surname", "RuleType_StringLength", "", "", "Must be between 2 and 50 characters long", 2, 50),
                new("Validated.TenantValidationBuilder.ConsoleClient.Common.Models.ContactDto", "Email", "Email", "RuleType_Regex", "MinMaxToValueType_String", @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", "Must be a valid email format", 4, 75),
                new("Validated.TenantValidationBuilder.ConsoleClient.Common.Models.ContactDto", "CompareDOB", "Compare date of birth", "RuleType_MemberComparison", "", "", "Compare date of birth must be greater than the date of birth", 0, 0, "", "","","DOB", "CompareType_GreaterThan"),

                new("Validated.TenantValidationBuilder.ConsoleClient.Common.Models.ContactDto", "DOB", "Date of birth", "RuleType_RollingDate", "MinMaxToValueType_Year", "", "Date of birth must be between {MinDate} and {MaxDate}", 10, 10, "-122", "-18"),
                new("Validated.TenantValidationBuilder.ConsoleClient.Common.Models.ContactDto", "Mobile", "Mobile Number", "RuleType_Regex", "MinMaxToValueType_String", @"^(?:\+[1-9]\d{1,3}[ -]?7\d{9}|07\d{9})$", "Must be a valid UK mobile number format", 11, 16, "", ""),
                new("Validated.TenantValidationBuilder.ConsoleClient.Common.Models.ContactDto", "Age", "Age", "RuleType_CompareTo", "MinMaxToValueType_Int32", "", "Must be 18 or over", 0, 0, "", "","18","","CompareType_GreaterThanOrEqual"),
                new("Validated.TenantValidationBuilder.ConsoleClient.Common.Models.ContactDto", "NullableAge", "Nullable age", "RuleType_Range", "MinMaxToValueType_Int32", "", "Must be between 10 and 50", 1, 3, "10", "50"),

                new("Validated.TenantValidationBuilder.ConsoleClient.Common.Models.ContactDto", "Entries", "Entries", "RuleType_StringLength", "", "", "Must be between 1 and 10 characters in length", 1, 10, "", ""),
                new("Validated.TenantValidationBuilder.ConsoleClient.Common.Models.ContactDto", "Entries", "Entries", ValidatedConstants.RuleType_CollectionLength, "", "", "Must have at least 1 item but no more than 5", 1, 5, "", "","","", "",ValidatedConstants.TargetType_Collection),

                new("Validated.TenantValidationBuilder.ConsoleClient.Common.Models.AddressDto", "AddressLine", "Address Line", "RuleType_Regex", "MinMaxToValueType_String", @"^(?=.{5,250}$)(?!.* {2})(?!.*[,\-']{2})[A-Za-z0-9][A-Za-z0-9 ,\-\n']+[A-Za-z0-9]$", "Must start with a letter or number and be 5 to 250 characters in length.", 5, 250),
                new("Validated.TenantValidationBuilder.ConsoleClient.Common.Models.AddressDto", "TownCity", "Town / City", "RuleType_Regex", "MinMaxToValueType_String", @"^(?=.{3,100}$)[A-Z](?!.* {2})(?!.*'{2})(?!.*-{2})[\-A-Za-z ']+[a-z]+$", "Must start with a capital letter and be between 3 to 100 characters in length.", 3, 100),
                new("Validated.TenantValidationBuilder.ConsoleClient.Common.Models.AddressDto", "County", "County", "RuleType_Regex", "MinMaxToValueType_String", @"^(?=.{3,100}$)[A-Z](?!.* {2})(?!.*'{2})(?!.*-{2})[\-A-Za-z ']+[a-z]+$", "Must start with a capital letter and be between 3 to 100 characters in length.", 3, 100),
                new("Validated.TenantValidationBuilder.ConsoleClient.Common.Models.AddressDto", "Postcode", "Postcode", "RuleType_Regex", "MinMaxToValueType_String", @"^(GIR 0AA)|((([ABCDEFGHIJKLMNOPRSTUWYZ][0-9][0-9]?)|(([ABCDEFGHIJKLMNOPRSTUWYZ][ABCDEFGHKLMNOPQRSTUVWXY][0-9][0-9]?)|(([ABCDEFGHIJKLMNOPRSTUWYZ][0-9][ABCDEFGHJKSTUW])|([ABCDEFGHIJKLMNOPRSTUWYZ][ABCDEFGHKLMNOPQRSTUVWXY][0-9][ABEHMNPRVWXY])))) [0-9][ABDEFGHJLNPQRSTUWXYZ]{2})$",
                                                                                            "Must be a valid UK formatted postcode.", 5, 15),

                new("Validated.TenantValidationBuilder.ConsoleClient.Common.Models.ContactMethodDto", "ContactMethods", "Contact methods", ValidatedConstants.RuleType_CollectionLength, "", "", "Must have at least 1 item but no more than 10", 1, 10, "", "","","", "",ValidatedConstants.TargetType_Collection),
                new("Validated.TenantValidationBuilder.ConsoleClient.Common.Models.ContactMethodDto", "MethodType", "Contact method", "RuleType_StringLength", "", "", "Must be between 2 and 20 characters", 2, 20, "", "","","", ""),
                new("Validated.TenantValidationBuilder.ConsoleClient.Common.Models.ContactMethodDto", "MethodValue", "Value","RuleType_StringLength", "", "", "Must be between 2 and 20 characters", 2, 20, "", "","","", ""),

                new("Validated.TenantValidationBuilder.ConsoleClient.Common.Models.Node", "Name","Name", ValidatedConstants.RuleType_StringLength,"","","Should be between 4 and 10 characters in length found {ValidatedValue}",4,10)

           ];
}
