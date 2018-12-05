using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Jingwood.WindowsFormControl.StyleTable.Common
{
	public enum LogLevel : byte
	{
		All = 0,
		Trace = 1,
		Debug = 2,
		Info = 3,
		Warn = 4,
		Error = 5,
		Fatal = 6,
	}

	public interface ILogWritter
	{
		void Log(string cat, string msg);
		void Shutdown();
	}

	public class Logger
	{
		private static readonly Logger instance = new Logger();
		internal static Logger Instance { get { return instance; } }

		private List<ILogWritter> writters = new List<ILogWritter>();

		public LogLevel CurrentLevel { get; set; }

		private Logger()
		{
			this.CurrentLevel = LogLevel.Info;

			writters.Add(new ConsoleLogWriter());
#if LOG_TO_FILE
			writters.Add(new DebugFileLogger());
#endif
		}

		public static void RegisterWritter(ILogWritter writter)
		{
			instance.writters.Add(writter);
		}

		private bool turnSwitch = true;

		public static void Off()
		{
			instance.turnSwitch = false;
		}

		public static void On()
		{
			instance.turnSwitch = true;
		}

		#region Trace
		public static void Trace(string cat, string format, params object[] args)
		{
			Trace(cat, string.Format(format, args));
		}

		public static void Trace(string cat, string msg)
		{
			instance.WriteLog(LogLevel.Trace, cat, msg);
		}
		#endregion // Trace

		#region Debug

		[Conditional("DEBUG")]
		public static void Debug(string cat, string format, params object[] args)
		{
			Debug(cat, string.Format(format, args));
		}

		[Conditional("DEBUG")]
		public static void Debug(string cat, string msg)
		{
			instance.WriteLog(LogLevel.Info, cat, msg);
		}
		#endregion // Debug

		#region Info
		public static void Log(string cat, string format, params object[] args)
		{
			Log(cat, string.Format(format, args));
		}

		public static void Log(string cat, string msg)
		{
			instance.WriteLog(LogLevel.Info, cat, msg);
		}
		#endregion // Info

		#region Warn
		public static void Warn(string cat, string format, params object[] args)
		{
			Warn(cat, string.Format(format, args));
		}

		public static void Warn(string cat, string msg)
		{
			instance.WriteLog(LogLevel.Trace, cat, msg);
		}
		#endregion // Warn

		#region Error
		public static void Error(string cat, string format, params object[] args)
		{
			Error(cat, string.Format(format, args));
		}

		public static void Error(string cat, string msg)
		{
			instance.WriteLog(LogLevel.Trace, cat, msg);
		}
		#endregion // Trace

		#region Fatal
		public static void Fatal(string cat, string format, params object[] args)
		{
			Fatal(cat, string.Format(format, args));
		}

		public static void Fatal(string cat, string msg)
		{
			instance.WriteLog(LogLevel.Trace, cat, msg);
		}
		#endregion // Fatal

		//private Mutex mutex = new Mutex();

		public void WriteLog(LogLevel level, string cat, string msg)
		{
			if (turnSwitch && level >= CurrentLevel)
			{
				//mutex.WaitOne();

				foreach (var writer in this.writters)
				{
					writer.Log(cat, msg);
				}

				//mutex.ReleaseMutex();
			}
		}

		public void Shutdown()
		{
			foreach (var writer in this.writters)
			{
				writer.Shutdown();
			}
		}
	}

	public class ConsoleLogWriter : ILogWritter
	{
		#region ILogWritter Members

		public void Log(string cat, string msg)
		{
			Console.WriteLine(string.Format("[{0}] {1}: {2}",
				DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), cat, msg));
		}

		public void Shutdown()
		{
		}

		#endregion
	}

	public class FileLogWriter : ILogWritter
	{
		public string LogFile { get; private set; }

		public FileLogWriter(string logFile)
		{
			this.LogFile = logFile;

			var dir = Path.GetDirectoryName(logFile);
			var dirInfo = new DirectoryInfo(dir);
			if (!dirInfo.Exists) dirInfo.Create();

			fs = new FileStream(LogFile, FileMode.Append, FileAccess.Write);
			sw = new StreamWriter(fs);
		}

		private FileStream fs = null;
		private StreamWriter sw = null;

		public void Log(string cat, string msg)
		{
			sw.WriteLine(string.Format("[{0}] {1}: {2}",
					DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), cat, msg));

			sw.Flush();
		}

		public void Shutdown()
		{
			if (sw != null)
			{
				sw.Close();
				fs.Close();
				sw.Dispose();
				fs.Dispose();
			}
		}
	}
}
