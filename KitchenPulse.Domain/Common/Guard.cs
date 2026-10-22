namespace KitchenPulse.Domain.Common;

public static class Guard
{
    public static void EnumDefined<T>(T value, string paraName) where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentException($"Invalid {paraName}.", nameof(paraName));
        }

    }
}
