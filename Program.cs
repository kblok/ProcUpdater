using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ProcUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var arguments = LoadArguments(args);

                if (arguments != null && IsValid(arguments))
                {

                }
                else
                {
                    PrintHelp();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("ERROR");
                Console.WriteLine(ex.GetType().Name);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
            }
        }

        private static bool IsValid(ProcUpdaterSettings arguments)
        {
            
            return 
                !string.IsNullOrEmpty(arguments.ConnectionString) &&
                !string.IsNullOrEmpty(arguments.StoredProceduresPath);
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Usage: ProcUpdater.exe [options]\n\n");
            Console.WriteLine("Options");
            Console.WriteLine("-conn:\tConnection String (REQUIRED)");
            Console.WriteLine("-path:\tPath where the stored procedures are located (REQUIRED)");
            Console.WriteLine("-alive:\tKeep the app open");
            Console.WriteLine("-watch:\tWatch changes in the path and auto-updated procedures");
            Console.WriteLine("\n\nIn-app commands");
            Console.WriteLine("run:\tRuns the procedure update task");
            Console.WriteLine("quit:\tQuits the application");
        }


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
                    {"-watch", ProcUpdaterSettings.FileWatchKey}
                    });

                var configuration = builder.Build();
                var procArguments = new ProcUpdaterSettings();
                configuration.GetSection("ProcUpdater").Bind(procArguments);

                return procArguments;
            }
            catch(FormatException)
            {
                return null;
            }

        }
    }
}
