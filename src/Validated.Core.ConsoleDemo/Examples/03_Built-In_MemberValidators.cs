using Validated.Core.Extensions;
using Validated.Core.Validators;

namespace Validated.Core.ConsoleDemo.Examples;

public static class Built_In_MemberValidators
{
    /*
        * Use the docs to get the full list or just dot to see them all in code. 
        *
        * Regex is the one I use most in apps so I will show that and how we can combine it with another - there is no fail fast everything is evaluated.
        * 
        * I will use a pattern I use a lot for names of things, that stops double spaces, apostrophes and stuff, it also has a length check.
        * @"^(?=.{2,50}$)[A-Z]+['\- ]?[A-Za-z]*['\- ]?[A-Za-z]+$"
    */
    public static async Task Run()
    {
        var pattern         = @"^(?=.{2,50}$)[A-Z]+['\- ]?[A-Za-z]*['\- ]?[A-Za-z]+$";
        var failureMessage = "Must be between 2 and 50 characters, start with a capital and not contain double spaces, apostrophes or dashes";
        /*
            * As the built-in ones assume you will be validating object properties is asks for property names (for dev's) and a display name (for end users)
            * Rather than just use an empty string, lets pretend this is for some persons first name (no matter how controversial)
        */
        var regexNameValidator = MemberValidators.CreateStringRegexValidator(pattern, "FirstName", "First name", failureMessage);

        var validatedName = await regexNameValidator("S");

        await Console.Out.WriteLineAsync($"Is name valid {validatedName.IsValid} - Failures {String.Join("\r\n",validatedName.Failures.Select(f => f))}\r\n");
        /*
            * It failed the length check. How about we remove it and combine it with a string length validator for two separate messages 
        */ 

        var amendedPattern = @"^[A-Z]+['\- ]?[A-Za-z]*['\- ]?[A-Za-z]+$";

        var nameValidator = MemberValidators.CreateStringRegexValidator(amendedPattern, "FirstName", "First name", "Must start with a capital and not contain double spaces, apostrophes or dashes")
                                .AndThen(MemberValidators.CreateStringLengthValidator(2, 50, "FirstName", "First name", "Must be between 2 and 50 characters in length"));

        validatedName = await nameValidator("S");

        await Console.Out.WriteLineAsync($"Is name valid {validatedName.IsValid} - Failures {String.Join("\r\n", validatedName.Failures.Select(f => f))}\r\n");
        /*
            * On screen you would group these. As we have multiple validators/messages, only the invalid messages will be displayed, both in this case 
        */
        if (validatedName.IsInvalid)
        {
            validatedName.Failures.GroupBy(key => key.DisplayName).ToList().ForEach(async group =>
            {
                await Console.Out.WriteLineAsync($"{group.Key}:");
                group.ToList().ForEach(failure => Console.WriteLine(failure.FailureMessage));
            });
        }

        /*
            * Reminder, you can use the validators for what ever you want! 
        */ 
    }
}
