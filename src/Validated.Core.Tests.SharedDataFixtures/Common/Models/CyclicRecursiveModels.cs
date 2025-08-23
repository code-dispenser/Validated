namespace Validated.Core.Tests.SharedDataFixtures.Common.Models;
public class Node
{
    public string Name { get; set; } = "";
    public Node? Child { get; set; }
}

public class Parent
{
    public List<Child> Children { get; set; } = [];
    public string Name { get; set; } = default!;
}

public class Child
{
    public string Name   { get; set; } = default!;
    public int    Age    { get; set; } = default!;
    public Parent Parent { get; set; } = default!;
}