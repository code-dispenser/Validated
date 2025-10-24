namespace Validated.Core.Common.Constants;

internal static class ErrorMessages
{
    /*
        * Constants with User in them are seen by end users. 
    */ 
    public const string Validator_Factory_User_Failure_Message      = "System validation error, please contact support.";
    public const string Validator_Creation_Error_Message            = "Unable to create the validator for rule: {ruleConfig}";
    public const string Validator_Creation_Failure_User_Message     = "System validation error. If this persists please contact support.";
    public const string Validator_No_Rules_Error_Message            = "No rules found for member: {member}";

    public const string Validator_Max_Depth_Exceeded_User_Message   = "Maximum validation depth exceeded";

    public const string Validator_Member_Null_Value_User_Message    = "System validation error, the value cannot be null. If this persists please contact support.";

    public const string Validator_Entity_Null_User_Message          = "System validation error, entity cannot be null. If this persists please contact support.";

    public const string Validator_Bad_Expression_Message            = "Failed to compile or analyse the selector expression";

    public const string Validator_Nesting_Unsupported_Message       = "Deeply nested member access is not supported in {0}. The expression {1} reaches through multiple objects. " +
                                                                       "Please use 'ForNestedMember' to validate the nested property with a dedicated validator for the nested type.";

    public const string Validator_Factory_Not_Found                 = "The validator factory for the rule type of: {ruleType} was not found. Using the failing validator to fail the validation";

    public const string Validation_Builder_Unbalenced_DoWhen        = "The DoWhen method(s) are not closed, missing {0} EndWhen(S). Please add the missing EndWhen(s)";

    public const string Default_Failure_Message = "Validation failure";
}
