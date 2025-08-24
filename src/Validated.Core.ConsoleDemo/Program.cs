using Validated.Core.ConsoleDemo.Examples;

namespace Validated.Core.ConsoleDemo;

internal class Program
{
    /*
        * This demo project just shows the basics but as its referencing the actual project used for the nuget package you can step through any part of the code. 
        * 
        * For more advanced scenarios, standalone solutions referencing will be placed in the Examples folder in the repo, so you can run these separately..
        * 
        * These more advanced scenarios will show techniques to validate domain value objects without violating your protected domain project as well as showing the multi-tenant / semi-dynamic approach (without reflection) 
        * of creating the validators from configuration data.This simple string data can be stored in a database, retrieved at runtime, added to cache and periodically updated with periodic changes.
        * 
        * These more advanced solutions are multi project solutions to replicate more real world scenarios / usage.
        * 
    */ 
    static async Task Main()
    {
        await The_Beginning_Validated_Of_T.Run();

        //await MemberValidator_Building_Block.Run();

        //await Built_In_MemberValidators.Run();

        //await Using_Validation_Builder_Part_1.Run();

        //await Using_Validation_Builder_Part_2.Run();

        //await Using_Validation_Builder_Part_3.Run();

        //await Using_Validation_Builder_Part_4.Run();

        //await Without_The_Builder.Run();

        Console.ReadLine();
    }
}
