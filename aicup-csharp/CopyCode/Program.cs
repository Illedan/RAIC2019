using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CG_CopyCode
{
    class Program
    {
        private const string m_path = @"/Users/erikkvanli/Repos/RAIC2019/aicup-csharp/Strategy",
            m_outputPath = @"/Users/erikkvanli/Repos/RAIC2019/Output.cs";

        static async Task Main()
        {
            var lastUpdated = DateTime.MinValue;
            Console.Error.WriteLine("running?");
            while (true)
            {
                try
                {
                    await Task.Delay(500);
                    var files = GetFiles();
                    var lastEdited = FindLastEdited(files);
                    if (lastEdited == lastUpdated) continue;
                    var mergedFile = CreateMergedCsFile(files.Select(f => File.ReadAllLines(f)).SelectMany(f => f).ToList(), lastEdited);
                    File.WriteAllText(m_outputPath, mergedFile);
                    lastUpdated = lastEdited;
                    Console.WriteLine("Updated: " + lastEdited.ToString("MM/dd/yyyy H:mm \n"));
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                }
            }
        }

        private static string CreateMergedCsFile(List<string> content, DateTime edited)
        {
            var usings = content.Where(c => c.StartsWith("using", StringComparison.Ordinal)).Distinct().ToList();
            content.RemoveAll(c => c.StartsWith("using", StringComparison.Ordinal));

            return string.Join("\n", usings) + "\n\n\n // LastEdited: " + edited.ToString("dd/MM/yyyy H:mm \n\n\n") + string.Join("\n", content);
        }

        private static string[] GetFiles()
        {
            return Directory.GetFiles(m_path, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.ToLower().Contains("/bin/") && !f.ToLower().Contains("/obj/"))
                .ToArray();
        }

        private static DateTime FindLastEdited(string[] filePaths)
        {
            return filePaths.Max(f => File.GetLastWriteTime(f));
        }
    }
}
