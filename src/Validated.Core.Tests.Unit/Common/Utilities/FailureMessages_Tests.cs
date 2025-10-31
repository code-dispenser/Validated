using FluentAssertions;
using Validated.Core.Common.Constants;
using Validated.Core.Common.Utilities;

using Token = Validated.Core.Common.Constants.FailureMessageTokens;

namespace Validated.Core.Tests.Unit.Common.Utilities;

public class FailureMessages_Tests
{
    [Theory]
    [InlineData("Opening brace { in the string", true)]
    [InlineData("No brace in the string", false)]
    [InlineData(null, false)]
    public void Requires_token_replacement_should_return_true_if_an_opening_brace_is_in_the_string(string? messageTemplate, bool happyPath)
    {
        if(true == happyPath)
        {
            FailureMessages.RequiresTokenReplacement(messageTemplate!).Should().BeTrue();
            return;
        }

        FailureMessages.RequiresTokenReplacement(messageTemplate!).Should().BeFalse();

    }

    [Fact]
    public void Format_should_replace_the_value_to_validate_and_display_name_tokens_if_found_in_the_string()
    {
        var messageTemplate = $"The value for the {Token.DISPLAY_NAME} input was: {Token.VALIDATED_VALUE} which is not valid";

        var failureMessage = FailureMessages.Format(messageTemplate, "42", "Date of birth");

        failureMessage.Should().Be("The value for the Date of birth input was: 42 which is not valid");
    }

    [Fact]
    public void Format_string_length_message_should_replace_the_value_to_validate_and_display_name_and_actual_length_tokens_if_found_in_the_string()
    {
        var valueToValidate = "Hello World";
        var messageTemplate = $"The length of the string {Token.VALIDATED_VALUE} for {Token.DISPLAY_NAME} was: {Token.ACTUAL_LENGTH} which is not valid";

        var failureMessage = FailureMessages.FormatStringLengthMessage(messageTemplate, valueToValidate, "Programmers", valueToValidate.Length.ToString());

        failureMessage.Should().Be($"The length of the string {valueToValidate} for Programmers was: {valueToValidate.Length} which is not valid");
    }

    [Fact]
    public void Format_collection_length_message_should_replace_the_display_name_and_actual_length_tokens_if_found_in_the_string()
    {
        var messageTemplate = $"The length of the {Token.DISPLAY_NAME} collection was: {Token.ACTUAL_LENGTH} which is not valid";

        var failureMessage = FailureMessages.FormatCollectionLengthMessage(messageTemplate, "Contacts", "0");

        failureMessage.Should().Be($"The length of the Contacts collection was: 0 which is not valid");
    }

    [Fact]
    public void Format_compare_value_message_should_replace_the_value_to_validate_and_display_name_and_compare_to_value_tokens_if_found_in_the_string()
    {
        var messageTemplate = $"The entered age of {Token.VALIDATED_VALUE} for the {Token.DISPLAY_NAME} input is less than the required age of {Token.COMPARE_TO_VALUE}";

        var failureMessage = FailureMessages.FormatCompareValueMessage(messageTemplate, "13", "Childs age","16");

        failureMessage.Should().Be($"The entered age of 13 for the Childs age input is less than the required age of 16");
    }


    [Fact]
    public void Format_Rolling_date_message_should_replace_the_value_to_validate_and_display_name_and_min_max_today_dates_tokens_if_found_in_the_string()
    {
        var today           = DateOnly.FromDateTime(new DateTime(2000, 06, 15));
        var minDate         = today.AddDays(-100);
        var maxDate         = today.AddDays(100);
        var valueToValidate = today.AddDays(110);

        var messageTemplate = $"The date of {Token.VALIDATED_VALUE} was not between the dates {Token.MIN_DATE} and {Token.MAX_DATE} for Registration based on the date of {Token.TODAY}";

        var failureMessage = FailureMessages.FormatRollingDateMessage(messageTemplate, valueToValidate.ToString("O"), "Registration",minDate.ToString("O"),maxDate.ToString("O"), today.ToString("O"));

        failureMessage.Should().Be($"The date of {valueToValidate:O} was not between the dates {minDate:O} and {maxDate:O} for Registration based on the date of {today:O}");
    }


    [Fact]
    public void Format_decimal_precision_scale_message_format_should_replace_the_value_to_validate_and_display_name_and_all_provided_precision_and_scale_tokens()
    {
        var maxPrecision    = "10";
        var maxScale        = "5";
        var actualPrecision = "12";
        var actualScale     = "6";
        var valueToValidate = "123456.123456";

        var messageTemplate = $"The value: {Token.VALIDATED_VALUE} for: {Token.DISPLAY_NAME} did not meet the precision and scale requirements. " +
                              $"The maximum precision and scale set were {Token.MAX_PRECISION} and {Token.MAX_SCALE} but found the actual precision and scale of {Token.ACTUAL_PRECISION} and {Token.ACTUAL_SCALE}";

        var failureMessage = FailureMessages.FormatDecimalPrecisionScaleMessage(messageTemplate, valueToValidate.ToString(), "Amount",maxPrecision,maxScale,actualPrecision,actualScale);

        failureMessage.Should().Be($"The value: 123456.123456 for: Amount did not meet the precision and scale requirements. " +
                                   $"The maximum precision and scale set were 10 and 5 but found the actual precision and scale of 12 and 6");
    }
}
