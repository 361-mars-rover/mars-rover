/*
JIKAEL + DOV
Defines an enum representing the current strategy for updating the rover physics.
Also defined extensions to the enum to get descriptions via GetDescription(mode)
which are used to fill out menu info.
*/

public enum AIMode
{
    CircleAI,
    SunlightAI,
    Manual
}

public static class AIModeExtensions
{
    public static string GetDescription(this AIMode mode)
    {
        switch (mode)
        {
            case AIMode.CircleAI:
                return "Searches in a circular pattern.";
            case AIMode.SunlightAI:
                return "Reacts to sunlight changes.";
            case AIMode.Manual:
                return "Drive the rover manually.";
            default:
                return "Unknown AI mode.";
        }
    }
}