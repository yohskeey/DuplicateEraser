using System.Text;

namespace DuplicateEraser;

internal class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine(@"usage: DuplicateEraser.exe <textfile> [--separator=<separatoroption>]  [--encoding=<encodingoption>]
  separatoroption:
    l: lf | crlf <default>
    c: comma
  encodingoption:
    utf-8: <default>
    sjis: Shift-JIS
");
            return;
        }
        if (!File.Exists(args[0]))
        {
            Console.WriteLine("File Not Found");
        }
        var filePath = Path.GetFullPath(args[0]);
        bool isComma = false;
        string encode = "";
        foreach (var arg in args)
        {
            if (arg.StartsWith("--separator="))
            {
                isComma = (arg.Split("=")[1] == "c");
            }
            else if (arg.StartsWith("--encoding="))
            {
                encode = arg.Split("=")[1];
            }
        }
        IEnumerable<string> words;

        words = ReadFile(filePath, isComma, encode);

        if (!DuplicateEraser(filePath, words))
        {
            Console.WriteLine("Error.");
            return;
        }
    }

    static bool DuplicateEraser(string filePath, IEnumerable<string> words)
    {
        var removeDuplicateWords = RemoveDuplication(words);
        var duplicateWords = FindDuplication(words);
        var path = Path.GetFileName(filePath);

        if (removeDuplicateWords is null || duplicateWords is null)
        {
            return false;
        }
        using (var outputFile = new StreamWriter($"_RemoveDuplicate_{path}", false))
        {
            foreach (var t in removeDuplicateWords)
            {
                outputFile.WriteLine(t);
            }
        }
        using (var outputFile = new StreamWriter($"_Duplicate_{path}", false))
        {
            foreach (var t in duplicateWords)
            {
                outputFile.WriteLine(t);
            }
        }
        return true;
    }

    /// <summary>
    /// 文字列リストから、重複する要素を除外する。
    /// </summary>
    /// <param name="list">文字列リスト</param>
    /// <returns>重複している要素のリスト</returns>
    public static IEnumerable<string> RemoveDuplication(IEnumerable<string> list) => list.Distinct();

    /// <summary>
    /// 文字列リストから、重複する要素を抽出する。
    /// </summary>
    /// <param name="list">文字列リスト</param>
    /// <returns>重複している要素のリスト</returns>
    public static IEnumerable<string> FindDuplication(IEnumerable<string> list)
    {
        // 要素名でGroupByした後、グループ内の件数が2以上（※重複あり）に絞り込み、
        // 最後にIGrouping.Keyからグループ化に使ったキーを抽出している。
        var duplicates = list.GroupBy(name => name).Where(name => name.Count() > 1)
            .Select(group => group.Key).ToList();

        return duplicates;
    }

    static IEnumerable<string> ReadFile(string filePath, bool isComma, string encode)
    {
        var encoding = Encoding.UTF8;
        if (encode == "sjis")
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            encoding = Encoding.GetEncoding("shift_jis");
        }
        if (isComma)
        {
            var text = File.ReadAllText(filePath, encoding);
            return text.Split(",");
        }
        else
        {
            return File.ReadAllLines(filePath, encoding);
        }
    }
}