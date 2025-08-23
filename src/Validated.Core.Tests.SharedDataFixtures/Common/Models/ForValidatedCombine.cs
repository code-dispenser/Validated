namespace Validated.Core.Tests.SharedDataFixtures.Common.Models;


public record CombineWithTwoParam(string Name, int Age);
public record CombineWithThreeParam(string Name, int Age, DateOnly DOB); 
public record CombineWithFourParam(string Name, int Age, DateOnly DOB, decimal Total);