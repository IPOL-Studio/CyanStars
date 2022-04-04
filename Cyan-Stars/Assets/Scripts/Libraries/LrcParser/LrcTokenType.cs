namespace CatLrcParser
{
    /// <summary>
    /// Lrc词法单元类型
    /// </summary>
    public enum LrcTokenType
    {
        Eof,
        Number,
        String,
        LeftBracket,  //[
        RightBracket,  //]
        Colon,  //:
        Dot, //.
    }

}

