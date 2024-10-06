using System.Reflection;

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
