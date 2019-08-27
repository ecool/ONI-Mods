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
            LogInfo($"{modname} - #{shortname} initialized with log level {loglevel.ToString()}.");
        }

        public static void LogInit(string shortname, LogLevel loglevel)
        {
            string modname = GetModName();
            _mods.Add(modname, new Mod(modname, shortname, loglevel));
            LogInfo($"{modname} initialized set to [{loglevel.ToString()}].");
        }

        public static void WriteLog(string msg, LogLevel loglevel)
        {
            if (_mods.TryGetValue(GetModName(), out Mod mod))
            {
                if (CheckLevel(mod, loglevel)) {
                    Debug.Log($"{mod.LogLevel} == {loglevel}");
                    Console.WriteLine($"{Timestamp()} <<CoolLib>> #{mod.ShortName} @{CallingClassName()}: [{loglevel.ToString()}] {msg}");
                }
            }
        }

        public static void LogDebug(string msg) => WriteLog(msg, LogLevel.DEBUG);
        public static void LogInfo(string msg) => WriteLog(msg, LogLevel.INFO);
        public static void LogWarn(string msg) => WriteLog(msg, LogLevel.WARN);

        public static string CallingClassName()
        {
            var methodInfo = new StackTrace().GetFrame(3).GetMethod();
            return methodInfo.ReflectedType.Name;
        }

        public static bool CheckLevel(Mod mod, LogLevel loglevel) => mod.LogLevel >= loglevel;

        private static string GetModName()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.ProductName;
        }
        // SEE: CaiLib.Logger.Logger
        private static string Timestamp() => System.DateTime.UtcNow.ToString("[HH:mm:ss.fff]");
    }

    public enum LogLevel{
        WARN,
        INFO,
        DEBUG
    }
}
