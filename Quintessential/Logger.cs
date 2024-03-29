﻿using System;
using System.IO;

namespace Quintessential;

// don't actually know how logging works in OM, but rn it looks like it just doesn't?
// so let's do it ourself
public static class Logger {

	private static string LogPath;
	public static bool Setup {
		get;
		private set;
	} = false;

	public static void Init() {
		LogPath = Path.Combine(QuintessentialLoader.PathLightning, "log.txt");
		File.Delete(LogPath);
		Log("Quintessential log");
		Setup = true;
	}

	public static void Log(string text) {
		File.AppendAllText(LogPath, $"({DateTime.Now}) {text ?? "null"}\n");
	}

	public static void Log(object e) {
		Log(e?.ToString());
	}
}