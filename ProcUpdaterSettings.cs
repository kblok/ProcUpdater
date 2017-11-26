using System;

namespace ProcUpdater
{
    public class ProcUpdaterSettings
    {
        public const string SectionKey = "ProcUpdater";
        public const string ConnectionStringKey = "ProcUpdater:ConnectionString";
        public const string StoredProceduresPathKey = "ProcUpdater:StoredProceduresPath";
        public const string FileWatchKey = "ProcUpdater:FileWatch";
        public const string StayAliveKey = "ProcUpdater:StayAlive";
        public const string VerboseKey = "ProcUpdater:Verbose";

        public const string ExitKeyword = "quit";
        public const string RunKeyword = "run";

        public string ConnectionString { get; set; }
        public string StoredProceduresPath { get; set; }
        public bool FileWatch { get; set; }
        public bool StayAlive { get; set; }
        public bool Verbose { get; set; }
    }
}
