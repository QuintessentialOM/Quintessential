using System;

namespace Quintessential;

/// <summary>
/// Writes information to the logs.txt.
/// </summary>
public static class Logger {
	
	public static bool Setup {
		get;
		private set;
	} = false;

	public static void Init() {
		if (!Setup) {
			#pragma warning disable CS0618 // call to deprecated method
			Log("Quintessential legacy logger (deprecated)");
			Setup = true;
		}
	}

	/// <summary>
	/// Log text to the logs file.
	/// </summary>
	/// <param name="text">The text to log.</param>
	[Obsolete("Use [YourMod].Instance.Log instead")]
	public static void Log(string text) {
		Logging.LogManager.LogRaw($"({DateTime.Now}) [old logger] {text ?? "null"}\n");
    }

    /// <summary>
    /// Log text to the logs file.<br/>
	/// Does not log the current time.
    /// </summary>
    /// <param name="text">The text to log.</param>
	[Obsolete("Use [YourMod].Instance.Log instead")]
    public static void LogNoTime(string text) {
		Logging.LogManager.LogRaw($"[old logger] {text ?? "null"}\n");
    }

	/// <summary>
	/// Log an objec to the logs file.
	/// </summary>
	/// <param name="e">The object to log.</param>
	[Obsolete("Use YourMod.Instance.Log instead")]
    public static void Log(object e) {
		Log(e?.ToString());
	}
}
