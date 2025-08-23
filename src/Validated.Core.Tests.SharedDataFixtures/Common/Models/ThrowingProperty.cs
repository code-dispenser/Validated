namespace Validated.Core.Tests.SharedDataFixtures.Common.Models;

public class ThrowingProperty
{
    public string BadProperty => throw new InvalidOperationException("Property access failed");
    public string GoodProperty => "Good";
}
