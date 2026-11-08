using MonoMod;
using Quintessential.Internal;
using Quintessential.Logging;
using System;
using System.Linq;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

[MonoModPatch("AppInit")]
class patch_AppInit {
    public static extern void orig_Main(string[] args);

    public static void Main(string[] args) {
        Type appConsts = typeof(AppConsts);
        if (appConsts != null) {
            if (args.Contains("-devEnv")) {
                appConsts.GetField("isDevEnv").SetValue(null, true);
                appConsts.GetField("logSimulationState").SetValue(null, true);
            }
            if (args.Contains("-openGameLogWindow")) {
                appConsts.GetField("showLogWindow").SetValue(null, true);
            }
            if (args.Contains("-pseudoLang")) {
                appConsts.GetField("allowPseudo").SetValue(null, true);
            }
            if (args.Contains("-unsafeExport")) {
                LegacyPorting.UnsafeLegacyExport = true;
            }
            foreach (var arg in args) {
                if (arg.StartsWith("-logLevel=") && Enum.TryParse<LogLevel>(arg.Split('=', 2)[1], true, out var logLevel)) {
                    Logger.defaultMinimumLogLevel = logLevel;
                }
            }
        }

        orig_Main(args);
    }
}
