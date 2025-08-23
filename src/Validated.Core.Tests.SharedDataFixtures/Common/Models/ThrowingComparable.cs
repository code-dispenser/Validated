namespace Validated.Core.Tests.SharedDataFixtures.Common.Models;

public class ThrowingComparable : IComparable<ThrowingComparable>, IComparable
{
    public int CompareTo(ThrowingComparable? other) => throw new InvalidOperationException("CompareTo failed");

    public int CompareTo(object? obj) => throw new InvalidOperationException("CompareTo failed");
}
