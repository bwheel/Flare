
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace Sparkler;

public class Program
{


    static string[] readEmbeddedResource(string resourceName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using (Stream stream = assembly.GetManifestResourceStream(resourceName)!)
        using (var reader = new StreamReader(stream))
            return reader.ReadToEnd().Split("\r\n");
    }

    static void printHelp()
    {
        Console.WriteLine($"{Assembly.GetAssembly(typeof(Program))!.GetName().Name}");
        Console.WriteLine("Generates random log messages");
        foreach (var item in Config.ConfigDefaults)
        {
            Console.WriteLine($"\t--{item.Key}\tDEFAULT:{item.Value}");
        }
    }

    static void Main(string[] args)
    {
        // parse input
        if (args.Select(a => a.ToLower()).Any(a => a == "-h" || a == "--help"))
        {
            printHelp();
            Environment.Exit(0);
        }

        var config = new Config(args);
        var rng = new Random(DateTime.UtcNow.Millisecond);

        // load dataset
        string fileName = config.Source.ToEmbeddedResourceName();
        string[] dataSet = readEmbeddedResource(fileName);

        // print dataset
        int count = 0;
        while (count < config.Iterations || 0 > config.Iterations)
        {
            int lineNum = rng.Next(0, dataSet.Length - 1);
            string line = dataSet[lineNum];
            Console.Out.WriteLine(line);
            Console.Out.Flush();
            Thread.Sleep(config.Delay);
            count++;
        }
    }
}
