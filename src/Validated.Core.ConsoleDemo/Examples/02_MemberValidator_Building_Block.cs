using Validated.Core.Types;

namespace Validated.Core.ConsoleDemo.Examples;

public static class MemberValidator_Building_Block
{
    /*
        * MemberValidator is the building block, everything is built from this simple delegate. Most of its params you can just ignore until you need to build
        * custom validators where you may or may not need to use them.
    */ 
    public static async Task Run()
    {
        await Console.Out.WriteLineAsync("The MemberValidator<T> delegate requires a Value (that will be validated) and an optional path and compare To value, and an optional \r\n" +
                                         "cancellation token- ignore these for now. It returns a Task<Validated<T>> - everything is async\r\n." +
                                         "The built in validators arn't actually async but as you can provide your own validators which might be async everything uses Task \r\n" +
                                         "to make life easier when combining validators together to form new validators.\r\n");

        await Console.Out.WriteLineAsync("Lets build our own simple validator (its just a function, well function factory) thar matches the delegate signature. This is exactly how " +
                                        "the built in ones are built for reference.\r\n" +
                                        "Well look at some of the built in ones later that you call to get your own specific validator\r\n");

        await Console.Out.WriteLineAsync("We call our CreateHelloWorldValidator to get our custom validator that has the passed in failure message baked into it.\r\n");

        var validator = CreateHelloWorldValidator("Must match World");

        await Console.Out.WriteLineAsync("We now have our newly created validator lets execute it using the value of 'World' and print the results\r\n");

        var firstValidated = await validator("World");

        Console.Out.WriteLine($"Is the result valid: {firstValidated.IsValid}. Calling GetValueOr with 'Default Value' outputs {firstValidated.GetValueOr("Default Value")}\r\n");

        Console.Out.WriteLine($"Lets do that again but this time providing the value of 'John'");

        var secondValidated = await validator("John");

        Console.Out.WriteLine($"Is the result valid: {secondValidated.IsValid} - Failures: {String.Join("\r\n",secondValidated.Failures.Select(f => f))}");

        Console.Out.WriteLine("One important thing to remember is that you can validate any value you like, it doesn't have to be some property on an object.\r\n" +
                              "Any validator you create can be used for multiple purposes and/or combined with others to create s single validator that can validate\r\n" +
                              "entire object graphs as shown later");

    }

    /*
        * This method is a function factory, it builds and returns a new function that matches the signature expected by the MemberValidator delegate. 
        * The returned function still expects a value to validate (and optional params, which we discarded) but it now has he validator logic,
        * and supplied failureMessage via closure captures built into it.
        * 
        * I like to put these factory functions in a project that's shared so you can use them in any part of your application. 
        * You can also create static/non static wrapper classes that have methods to call these factory functions with these classes providing the failures messages.
        * The choice is yours on how you compose things.
      */
    public static MemberValidator<string> CreateHelloWorldValidator(string failureMessage)

        => (valueToValidate, _, _, _) => // the delegate needs a value, we can just discard the other optional params if not needed (path, compareTo, cancellationToken)  
        {
            var isValid = valueToValidate == "World";
            /*
                * Don't forget the delegate MemberValidator returns a Task<Validated<T>> so as we have no async stuff in here to await we just use Task.FromResult 
            */ 
            return isValid ? Task.FromResult(Validated<string>.Valid(valueToValidate)) 
                           : Task.FromResult(Validated<string>.Invalid(new InvalidEntry(failureMessage)));//Don't forget MemberValidator returns a Task<Validated<T>> 
        };   
        
}
