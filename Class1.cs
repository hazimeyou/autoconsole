public abstract class BasePlugin
{
    public abstract string PluginName { get; }
    public abstract string console(string command);
    public abstract void consoleoutput(string[] args);
    public abstract Task<string> GetOutputAsync(string command, StreamWriter inputWriter);
}
