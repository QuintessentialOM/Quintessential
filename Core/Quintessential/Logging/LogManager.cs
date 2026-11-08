using System;
using System.Collections.Generic;

namespace Quintessential.Logging;

internal static class LogManager {
	internal static Dictionary<string, Logger> logsByModId = [];

	// Wrapped in case we ever want to do anything other than directly emitting to stdout/stderr (writing to file as well, etc)
	internal static void LogRaw(string str) {
		Console.Write(str);
	}

	internal static void LogStderrRaw(string str) {
		Console.Error.Write(str);
	}

	internal static Logger GetModLogger(string modId) {
		if (!logsByModId.ContainsKey(modId)) {
			logsByModId[modId] = new Logger(modId);
		}
		return logsByModId[modId];
	}
}
