namespace CoolLib
{
    public class Mod
    {
        public string Name;
        public string ShortName;
        public LogLevel LogLevel;

        public Mod(string name, string shortname, LogLevel loglevel) {
            Name = name;
            ShortName = shortname;
            LogLevel = loglevel;
        }
    }
}
