using Validated.Core.Types;

namespace Validated.Core.ConsoleDemo.Examples;

public static class The_Beginning_Validated_Of_T
{
    /*
        * Every validator returns a Validated<T> that is either Valid or Invalid. If valid it will contain the validated value, if invalid it will contain a list of one or more failures - List<InvalidEntry>
        * 
        * Validated<T> is a type structure (container) used for many purposes in this instance is the return type of every validator/validation.
        * The T in Validated could be a simple primitive or a complete object graph.
    
        * For those familiar with using Return Types (my own library is called Flow) I would still use your return type after the validation i.e get the Validated<T> and then create a little
        * extension method to transform its contents to your result type, but that's just the way I (the author) like to do things. 
     
        * For those new to result types don't fret about how you get things out of these types of containers. After working with them you will wonder what all the fuss was about
     */


    public static async Task Run()
    {

        await Console.Out.WriteLineAsync("Call our Hello method using the input 'John' and get back a Validated." + "\r\n");

        var firstValidated = Hello("John");

        await Console.Out.WriteLineAsync($"Is the result valid: {firstValidated.IsValid} - failures: {String.Join("\r\n", firstValidated.Failures.Select(f => f))} \r\n");

        await Console.Out.WriteLineAsync("Lets try that again using the input 'World'." + "\r\n");
        
        var secondValidated = Hello("World");

        await Console.Out.WriteLineAsync($"Is the result valid {secondValidated.IsValid} - failures: {secondValidated.Failures.Count}\r\n");

        await Console.Out.WriteLineAsync("There are two ways to get valid values out of Validated. The Match method that requires two functions, one for the \r\n" +
                                         "the happy path and one for the sad path, only one of which will be executed or by using GetValueOr method.\r\n" +
                                         "The GetValueOr method will either return the valid value or just return the fallback value that you provide.\r\n" +
                                         "Lets call this method with a fallback value of 'Some Default' and see what we get back.\r\n");

        await Console.Out.WriteLineAsync($"The value returned is: {secondValidated.GetValueOr("Some Default")}\r\n");

        await Console.Out.WriteLineAsync($"Lets use the Match method, we need to supply two functions one for the happy path and one for the sad path " +
                                         $"I will use the following: secondValidated.Match(failure => \"Some Default\", success => success);\r\n");

        await Console.Out.WriteLineAsync($"The value returned is: {secondValidated.Match(failure => "Some Default", success => success)}");
    }

    private static Validated<string> Hello(string whatShouldThisBe)

        => whatShouldThisBe == "World" ? Validated<string>.Valid(whatShouldThisBe + " is correct") : Validated<string>.Invalid(new InvalidEntry("Expected 'World' to be entered"));

}
