using System;
using System.IO;
using System.Text;

namespace UTF8Converter
{
  class Program
  {
    /// <summary>
    /// https://stackoverflow.com/a/29138903
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    private static string GetEncoding(string filename)
    {
      using (var fs = File.OpenRead(filename))
      {
        var cdet = new Ude.CharsetDetector();
        cdet.Feed(fs);
        cdet.DataEnd();
        if (cdet.Charset != null)
          Console.WriteLine("Charset: {0}, confidence: {1} : " + filename, cdet.Charset, cdet.Confidence);
        else
          Console.WriteLine("Detection failed: " + filename);
        return cdet.Charset;
      }
    }

    static void Main(string[] args)
    {

      if (args.Length == 1)
      {
        foreach (var f in new DirectoryInfo($@"{args[0]}").GetFiles("*.cs", SearchOption.AllDirectories))
        {
          try
          {
            var fileEnc = GetEncoding(f.FullName);
            if (fileEnc != null && !string.Equals(fileEnc, Encoding.GetEncoding(65001).EncodingName, StringComparison.OrdinalIgnoreCase))
            {
              var str = File.ReadAllText(f.FullName, Encoding.GetEncoding(fileEnc));
              File.WriteAllText(f.FullName, str, Encoding.GetEncoding(65001));
            }
          }
          catch (Exception e)
          {
            Console.WriteLine(e);
            var processes = FileUtil.WhoIsLocking(f.FullName);
            foreach (var process in processes)
            {
              Console.WriteLine($"---Blocking process: {process.ProcessName} will be killed---");
              process.Kill();
            }
          }
        }
      }
      else
      {
        Console.WriteLine("Pass root directory of your project you would like to convert as a parameter");
      }
    }
  }
}