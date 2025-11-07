using System.Collections.Immutable;
using Validated.Core.Common.Constants;
using Validated.Core.Types;
using Validated.RuleConfigurations.ConsoleClient.Common.Models;

namespace Validated.RuleConfigurations.ConsoleClient.Common.Data;

internal class StaticData
{
    public static ContactDto CreateContactObjectGraph()
    {
        List<ContactMethodDto> contactMethods = [new("MethodTypeOne", "MethodValueOne"), new("MethodTypeTwo", "MethodValueTwo")];

        return new() {Age = 42, ContactMethods = contactMethods, FamilyName="Doe", GivenName = "John", Title="Mr" };
    }


    /*
        * Here is our base rule configs which by default will use have the TenantID of ALL and the CultureID of en-GB - You should always have this base 
        * as if no TenantID and CultureID matches are found it falls back to matching the TenantID ALL and CultureID of en-GB. Without this there is no match/default rule.
        * 
        * Just make your base and then add TenantID and Specific cultures where needed.
        * 
        * Note that the FamilyName has two rules which will get combined with AndThen to form the single FamilyName validator a single validator. 
        * You can have as many rules as you like.
    */ 

    public static ImmutableList<ValidationRuleConfig> GetValidationRuleConfigs()

        => [
                    new("Validated.RuleConfigurations.ConsoleClient.Common.Models.ContactDto", "Title", "Title", "RuleType_Regex", "MinMaxToValueType_String", @"^(Mr|Mrs|Ms|Dr|Prof)$", "Must be one of Mr, Mrs, Ms, Dr, Prof", 2, 4),
                    new("Validated.RuleConfigurations.ConsoleClient.Common.Models.ContactDto", "GivenName", "First name", "RuleType_Regex", "MinMaxToValueType_String", @"^(?=.{2,50}$)[A-Z]+['\- ]?[A-Za-z]*['\- ]?[A-Za-z]+$", "Must start with a capital letter and be between 2 and 50 characters in length", 2, 50),
                    new("Validated.RuleConfigurations.ConsoleClient.Common.Models.ContactDto", "FamilyName", "Surname", "RuleType_Regex", "MinMaxToValueType_String", @"^[A-Z]+['\- ]?[A-Za-z]*['\- ]?[A-Za-z]*$", "Must start with a capital letter", 2, 50, "", "", "", "","", ValidatedConstants.TargetType_Item),
                    new("Validated.RuleConfigurations.ConsoleClient.Common.Models.ContactDto", "FamilyName", "Surname", "RuleType_StringLength", "", "", "Must be between 2 and 50 characters long", 2, 50),
                    new("Validated.RuleConfigurations.ConsoleClient.Common.Models.ContactDto", "Age", "Age", "RuleType_CompareTo", "MinMaxToValueType_Int32", "", "Must be 18 or over", 0, 0, "", "","18","","CompareType_GreaterThanOrEqual"),
  
                    new("Validated.RuleConfigurations.ConsoleClient.Common.Models.ContactMethodDto", "ContactMethods", "Contact methods", ValidatedConstants.RuleType_CollectionLength, "", "", "Must have at least 1 item but no more than 10", 1, 10, "", "","","", "",ValidatedConstants.TargetType_Collection),
                    new("Validated.RuleConfigurations.ConsoleClient.Common.Models.ContactMethodDto", "MethodType", "Contact method", "RuleType_StringLength", "", "", "Must be between 2 and 20 characters", 2, 20, "", "","","", ""),
                    new("Validated.RuleConfigurations.ConsoleClient.Common.Models.ContactMethodDto", "MethodValue", "Value","RuleType_StringLength", "", "", "Must be between 2 and 20 characters", 2, 20, "", "","","", ""),

           ];
}
