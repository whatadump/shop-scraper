namespace ShopScraper.Infrastructure.Attributes;

using System;

[AttributeUsage(AttributeTargets.All)]
public class MemberNameAttribute(string name) : Attribute
{
    public string Name { get; private set; } = name;
}