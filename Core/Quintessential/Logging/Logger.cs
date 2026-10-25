using System;
using System.Globalization;
using System.Text;
using System.Threading;

namespace Quintessential.Logging;

public class Logger {
	private readonly string modId;

	// TODO relies on .cctor being called from the main thread, which should always happen I think?
	private static readonly Thread mainThread = Thread.CurrentThread;

	internal Logger(string modId) {
		this.modId = modId;
	}

	private static string GetTimePrefix() {
		return DateTime.Now.ToString("HH:mm:ss ", CultureInfo.InvariantCulture);
	}

	private static string GetThreadNamePrefix() {
		return Thread.CurrentThread.Name != null ? "[Thread.CurrentThread.Name] " : $"<Thread {Thread.CurrentThread.ManagedThreadId}> ";
	}

	private static string GetLogLevelPrefix(LogLevel logLevel) {
		return logLevel switch {
			LogLevel.Trace => "[TRACE] ",
			LogLevel.Debug => "[DEBUG] ",
			LogLevel.Info => "[INFO] ",
			LogLevel.Warn => "[WARN] ",
			LogLevel.Error => "[ERROR] ",
			LogLevel.Critical => "[CRITICAL] ",
			_ => ""
		};
	}

	private string GetModIdPrefix() {
		return $"[{modId}] ";
	}

	public void Log(LogLevel logLevel, string? message, params object?[] formatArgs) {
		var sb = new StringBuilder();
		sb.Append(GetTimePrefix());
		sb.Append(GetLogLevelPrefix(logLevel));

		if (Thread.CurrentThread != mainThread)
			sb.Append(GetThreadNamePrefix());

		sb.Append(GetModIdPrefix());

		if (message != null) {
			sb.AppendFormat(message, formatArgs);
		} else {
			sb.Append("null");
		}
		sb.Append("\n");

		if (logLevel >= LogLevel.Error) {
			LogManager.LogStderrRaw(sb.ToString());
		} else {
			LogManager.LogRaw(sb.ToString());
		}
	}

	public void Critical(string? message, params object?[] formatArgs) {
		Log(LogLevel.Critical, message, formatArgs);
	}

	public void Error(string? message, params object?[] formatArgs) {
		Log(LogLevel.Error, message, formatArgs);
	}

	public void Warn(string? message, params object?[] formatArgs) {
		Log(LogLevel.Warn, message, formatArgs);
	}

	public void Info(string? message, params object?[] formatArgs) {
		Log(LogLevel.Info, message, formatArgs);
	}

	public void Debug(string? message, params object?[] formatArgs) {
		Log(LogLevel.Debug, message, formatArgs);
	}

	public void Trace(string? message, params object?[] formatArgs) {
		Log(LogLevel.Trace, message, formatArgs);
	}

	public void Log(LogLevel logLevel, object? obj) {
		Log(logLevel, "{0}", obj);
	}

	public void Critical(object? obj) {
		Log(LogLevel.Critical, obj);
	}

	public void Error(object? obj) {
		Log(LogLevel.Error, obj);
	}

	public void Warn(object? obj) {
		Log(LogLevel.Warn, obj);
	}

	public void Info(object? obj) {
		Log(LogLevel.Info, obj);
	}

	public void Debug(object? obj) {
		Log(LogLevel.Debug, obj);
	}

	public void Trace(object? obj) {
		Log(LogLevel.Trace, obj);
	}
}
