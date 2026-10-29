using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Quintessential.Logging;

public class AnsiFormat {
	public enum FourBitColor {
		Black = 30,
		Red,
		Green,
		Yellow,
		Blue,
		Magenta,
		Cyan,
		Gray,
		DarkGray = 90,
		BrightRed,
		BrightGreen,
		BrightYellow,
		BrightBlue,
		BrightMagenta,
		BrightCyan,
		White
	}

	private const string escapeCodeStart = "\e[";
	private const string escapeCodeEnd = "m";

	private const string resetStyle = escapeCodeStart + escapeCodeEnd;

	public static string Reset() {
		return resetStyle;
	}

	private List<string> formatStrings = [];

	public AnsiFormat ResetStyle() {
		formatStrings.Add($"0");
		return this;
	}

	public AnsiFormat SwapForegroundBackground() {
		formatStrings.Add("7m");
		return this;
	}

	// TODO 8-bit methods? probably unnecessary?

	public AnsiFormat Foreground4Bit(FourBitColor color) {
		formatStrings.Add($"{((int)color)}");
		return this;
	}

	public AnsiFormat Background4Bit(FourBitColor color) {
		formatStrings.Add($"{((int)color)+10}");
		return this;
	}

	public AnsiFormat Foreground24Bit(int r, int g, int b) {
		formatStrings.Add($"38;2;{r};{g};{b}");
		return this;
	}
	public AnsiFormat Background24Bit(int r, int g, int b) {
		formatStrings.Add($"48;2;{r};{g};{b}");
		return this;
	}

	public override string ToString() {
		return escapeCodeStart + string.Join(";", formatStrings) + escapeCodeEnd;
	}

	/// Substitutes ESC characters with the literal \e, and \e with \\e. Needed because string.Format complains about ESC characters.
	public static string Escape(string raw) {
		return raw.Replace("""\e""", """\\e""").Replace("\e", """\e""");
	}

	// Match instances of literal \e that don't have a second "\" before
	private static Regex escapedRegex = new("""(?<!\\)\\e""");

	// Substitutes \e with ESC characters, and \\e with \e.
	public static string Unescape(string escaped) {
		return escapedRegex.Replace(escaped, "\e").Replace("""\\e""", """\e""");
	}
}
