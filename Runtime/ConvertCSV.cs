using System.IO;
using System.Text;

namespace Weariness.Util.CSV
{
    public static partial class ConvertCSV
    {
        public static string EscapeCsv(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            bool requiresEscape = false;

            // 1. 빠른 경로: 아무 특수문자 없음
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == ',' || c == '"' || c == '\n' || c == '\r')
                {
                    requiresEscape = true;
                    break;
                }
            }

            if (!requiresEscape)
                return input;

            // 2. Escape 필요: StringBuilder 재사용
            var sb = new StringBuilder(input.Length + 10); // 넉넉하게 잡기
            sb.Append('"');

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == '"')
                    sb.Append("\"\""); // 이중 따옴표
                else
                    sb.Append(c);
            }

            sb.Append('"');
            return sb.ToString();
            
            // if (input.Contains(",") || input.Contains("\"") || input.Contains("\n"))
            // {
            //     return "\"" + input.Replace("\"", "\"\"") + "\"";
            // }
            //
            // return input;
        }

        // 일단 무조건 utf-8형태로 저장
        public static void WriteCsv(string path, string[] lines)
        {
            // UTF-8 without BOM
            using (var writer = new StreamWriter(path, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                foreach (var line in lines)
                {
                    writer.WriteLine(line);
                }
            }
        }

        public static void WriteCsv(string path, string csv)
        {
            // UTF-8 without BOM
            using (var writer = new StreamWriter(path, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                writer.Write(csv);
            }
        }
    }
}