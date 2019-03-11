using System;

namespace ImageRecognition.Utility
{
	public class ConsoleLog : ILog
	{
		public void Message(LogLevel level, string message, params object[] args)
		{
			var levelName = Enum.GetName(typeof(LogLevel), level);
			Console.Out.WriteLine("{0} [{1}] {2}",
				DateTime.Now.ToString("HH:mm:ss.fff"),
				levelName,
				(args?.Length > 0) ? string.Format(message, args) : message
			);
		}
	}
}
