using Validated.CollectionsRecursion.ConsoleClient.Common.Models;

namespace Validated.CollectionsRecursion.ConsoleClient.Common.Data;

public static class StaticData
{
    public static Node BuildNodeTree(int depthIncludingRoot)
    {
        depthIncludingRoot = depthIncludingRoot < 2 ? 2 : depthIncludingRoot;

        var root    = new Node { Name = "Root" };
        var current = root;

        for (int index = 1; index < depthIncludingRoot; index++)
        {
            var childNode = new Node { Name = $"Child-{index:D2}" };
            current.Child = childNode;
            current       = childNode;
        }

        return root;
    }
    public static ContactDto CreateContactObjectGraph()
    {
        var dob       = new DateOnly(1980, 1, 1);
        var olderDob = new DateOnly(1980, 1, 2);

        var nullableAge = DateTime.Now.Year - dob.Year - (DateTime.Now.DayOfYear < dob.DayOfYear ? 1 : 0);
        var age         = DateTime.Now.Year - dob.Year - (DateTime.Now.DayOfYear < dob.DayOfYear ? 1 : 0);

        AddressDto address = new() { AddressLine = "AddressLine", County = "County", Postcode="PostCode", TownCity="Town" };
        
        List<ContactMethodDto> contactMethods = [new("MethodTypeOne", "MethodValueOne"), new("MethodTypeTwo", "MethodValueTwo")];

        return new() { Address = address, NullableAge = nullableAge, Age = age, ContactMethods = contactMethods, DOB = dob, CompareDOB = olderDob, Email = "john.doe@gmail.com", FamilyName="Doe", GivenName = "John", Mobile="123456789", Title="Mr" };
    }
}
