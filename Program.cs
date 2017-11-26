using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace ProcUpdater
{
    class Program
    {
        static ProcUpdaterSettings _settings;

        static void Main(string[] args)
        {
            try
            {
                _settings = LoadArguments(args);

                if (_settings != null && IsValid(_settings))
                {
                    RunDirectoryScripts(new DirectoryInfo(_settings.StoredProceduresPath));

                    //Setup Watcher
                    if (_settings.FileWatch)
                    {
                        SetupWatcher();
                    }
                    else if (_settings.StayAlive)
                    {
                        while (true)
                        {
                            Console.WriteLine("Waiting for new commands (run or quit)");
                            switch (Console.ReadLine())
                            {
                                case ProcUpdaterSettings.RunKeyword:
                                    RunDirectoryScripts(new DirectoryInfo(_settings.StoredProceduresPath));
                                    break;
                                case ProcUpdaterSettings.ExitKeyword:
                                    return;
                            }
                        }
                    }
                }
                else
                {
                    PrintHelp();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR");
                Console.WriteLine(ex.GetType().Name);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Setups the watcher.
        /// </summary>
        private static void SetupWatcher()
        {
            var watcher = new FileSystemWatcher();
            watcher.Path = _settings.StoredProceduresPath;
            watcher.Filter = "*.sql";
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.IncludeSubdirectories = true;

            watcher.Created += FileWatcher_FileChanged;
            watcher.Changed += FileWatcher_FileChanged;
            watcher.Renamed += FileWatcher_FileChanged;

            watcher.EnableRaisingEvents = true;
            Console.WriteLine("Waiting for changes");
            Console.ReadLine();
        }

        private static void FileWatcher_FileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                RunFile(new FileInfo(e.FullPath));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed processing {e.FullPath}: {ex.Message}");
            }
        }

        /// <summary>
        /// Runs all the produres inside the directory
        /// </summary>
        /// <param name="directoryInfo">Directory where the files are located.</param>
        private static void RunDirectoryScripts(DirectoryInfo directoryInfo)
        {
            foreach (var directory in directoryInfo.GetDirectories())
            {
                RunDirectoryScripts(directory);
            }
            foreach (var file in directoryInfo.GetFiles("*.sql"))
            {
                RunFile(file);
            }
        }

        /// <summary>
        /// Updates the procedure
        /// </summary>
        /// <param name="file">File with the procedure.</param>
        private static void RunFile(FileInfo file)
        {
            if (_settings.Verbose)
            {
                Console.Write($"Processing script: {file.FullName}.....");
            }

            //Get the string for the file
            string script;
            var fileStream = new FileStream(file.FullName, FileMode.Open);
            using (var reader = new StreamReader(fileStream))
            {
                script = reader.ReadToEnd();
            }

            //Check if the string has a proc inside
            if (script.Contains("PROCEDURE"))
            {
                //Curate the procedure

                //We don't support multiple batchs, a.k.a. GO
                //So we remove all the GOs we find
                script = Regex.Replace(script, "^Go[$\\n]*", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                //We replace "CREATE PROCEDURE" for "CREATE OR ALTER PROCEDURE"
                script = Regex.Replace(script, "CREATE *PROCEDURE", "CREATE OR ALTER PROCEDURE", RegexOptions.IgnoreCase);

                //Perform update
                if (RunScript(script))
                {
                    if (_settings.Verbose)
                    {
                        Console.Write("OK\n");
                    }
                }
                else
                {
                    if (_settings.Verbose)
                    {
                        Console.Write("FAILED\n");
                    }
                }
            }
            else
            {
                if (_settings.Verbose)
                {
                    Console.Write("No procedure found\n");
                }
            }
        }

        /// <summary>
        /// Runs the script.
        /// </summary>
        /// <param name="script">Curated script.</param>
        private static bool RunScript(string script)
        {
            SqlConnection conn = null;
            var result = false;

            try
            {
                conn = new SqlConnection(_settings.ConnectionString);
                conn.Open();
                var cmd = new SqlCommand(script, conn);
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.ExecuteNonQuery();
                result = true;
            }
            catch (Exception ex)
            {
                if (_settings.Verbose)
                {
                    Console.Write($"{ex.Message}\t");
                }
            }
            finally
            {
                if (conn?.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                conn?.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Checks that the all the arguments are valid
        /// </summary>
        /// <returns><c>true</c>, if all arguments are valid <c>false</c> otherwise.</returns>
        /// <param name="arguments">User Settings</param>
        private static bool IsValid(ProcUpdaterSettings settings)
        {
            return
                !string.IsNullOrEmpty(settings.ConnectionString) &&
                !string.IsNullOrEmpty(settings.StoredProceduresPath) &&
                new DirectoryInfo(settings.StoredProceduresPath).Exists;
        }

        /// <summary>
        /// Prints help message in case of an errr or missing arguments.
        /// </summary>
        private static void PrintHelp()
        {
            Console.WriteLine("Usage: ProcUpdater.exe [options]\n\n");
            Console.WriteLine("Options");
            Console.WriteLine("-conn:\tConnection String (REQUIRED)");
            Console.WriteLine("-path:\tPath where the stored procedures are located (REQUIRED)");
            Console.WriteLine("-alive:\tKeep the app open");
            Console.WriteLine("-watch:\tWatch changes in the path and auto-updated procedures");
            Console.WriteLine("-verbose:\tVerbose mode");
            Console.WriteLine("\n\nIn-app commands");
            Console.WriteLine("run:\tRuns the procedure update task");
            Console.WriteLine("quit:\tQuits the application");
        }

        /// <summary>
        /// Creates a <see cref="ProcUpdaterSettings"/> based on command line arguments and json config file
        /// </summary>
        /// <returns>ARguments set by the user</returns>
        /// <param name="commandArgs">Command arguments received by the main method</param>
        private static ProcUpdaterSettings LoadArguments(string[] commandArgs)
        {
            try
            {
                //Setup the configuration sources
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .AddCommandLine(commandArgs, new Dictionary<string, string>
                    {
                    {"-conn", ProcUpdaterSettings.ConnectionStringKey},
                    {"-path", ProcUpdaterSettings.StoredProceduresPathKey},
                    {"-alive", ProcUpdaterSettings.StayAliveKey},
                    {"-watch", ProcUpdaterSettings.FileWatchKey},
                    {"-verbose", ProcUpdaterSettings.VerboseKey}
                    });

                var configuration = builder.Build();
                var procArguments = new ProcUpdaterSettings();
                configuration.GetSection("ProcUpdater").Bind(procArguments);

                return procArguments;
            }
            catch (FormatException)
            {
                return null;
            }

        }
    }
}
