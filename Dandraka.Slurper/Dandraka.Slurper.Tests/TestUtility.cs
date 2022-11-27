using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dandraka.Slurper.Tests;

internal class TestUtility
{
    public string getFile(string fileName)
    {
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "testdata", fileName);
        return File.ReadAllText(path, Encoding.UTF8);
    }

    public string getFileFullPath(string fileName)
    {
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "testdata", fileName);
        return path;
    }

    public async Task<List<string>> getHttpFiles(List<string> urlList)
    {
        var list = new List<string>();
        using (var client = new HttpClient())
        {
            foreach (var url in urlList)
            {
                try
                {
                    var result = await client.GetAsync(url);
                    if (result.IsSuccessStatusCode)
                    {
                        var content = await result.Content.ReadAsByteArrayAsync();
                        list.Add(Encoding.UTF8.GetString(content));
                        Console.WriteLine($"GET HTTP: Read {Math.Round(list[list.Count - 1].Length / (1024m * 1024m), 2)} MB from {url}");
                    }
                    else
                    {
                        Console.WriteLine($"*** WARNING *** GET HTTP: Could not download from {url}, skipping.\r\nResult {result.StatusCode}: {result.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"*** WARNING *** GET HTTP: Could not download from {url}, skipping.\r\nException {ex.GetType().FullName}: {ex.Message}");
                }
            }
        }
        return list;
    }
}
