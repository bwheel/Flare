using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Sparkler;

public class Config
{
  private IConfiguration m_config;
  public const string KEY_SOURCE = "sparkler_source";
  public const string KEY_ITERATIONS = "sparkler_iterations";
  public const string KEY_DELAY = "sparkler_delay";
  public const SourceTypes DEFAULT_SOURCE_TYPE = SourceTypes.Python;
  public const int DEFAULT_ITERATIONS = -1;
  public static readonly TimeSpan DEFAULT_DELAY = TimeSpan.FromMilliseconds(500);

  public static readonly Dictionary<string, string> ConfigDefaults = new Dictionary<string, string>()
    {
        { KEY_SOURCE, DEFAULT_SOURCE_TYPE.ToString()},
        {KEY_ITERATIONS, DEFAULT_ITERATIONS.ToString()},
        {KEY_DELAY, DEFAULT_DELAY.ToString()}
    };

  public SourceTypes Source => m_config.GetValue(KEY_SOURCE, DEFAULT_SOURCE_TYPE);
  public int Iterations => m_config.GetValue(KEY_ITERATIONS, DEFAULT_ITERATIONS);
  public TimeSpan Delay =>
      m_config.GetValue(KEY_DELAY, DEFAULT_DELAY) > TimeSpan.FromMilliseconds(10)
          ? m_config.GetValue(KEY_DELAY, DEFAULT_DELAY)
          : DEFAULT_DELAY;

  public Config(string[] args)
  {
    m_config = parseConfig(args);
  }

  private IConfiguration parseConfig(string[] args)
  {
    IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();
    return configuration;
  }
}