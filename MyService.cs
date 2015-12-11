using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace First_WindowsService
{
    public partial class MyService : ServiceBase
    {
        private int eventid = 0;
        private FileSystemWatcher watcher;
        private string sourcePath;
        private string destPath;
        public MyService(string[] args)
        {
            InitializeComponent();
            string eventSourceName = "MySource";
            string logName = "MyNewLog";
            if (args.Count() > 0)
            {
                eventSourceName = args[0];
            }
            if (args.Count() > 1)
            {
                logName = args[1];
            }
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists(eventSourceName))
            {
                System.Diagnostics.EventLog.CreateEventSource(eventSourceName, logName);
            }
            eventLog1.Source = eventSourceName;
            eventLog1.Log = logName;
        }

        protected override void OnStart(string[] args)
        {
            //System.Diagnostics.Debugger.Launch();
            eventLog1.WriteEntry("In OnStart");
            this.sourcePath = @"C:\Users\dipeeka_2\Downloads";
            this.destPath = @"C:\Users\dipeeka_2\Downloads\Others\";
            watcher = new FileSystemWatcher();
            watcher.Path = this.sourcePath;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName |
                                   NotifyFilters.DirectoryName;
            // watcher.Filter = "*.pdf";
            //    watcher.Created += Watch_Created;
            //  watcher.Changed += Watch_Created;
            watcher.Renamed += Watch_Created;
            watcher.IncludeSubdirectories = false;
            watcher.EnableRaisingEvents = true;
            IList<string> fileList = Directory.GetFiles(sourcePath);
            foreach (var fileName in fileList)
            {
                FileInfo info = new FileInfo(fileName);
                string subPath = sourcePath + @"\" + info.Extension.Substring(1, info.Extension.Length - 1);
                bool exists = Directory.Exists(subPath);
                if (!exists)
                    Directory.CreateDirectory(subPath);
                System.IO.File.Copy(fileName, subPath + @"\" + info.Name);
                File.Delete(fileName);

            }

        }

        public void Watch_Created(object obj, FileSystemEventArgs args)
        {

            FileInfo info = new FileInfo(args.FullPath);
            FileAttributes attr = File.GetAttributes(args.FullPath);

            if (!attr.HasFlag(FileAttributes.Directory))
            {
                string subPath = sourcePath + @"\" + info.Extension.Substring(1, info.Extension.Length - 1);
                bool exists = Directory.Exists(subPath);
                if (!exists)
                    Directory.CreateDirectory(subPath);
                
                destPath = subPath + @"\" + args.Name;
                if (!(info.Extension.Equals(".tmp") || info.Extension.Equals(".crdownload")))
                {
                    if (File.Exists(destPath))
                    {
                        int count = 1;

                        string fileNameOnly = Path.GetFileNameWithoutExtension(destPath);
                        string extension = Path.GetExtension(destPath);
                        string path = Path.GetDirectoryName(destPath);


                        while (File.Exists(destPath))
                        {
                            string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                            destPath = Path.Combine(path, tempFileName + extension);
                        }
                    }
                    File.Copy(args.FullPath, destPath);
                    File.Delete(args.FullPath);
                }
            }
            //else
            //{
            //    Directory.CreateDirectory(info.FullName);

            //}

        }

        private bool IsFileLocked(string filename)
        {

            try
            {
                FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                stream.Dispose();
            }
            catch (Exception e)
            {
                return true;
            }
            return false;
        }
        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {

            eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventid++);
        }
        protected override void OnStop()
        {
            eventLog1.WriteEntry("stop");
        }
    }
}
