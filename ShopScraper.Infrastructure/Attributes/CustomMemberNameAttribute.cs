namespace ShopScraper.Infrastructure.Attributes;

using System;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class CustomMemberNameAttribute : Attribute
{
    public string Name { get; private set; }
    
    public int Index { get; private set; }

    public CustomMemberNameAttribute(string name, int index = 0)
    {
        Name = name;
        Index = index;
    }
}