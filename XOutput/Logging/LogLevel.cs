namespace XOutput.Logging;

/// <summary>
///     Log levels.
/// </summary>
public class LogLevel
{
    /// <summary>
    ///     Trace logger level.
    /// </summary>
    public static readonly LogLevel Trace = new("TRACE", 20);

    /// <summary>
    ///     Debug logger level.
    /// </summary>
    public static readonly LogLevel Debug = new("DEBUG", 40);

    /// <summary>
    ///     Info logger level.
    /// </summary>
    public static readonly LogLevel Info = new("INFO ", 60);

    /// <summary>
    ///     Warning logger level.
    /// </summary>
    public static readonly LogLevel Warning = new("WARN ", 80);

    /// <summary>
    ///     Error logger level.
    /// </summary>
    public static readonly LogLevel Error = new("ERROR", 100);

    protected int level;

    protected string text;

    protected LogLevel(string text, int level)
    {
        this.text = text;
        this.level = level;
    }

    public string Text => text;
    public int Level => level;
}