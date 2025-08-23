using Validated.Core.Builders;
using Validated.Core.Common.Constants;
using Validated.Core.ConsoleDemo.Common.Data;
using Validated.Core.ConsoleDemo.Common.Models;
using Validated.Core.Types;
using Validated.Core.Validators;

namespace Validated.Core.ConsoleDemo.Examples;

public static class Using_Validation_Builder_Part_4
{
    /*
        * You can compare a property to a value or another property of the same type (primitives currently).
        
    */ 
    public static async Task Run()
    {

        var contact = StaticData.CreateContactObjectGraph();

        var compareMemberDOBValidator = MemberValidators.CreateMemberComparisonValidator<ContactDto, DateOnly>
                                        (
                                            c => c.DOB, c => c.CompareDOB, CompareType.GreaterThanOrEqual, "Date of birth", 
                                            $"Must be greater than or equal to: {FailureMessageTokens.COMPARE_TO_VALUE} but found: {FailureMessageTokens.VALIDATED_VALUE}"
                                        );

        var compareDOBToValueValidator = MemberValidators.CreateCompareToValidator<DateOnly>
                                    (
                                        DateOnly.FromDateTime(DateTime.Now), CompareType.EqualTo, "DOB", 
                                        "Date of birth", $"Must be greater than todays date: {FailureMessageTokens.COMPARE_TO_VALUE} but found: {FailureMessageTokens.VALIDATED_VALUE}"
                                    );


        var contactValidator = ValidationBuilder<ContactDto>.Create()
                                    .ForMember(c => c.DOB, compareDOBToValueValidator)
                                        .ForComparisonWithMember(c => c.DOB, compareMemberDOBValidator)
                                            .Build();

        
        await WriteResult(await contactValidator(contact));
    }

    private static async Task WriteResult(Validated<ContactDto> validated)

        => await Console.Out.WriteLineAsync($"Is contact object valid: {validated.IsValid} - Failures: {String.Join("\r\n", validated.Failures.Select(f => f))}\r\n");
}
