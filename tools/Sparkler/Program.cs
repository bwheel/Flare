
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace Sparkler;

[AttributeUsage(AttributeTargets.Field)]
public class EmbeddedResourceNameAttribute : Attribute
{
    public string FilePath => $"Sparkler.data.{m_fileName}";
    private string m_fileName;
    public EmbeddedResourceNameAttribute(string fileName)
    {
        m_fileName = fileName;
    }
}

public enum SourceTypes
{
    [EmbeddedResourceName("python_logs.txt")]
    Python,

    [EmbeddedResourceName("dotnet_logs.txt")]
    DotNet,

    [EmbeddedResourceName("nodejs_logs.txt")]
    NodeJs,

    [EmbeddedResourceName("java_logs.txt")]
    Java,
};

public static class Extentions
{
    public static string ToEmbeddedResourceName(this SourceTypes source)
    {
        var sourceType = source.GetType();
        if (sourceType == null)
            throw new ArgumentNullException(nameof(sourceType));
        var memberInfo = sourceType!.GetMember(source.ToString()).FirstOrDefault();
        if (memberInfo == null)
            throw new ArgumentNullException(nameof(memberInfo));
        var attribute = memberInfo.GetCustomAttribute<EmbeddedResourceNameAttribute>();
        if (attribute == null)
            throw new ArgumentNullException(nameof(attribute));
        if (string.IsNullOrWhiteSpace(attribute.FilePath))
            throw new ArgumentNullException(nameof(attribute.FilePath));
        return attribute.FilePath;
    }
}

class Program
{
    public const string KEY_SOURCE = "SPARKLER_SOURCE";
    public const string KEY_ITERATIONS = "SPARKLER_ITERATIONS";
    public const string KEY_DELAY = "SPARKLER_DELAY";
    public const SourceTypes DEFAULT_SOURCE_TYPE = SourceTypes.Python;
    public const int DEFAULT_ITERATIONS = -1;
    public static readonly TimeSpan DEFAULT_DELAY = TimeSpan.FromMilliseconds(500);

    static string[] readEmbeddedResource(string resourceName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using (Stream stream = assembly.GetManifestResourceStream(resourceName)!)
        using (var reader = new StreamReader(stream))
            return reader.ReadToEnd().Split("\r\n");
    }

    static void Main(string[] args)
    {
        // parse input
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();
        Console.WriteLine(string.Join(',', configuration));
        var rng = new Random(DateTime.UtcNow.Millisecond);

        // load dataset
        string fileName = configuration.GetValue(KEY_SOURCE, DEFAULT_SOURCE_TYPE).ToEmbeddedResourceName();
        string[] dataSet = readEmbeddedResource(fileName);

        // print dataset
        int count = 0;
        while (
            count < configuration.GetValue(KEY_ITERATIONS, DEFAULT_ITERATIONS)
            || 0 > configuration.GetValue(KEY_ITERATIONS, DEFAULT_ITERATIONS)
            )
        {
            int lineNum = rng.Next(0, dataSet.Length - 1);
            string line = dataSet[lineNum];
            Console.Out.WriteLine(line);
            Console.Out.Flush();
            var delay = configuration.GetValue(KEY_DELAY, DEFAULT_DELAY) > TimeSpan.FromMilliseconds(10)
                ? configuration.GetValue(KEY_DELAY, DEFAULT_DELAY)
                : DEFAULT_DELAY;
            Thread.Sleep(delay);
            count++;
        }
    }
}
