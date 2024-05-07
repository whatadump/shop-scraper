namespace ShopScraper.Infrastructure.Utils;

using System;
using System.Linq;
using Attributes;

public static class AttributeUtils
{
    public static string GetMemberName<T>(this T enumVal) where T : Enum
    {
        var type = enumVal.GetType();
        var memInfo = type.GetMember(enumVal.ToString());
        var attributes = memInfo[0].GetCustomAttributes(typeof(MemberNameAttribute), false);
        return (attributes.Length > 0) ? ((MemberNameAttribute)attributes[0]).Name : string.Empty;
    }
    
    public static string GetCustomMemberName<T>(this T enumVal, int index = 0) where T : Enum
    {
        var type = enumVal.GetType();
        var memInfo = type.GetMember(enumVal.ToString());
        var attributes = memInfo[0].GetCustomAttributes(typeof(CustomMemberNameAttribute), false).Cast<CustomMemberNameAttribute>();

        return attributes.FirstOrDefault(x => x.Index == index)?.Name ?? string.Empty;
    }
}