using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System;

namespace CoolLib {
    public static class Log
    {
        private static readonly Dictionary<string, Mod> _mods = new Dictionary<string, Mod>();

        public static void LogInit(string shortname)
        {
            LogLevel loglevel = LogLevel.INFO;
            #if DEBUG
            loglevel = LogLevel.DEBUG;
            #endif
            string modname = GetModName();
            _mods.Add(modname, new Mod(modname, shortname, loglevel));
            LogInfo($"{modname} v{GetModVersion()} - #{shortname} initialized with log level {loglevel.ToString()}");
        }

        public static void LogInit(string shortname, LogLevel loglevel)
        {
            string modname = GetModName();
            _mods.Add(modname, new Mod(modname, shortname, loglevel));
            LogInfo($"{modname} v{GetModVersion()} - #{shortname} initialized with log level {loglevel.ToString()}");
        }

        public static void WriteLog(string msg, LogLevel loglevel)
        {
            if (_mods.TryGetValue(GetModName(), out Mod mod))
            {
                if (CheckLevel(mod, loglevel)) { // Only do output if the LogLevel is right
                    Console.WriteLine($"{Timestamp()} <<CoolLib>> #{mod.ShortName} @{CallingClassName()}: [{loglevel.ToString()}] {msg}");
                }
            }
        }

        // Log Methods
        public static void LogDebug(string msg) => WriteLog(msg, LogLevel.DEBUG);
        public static void LogInfo(string msg) => WriteLog(msg, LogLevel.INFO);
        public static void LogWarn(string msg) => WriteLog(msg, LogLevel.WARN);

        public static bool CheckLevel(Mod mod, LogLevel loglevel) => mod.LogLevel >= loglevel;

        // Extra Info
        public static string CallingClassName() => new StackTrace().GetFrame(4).GetMethod().ReflectedType.Name; // BUG: Might be causing issues with MacOS
        private static string GetModVersion() => GetInfo().FileVersion;
        private static string GetModName() => GetInfo().ProductName;
        private static FileVersionInfo GetInfo() => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

        // SEE: CaiLib.Logger.Logger
        private static string Timestamp() => System.DateTime.UtcNow.ToString("[HH:mm:ss.fff]");
    }

    public enum LogLevel{
        WARN,
        INFO,
        DEBUG
    }
}
