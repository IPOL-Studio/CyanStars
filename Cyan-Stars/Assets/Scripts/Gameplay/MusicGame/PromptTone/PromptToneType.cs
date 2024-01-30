namespace CyanStars.Gameplay.MusicGame
{
    public enum PromptToneType
    {
        NsKa,
        NsDing,
        NsTambourine,
        None,
    }

    public static class PromptToneTypeExtension
    {
        public static string ToBuiltInPromptToneName(this PromptToneType self) => self switch
        {
            PromptToneType.NsKa => nameof(PromptToneType.NsKa),
            PromptToneType.NsDing => nameof(PromptToneType.NsDing),
            PromptToneType.NsTambourine => nameof(PromptToneType.NsTambourine),
            _ => string.Empty
        };
    }
}
