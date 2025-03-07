
public static class EnumHelper
{
    public static string GetEnumValidValues<T>() where T : Enum
    {
        return string.Join(", ", Enum.GetValues(typeof(T))
                                     .Cast<T>()
                                     .Select(e => $"{Convert.ToInt32(e)}='{e}'"));
    }

    public static bool IsValidEnumValue<T>(int value) where T : Enum
    {
        return Enum.IsDefined(typeof(T), value);
    }
}
