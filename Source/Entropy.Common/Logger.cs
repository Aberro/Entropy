using Entropy.Common.Mods;
using Entropy.Common.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Entropy.Common;

public class Logger
{
	[Flags]
	public enum LogSeverity
	{
		Debug = 1 << 0,
		Information = 1 << 1,
		Warning = 1 << 2,
		Error = 1 << 3,
		Exception = 1 << 4,
		Fatal = 1 << 5,
		All = Debug | Information | Warning | Error | Exception | Fatal,
	}
	delegate void LogDebugDelegate(string message, bool unity);
	delegate void LogInfoDelegate(string message, bool unity);
	delegate void LogWarningDelegate(string message, bool unity);
	delegate void LogErrorDelegate(string message, bool unity);
	delegate void LogExceptionDelegate(Exception message, bool unity);
	delegate void LogFatalDelegate(string message, bool unity);
	LogDebugDelegate? _logDebugMethod;
	LogInfoDelegate? _logInfoMethod;
	LogWarningDelegate? _logWarningMethod;
	LogErrorDelegate? _logErrorMethod;
	LogExceptionDelegate? _logExceptionMethod;
	LogFatalDelegate? _logFatalMethod;
	private object? _slpLogger;

	[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We don't care about reasons")]
	public static Logger StealLogger(EntropyModBase mod)
	{
		ArgumentNullException.ThrowIfNull(mod);
		try
		{
			var assembly = mod.GetType().Assembly;
			// "Silently" try to steal LoadedMod's Logger from SLP without referencing it - Tom mad when me reference SLP and Tom shows angry warning. So me smart me not reference!
			// Anyway, if it fails - we fallback to a regular Logger that prints into Unity.
			var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(asm => asm.GetName().Name == "StationeersLaunchPad");
			var loadedModType = asm?.GetType("StationeersLaunchPad.Loading.LoadedMod");
			var loggerField = loadedModType?.GetField("Logger", BindingFlags.Public | BindingFlags.Instance);
			var modLoaderType = asm?.GetType("StationeersLaunchPad.Loading.ModLoader");
			var fields = modLoaderType?.GetFields(BindingFlags.NonPublic | BindingFlags.Static);
			var loadedModsField = modLoaderType?.GetField("AssemblyToMod", BindingFlags.NonPublic | BindingFlags.Static);
			var loadedMods = loadedModsField?.GetValue(null); // should be Dictionary<Assembly, LoadedMod>
			var tryGetValueMethod = loadedMods?.GetType().GetMethod("TryGetValue");
			object?[] parameters = [assembly, null];
			var result = tryGetValueMethod?.Invoke(loadedMods, parameters);
			if (result is bool b)
			{
				var loadedMod = parameters[1];
				if (loadedMod is not null)
				{
					var logger = loggerField?.GetValue(loadedMod);
					if (logger is not null)
						return new Logger(logger);
				}
			}
		} catch { }
		UnityEngine.Debug.Log("Couldn't steal SLP Logger, fallback, fallback! While Tom isn't looking!");
		return new Logger();
	}

	[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We don't care about reasons")]
	private Logger(object? slpLogger = null)
	{
		var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(asm => asm.GetName().Name == "StationeersLaunchPad");
		var loggerType = asm?.GetType("StationeersLaunchPad.Logger");
		var methods = loggerType?.GetMethods(BindingFlags.Public | BindingFlags.Instance);
		_slpLogger = slpLogger;
		try
		{
			var logDebugMethod = methods.FirstOrDefault(mi => mi.Name == "LogDebug");
			_logDebugMethod = logDebugMethod is not null ? (LogDebugDelegate) Delegate.CreateDelegate(typeof(LogDebugDelegate), slpLogger, logDebugMethod) : null;
		} catch { }
		try
		{
			var logInfoMethod = methods.FirstOrDefault(mi => mi.Name == "LogInfo");
			_logInfoMethod = logInfoMethod is not null ? (LogInfoDelegate) Delegate.CreateDelegate(typeof(LogInfoDelegate), slpLogger, logInfoMethod) : null;
		} catch { }
		try
		{
			var logWarningMethod = methods.FirstOrDefault(mi => mi.Name == "LogWarning");
			_logWarningMethod = logWarningMethod is not null ? (LogWarningDelegate) Delegate.CreateDelegate(typeof(LogWarningDelegate), slpLogger, logWarningMethod) : null;
		} catch { }
		try
		{
			var logErrorMethod = methods.FirstOrDefault(mi => mi.Name == "LogError");
			_logErrorMethod = logErrorMethod is not null ? (LogErrorDelegate) Delegate.CreateDelegate(typeof(LogErrorDelegate), slpLogger, logErrorMethod) : null;
		} catch { }
		try
		{
			var logExceptionMethod = methods.FirstOrDefault(mi => mi.Name == "LogException");
			_logExceptionMethod = logExceptionMethod is not null ? (LogExceptionDelegate) Delegate.CreateDelegate(typeof(LogExceptionDelegate), slpLogger, logExceptionMethod) : null;
		} catch { }
		try
		{
			var logFatalMethod = methods.FirstOrDefault(mi => mi.Name == "LogFatal");
			_logFatalMethod = logFatalMethod is not null ? (LogFatalDelegate) Delegate.CreateDelegate(typeof(LogFatalDelegate), slpLogger, logFatalMethod) : null;
		} catch { }
	}
	public Logger()
	{
	}
	public void LogDebug(string message, bool unity = true)
	{
		if (_logDebugMethod is not null && _slpLogger is not null)
			_logDebugMethod(message, unity);
		else if (unity)
			UnityEngine.Debug.Log($"[DEBUG] {message}");
		else
			Console.WriteLine($"[DEBUG] {message}");
	}
	public void LogInfo(string message, bool unity = true)
	{
		if (_logInfoMethod is not null && _slpLogger is not null)
			_logInfoMethod(message, unity);
		else if (unity)
			UnityEngine.Debug.Log($"[INFO] {message}");
		else
			Console.WriteLine($"[INFO] {message}");
	}
	public void LogWarning(string message, bool unity = true)
	{
		if (_logWarningMethod is not null && _slpLogger is not null)
			_logWarningMethod(message, unity);
		else if (unity)
			UnityEngine.Debug.LogWarning($"[WARNING] {message}");
		else
			Console.WriteLine($"[WARNING] {message}");
	}
	public void LogError(string message, bool unity = true)
	{
		if (_logErrorMethod is not null && _slpLogger is not null)
			_logErrorMethod(message, unity);
		else if (unity)
			UnityEngine.Debug.LogError($"[ERROR] {message}");
		else
			Console.WriteLine($"[ERROR] {message}");
	}
	public void LogException(Exception exception, bool unity = true)
	{
		if (_logExceptionMethod is not null && _slpLogger is not null)
			_logExceptionMethod(exception, unity);
		else if (unity)
			UnityEngine.Debug.LogException(exception);
		else
			Console.WriteLine($"[EXCEPTION] {exception}");
	}
	public void LogFatal(string message, bool unity = true)
	{
		if (_logFatalMethod is not null && _slpLogger is not null)
			_logFatalMethod(message, unity);
		else if (unity)
			UnityEngine.Debug.LogError($"[FATAL] {message}");
		else
			Console.WriteLine($"[FATAL] {message}");
	}
}