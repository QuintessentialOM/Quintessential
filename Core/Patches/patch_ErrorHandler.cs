using MonoMod;
using Quintessential;
using Quintessential.Internal;
using SDL2;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

[MonoModPatch("ErrorHandler")]
class patch_ErrorHandler {
	
	[MonoModReplace]
	public static void BindHandler() {
		AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
            var sb = new StringBuilder();
			sb.AppendLine("Encountered an error!");
			Exception e = args.ExceptionObject as Exception;
            sb.AppendLine("");
            sb.AppendLine(" -<>- Runtime information: ");
            sb.AppendLine(" Working directory   : " + Directory.GetCurrentDirectory());
            sb.AppendLine(" Process architecture: " + RuntimeInformation.ProcessArchitecture);
            sb.AppendLine(" Os      architecture: " + RuntimeInformation.OSArchitecture);
			sb.AppendLine(" Runtime identifier  : " + RuntimeInformation.RuntimeIdentifier);
            sb.AppendLine(" Runtime directory   : " + RuntimeEnvironment.GetRuntimeDirectory());
            sb.AppendLine(" Runtime version     : " + RuntimeEnvironment.GetSystemVersion());
            sb.AppendLine(" OS      version     : " + Environment.OSVersion);
            sb.AppendLine("");
            sb.AppendLine(" -<>- Exception:");
            sb.AppendLine(StackTraceHelper.FormatExceptionStackTrace(e));
            sb.AppendLine("");
            sb.AppendLine(" -<>- SDL Error:");
            sb.AppendLine(SDL.SDL_GetError());

            QuintessentialCore.Instance.Log.Error(sb.ToString());
        };
	}

    // Code from:
    // https://blog.ndepend.com/include-il-offset-into-production-exception-stack-traces/
    public static class StackTraceHelper {
        public static string FormatExceptionStackTrace(Exception ex) {
            return FormatWithILOffset(ex); // Get IL OffSet in RELEASE config
        }
        private static string FormatWithILOffset(Exception ex) {
            try {
                //Get offset:
                var stackTrace = new StackTrace(ex, true);
                StackFrame[] stackFrames = stackTrace.GetFrames();
                if (stackFrames == null || stackFrames.Length == 0) { return ex.StackTrace.ToString(); }

                var sb = new StringBuilder();
                sb.Append(ex.GetType());
                sb.Append(": ");
                sb.Append(ex.Message);
                sb.Append('\n');
                var stackFramesLength = new int[stackFrames.Length];
                for (var i = 0; i < stackFramesLength.Length; i++) {
                    var stackFrame = stackFrames[i];
                    var method = stackFrame.GetMethod();
                    var parameters = GetMethodParameters(method);
                    var ilOffset = GetILOffset(stackFrame.GetILOffset());
                    var lineInSource = stackFrame.HasSource() ? ((stackFrame.GetFileName() ?? "") + " line " + stackFrame.GetFileLineNumber()) : "";
                    sb.Append($"  at {method.ReflectedType.FullName}.{method.Name}({parameters}) {(string.IsNullOrWhiteSpace(lineInSource) ? ilOffset : lineInSource)} \r\n");
                }
                return sb.ToString();
            } catch (Exception) {
                return ex.StackTrace.ToString();
            }
        }
        private static string GetILOffset(int ilOffset) {
            // Format to hexadecimal to have a standard display of IL instruction OffSet
            var ilOffsetHexString = ilOffset.ToString("X").ToLower();

            // Get a Reflector-like ILOffset like "L_018e"
            var sb = new StringBuilder("L_");
            if (ilOffsetHexString.Length < 4) {
                sb.Append(new string('0', 4 - ilOffsetHexString.Length));
            }
            sb.Append(ilOffsetHexString);
            return sb.ToString();
        }
        private static string GetMethodParameters(MethodBase method) {
            if (!TryGetParameters(method, out ParameterInfo[] parameters, out string failureReason)) return failureReason;

            var length = parameters.Length;
            var sb = new StringBuilder();
            for (var i = 0; i < length; i++) {
                var parameter = parameters[i];
                sb.Append(parameter.ParameterType.Name);
                sb.Append(' ');
                sb.Append(parameter.Name);
                if (i < length - 1) {
                    sb.Append(", ");
                }
            }
            return sb.ToString();
        }
        private static bool TryGetParameters(MethodBase method, out ParameterInfo[] parameters, out string failureReason) {
            try {
                parameters = method.GetParameters();
                failureReason = null;
                return true;
            } catch (FileLoadException ex) {
                failureReason = $@"Exception thrown while calling MethodBase.GetParameters()   Exception Type: {{{ex.GetType()}}}  Exception Message: {{{ex.Message}}}";
                parameters = null;
                return false;
            }
        }
    }
}
