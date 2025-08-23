namespace Validated.Core.Tests.SharedDataFixtures.Common.Models;

public class NonComparable
{
    public int Value { get; }
    public NonComparable(int value)

        => Value = value;    
    
    public override bool Equals(object? obj)
    
        =>  obj is NonComparable other && other.Value == this.Value;
    
    public override int GetHashCode() 
        
        => Value.GetHashCode();
}
