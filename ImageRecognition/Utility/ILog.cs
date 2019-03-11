namespace ImageRecognition.Utility
{
	public enum LogLevel
	{
		Debug = 0,
		Info = 1,
		Warn = 2,
		Error = 3
	}

	public interface ILog
	{
		void Message(LogLevel level, string message, params object[] formatArgs);
	}

	public static class ILogExtensions
	{
		public static void Debug(this ILog log, string message)
		{
			log.Message(LogLevel.Debug, message);
		}

		public static void Debug(this ILog log, string format, params object[] formatArgs)
		{
			log.Message(LogLevel.Debug, format, formatArgs);
		}

		public static void Info(this ILog log, string message)
		{
			log.Message(LogLevel.Info, message);
		}

		public static void Info(this ILog log, string format, params object[] formatArgs)
		{
			log.Message(LogLevel.Info, format, formatArgs);
		}

		public static void Warn(this ILog log, string message)
		{
			log.Message(LogLevel.Warn, message);
		}

		public static void Warn(this ILog log, string format, params object[] formatArgs)
		{
			log.Message(LogLevel.Warn, format, formatArgs);
		}

		public static void Error(this ILog log, string message)
		{
			log.Message(LogLevel.Error, message);
		}

		public static void Error(this ILog log, string format, params object[] formatArgs)
		{
			log.Message(LogLevel.Error, format, formatArgs);
		}
	}
}
