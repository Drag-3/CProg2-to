using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using TextStreamReader;

namespace Bank.Logs
{
    public class Logger
    {
        public enum LogPriority
        {
            Info,
            Debug,
            Warning,
            Error
        }

        private readonly string _directoryPath = "C:\\Users\\" + Environment.UserName + "\\Documents\\Logs\\";
        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                _path = _directoryPath + DateTime.Now.Minute + "-" + DateTime.Now.Second+ "-" + value + ".log";
                _stream.Dispose();
                _stream = new TextStream(File.Open(_path, FileMode.Append));
            }
        }

        private string _path;
        private TextStream _stream;


        public Logger(string fileName)
        {
            _path = _directoryPath + DateTime.Now.Minute + "-" + DateTime.Now.Second+ "-" + fileName + ".log";
            
            try
            {
                if (!File.Exists(_path))
                {
                    var dir = Directory.CreateDirectory(_directoryPath);
                    dir.Create();
                }
            }
            catch (Exception e)
            {
                
                Console.WriteLine(e);
                throw;
            }
            _stream = new TextStream(File.Open(_path, FileMode.Append));
        }

        public void Log(LogPriority priority, string message)
        {
            
            var output = new StringBuilder(CreateLogHeader(priority));
            output.Append(message);
            
            _stream.WriteLine(output.ToString());
        }

        private static string CreateLogHeader(LogPriority priority)
        {
            var output = new StringBuilder();
            var priorityString = priority switch
            {
                LogPriority.Info => "INFO",
                LogPriority.Debug => "DEBUG",
                LogPriority.Warning => "WARNING",
                LogPriority.Error => "ERROR",
                _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null),
            };

            output.Append(priorityString);
            output.Append(" - ");
            output.Append(DateTime.Now.TimeOfDay);
            output.Append("");
            return output.ToString();
        }
    }
}