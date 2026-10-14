using System;
using System.IO;

namespace Quintessential;

// don't actually know how logging works in OM, but rn it looks like it just doesn't?
// so let's do it ourself
// Note, It does, just not how we would like it

/// <summary>
/// Writes information to the logs.txt.
/// </summary>
public static class Logger {
	
	private static string LogPath;
	public static bool Setup {
		get;
		private set;
	} = false;

	public static void Init() {
		if (!Setup) {
			LogPath = Path.Combine(QuintessentialLoader.PathLightning, "log.txt");
			File.Delete(LogPath);
			Log("Quintessential log");
			Setup = true;
		}
	}

	/// <summary>
	/// Log text to the logs file.
	/// </summary>
	/// <param name="text">The text to log.</param>
	public static void Log(string text) {
		File.AppendAllText(LogPath, $"({DateTime.Now}) {text ?? "null"}\n");
    }

    /// <summary>
    /// Log text to the logs file.<br/>
	/// Does not log the current time.
    /// </summary>
    /// <param name="text">The text to log.</param>
    public static void LogNoTime(string text) {
        File.AppendAllText(LogPath, $"{text ?? "null"}\n");
    }

	/// <summary>
	/// Log an objec to the logs file.
	/// </summary>
	/// <param name="e">The object to log.</param>
    public static void Log(object e) {
		Log(e?.ToString());
	}
}