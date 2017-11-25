using System;

namespace ProcUpdater
{
    public class ProcUpdaterSettings
    {
        public static string SectionKey => "ProcUpdater";
        public static string ConnectionStringKey => "ProcUpdater:ConnectionString";
        public static string StoredProceduresPathKey => "ProcUpdater:StoredProceduresPath";
        public static string FileWatchKey => "ProcUpdater:FileWatch";
        public static string StayAliveKey => "ProcUpdater:StayAlive";

        public static string ExitKeyword => "quit";
        public static string RunKeyword => "run";

        public string ConnectionString { get; set; }
        public string StoredProceduresPath { get; set; }
        public bool FileWatch { get; set; }
        public bool StayAlive { get; set; }
    }
}
