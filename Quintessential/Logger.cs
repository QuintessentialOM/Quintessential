using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quintessential {

	// don't actually know how logging works in OM, but rn it looks like it just doesn't?
	// so let's do it ourself
	class Logger {

		private static string LogPath;

		public static void Init() {
			LogPath = Path.Combine(QuintessentialLoader.PathLightning, "log.txt");
			File.Delete(LogPath);
			File.AppendAllText(LogPath, "Quintessential log");
		}

		public static void Log(string text) {
			File.AppendAllText(LogPath, $"({DateTime.Now}) {text}\n");
		}
	}
}