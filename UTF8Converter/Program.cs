using System;
using System.IO;
using System.Text;

namespace UTF8Converter
{
  class Program
  {
    /// <summary>
    /// https://stackoverflow.com/q/4520184
    /// </summary>
    /// <param name="srcFile"></param>
    /// <returns></returns>
    public static Encoding GetFileEncoding(string srcFile)
    {
      // *** Use Default of Encoding.Default (Ansi CodePage)
      Encoding enc = Encoding.Default;

      // *** Detect byte order mark if any - otherwise assume default
      byte[] buffer = new byte[5];
      FileStream file = new FileStream(srcFile, FileMode.Open);
      file.Read(buffer, 0, 5);
      file.Close();

      if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
        enc = Encoding.UTF8;
      else if (buffer[0] == 0xfe && buffer[1] == 0xff)
        enc = Encoding.Unicode;
      else if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff)
        enc = Encoding.UTF32;
      else if (buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76)
        enc = Encoding.UTF7;
      else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
        // 1201 unicodeFFFE Unicode (Big-Endian)
        enc = Encoding.GetEncoding(1201);
      else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
        // 1200 utf-16 Unicode
        enc = Encoding.GetEncoding(1200);


      return enc;
    }

    /// <summary>
    /// https://stackoverflow.com/a/7102180
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string ReadFileAsUtf8(string fileName)
    {
      Encoding encoding = Encoding.Default;
      String original = String.Empty;

      using (StreamReader sr = new StreamReader(fileName, Encoding.Default))
      {
        original = sr.ReadToEnd();
        encoding = sr.CurrentEncoding;
        sr.Close();
      }

      if (Equals(encoding, Encoding.UTF8))
        return original;

      byte[] encBytes = encoding.GetBytes(original);
      byte[] utf8Bytes = Encoding.Convert(encoding, Encoding.UTF8, encBytes);
      return Encoding.UTF8.GetString(utf8Bytes);
    }

    static void Main(string[] args)
    {
      if (args.Length == 1)
      {
        int nonUtf = 0;
        int utf8 = 0;
        foreach (var f in new DirectoryInfo($@"{args[0]}").GetFiles("*.cs", SearchOption.AllDirectories))
        {
          try
          {
            if (!Equals(GetFileEncoding(f.FullName), Encoding.UTF8))
            {
              nonUtf++;
              string s = ReadFileAsUtf8(f.FullName);
              File.WriteAllText (f.FullName, s, Encoding.UTF8);
            }
            else
            {
              utf8++;
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

        Console.WriteLine($"Non utf8:{nonUtf}, utf8:{utf8}");
      }
    }
  }
}