using System;
using System.Diagnostics;
using System.IO;

namespace CTR
{
    // Class representing a .NET project
    public class DotnetProject
    {
        public string Path { get; private set; }
        public string Name { get; private set; }

        // Constructor to initialize the project
        public DotnetProject(string path, string name)
        {
            Path = path;
            Name = name;

            // Ensure the directory exists
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Create the basic .NET console project
            CreateProject();
        }

        // Method to create a new .NET project (Console App by default)
        private void CreateProject()
        {
            // Run the 'dotnet new console' command to create a new console app
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"new classlib -o \"{Path}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(startInfo);
            process.WaitForExit();

            Console.WriteLine($"Project '{Name}' created at {Path}");
        }

        // Method to build the project
        public string Build()
        {
            // Run the 'dotnet build' command
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{Path}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(startInfo);
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Build failed:\n{error}");
            }

            Console.WriteLine($"Project '{Name}' built successfully.");

            // Parse the DLL path from the output
            var dllPath = ParseDllPath(output);
            Console.WriteLine("DLL Path is: "+dllPath);
            return dllPath;
        }

        private string ParseDllPath(string buildOutput)
        {
            var lines = buildOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.Contains("->") && line.Trim().EndsWith(".dll"))
                {
                    var parts = line.Split(new[] { " -> " }, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        return parts[1].Trim();  // Return the DLL path
                    }
                }
            }

            throw new Exception("Failed to locate the generated DLL.");
        }

        // Method to run the project
        public void Run()
        {
            // Run the 'dotnet run' command
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{Path}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(startInfo);
            process.WaitForExit();

            Console.WriteLine($"Project '{Name}' executed successfully.");
        }

        // Method to clean the project (remove bin and obj folders)
        public void Clean()
        {
            // Run the 'dotnet clean' command
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"clean \"{Path}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(startInfo);
            process.WaitForExit();

            Console.WriteLine($"Project '{Name}' cleaned successfully.");
        }
    }

    public class Compiler
    {
        // This method creates a new DotnetProject at the specified path with the given name
        public static DotnetProject MakeDotnetProject(string path, string name)
        {
            return new DotnetProject(path, name);
        }
    }
}

