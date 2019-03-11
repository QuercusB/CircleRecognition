using System;
using System.Collections;
using System.Collections.Generic;

namespace ImageRecognition.Utility
{
	public class MemoryLogMessage
	{
		public DateTime Timestamp;
		public LogLevel Level;
		public string Message;

		public string SimpleFormatted()
		{
			var levelName = Enum.GetName(typeof(LogLevel), Level);
			return string.Format("{0} [{1}] {2}",
				Timestamp.ToString("HH:mm:ss.fff"),
				levelName,
				Message);
		}
	}

	public class MemoryLogMessageEventArgs : EventArgs
	{
		public MemoryLogMessageEventArgs(MemoryLogMessage message)
		{
			Message = message;
		}

		public MemoryLogMessage Message { get; private set; }
	}

	public delegate void MemoryLogMessageEventHandler(object sender, MemoryLogMessageEventArgs args);

	public class MemoryLog : ILog, IEnumerable<MemoryLogMessage>
	{
		private List<MemoryLogMessage> log = new List<MemoryLogMessage>();

		public event MemoryLogMessageEventHandler MessageAdded = null;

		public void Message(LogLevel level, string message, params object[] formatArgs)
		{
			var logMessage = new MemoryLogMessage() { Timestamp = DateTime.Now, Level = level };
			if (formatArgs?.Length > 0)
				logMessage.Message = string.Format(message, formatArgs);
			else
				logMessage.Message = message;
			log.Add(logMessage);
			MessageAdded?.Invoke(this, new MemoryLogMessageEventArgs(logMessage));
		}

		public IEnumerator<MemoryLogMessage> GetEnumerator()
		{
			return log.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
