using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Weariness.Util.Extensions
{
    public static class StringExtension
    {
        #region Format

        private static readonly Regex FormatToken = new Regex(
            @"(?<!\{)\{(?<idx>\d+)\}(?!\})",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // {key} 형태의 placeholder. key 는 숫자(positional) 또는 단어(named)
        private static readonly Regex FormatPlaceholder = new Regex(
            @"\{(?<key>[^{}:\s]+)\}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // args 에 들어온 "key:value" 형태의 named 인자
        private static readonly Regex NamedArgPattern = new Regex(
            @"^(?<key>[^\s:{}]+):(?<value>.*)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static int GetFormatIndexCount(string token)
        {
            // var formatText = DataManager.Instance.GetText(token);
            string formatText = "";
            if (string.IsNullOrEmpty(formatText)) return 0;
            return FormatToken.Matches(formatText).Count;
        }

        public static bool HasFormatIndex(string token, int findIndex)
        {
            // var formatText = DataManager.Instance.GetText(token);
            string formatText = "";
            if (string.IsNullOrEmpty(formatText)) return false;

            var matches = FormatToken.Matches(formatText);
            foreach (Match match in matches)
            {
                if (int.Parse(match.Groups["idx"].Value) == findIndex)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 확장 Format.
        /// - {N} : args 에서 "key:value" 형식의 named 인자를 제외한 후, 남은 순서대로 0,1,2... 매핑
        /// - {name} : args 중 "name:value" 형식 문자열을 찾아 value 로 치환
        /// - 매칭 실패 시: 원문 placeholder 를 그대로 두고 Debug.LogError 출력
        /// </summary>
        public static string Format(this string text, params object[] args)
        {
            if (string.IsNullOrEmpty(text) || args == null || args.Length == 0)
                return text;

            var positional = new List<object>(args.Length);
            Dictionary<string, string> named = null;

            foreach (var arg in args)
            {
                string s;
                if (arg is string) s = arg as string;
                else s = arg.ToString();
                var namedMatch = NamedArgPattern.Match(s);
                if (namedMatch.Success)
                {
                    named ??= new Dictionary<string, string>();
                    named[namedMatch.Groups["key"].Value] = namedMatch.Groups["value"].Value;
                    continue;
                }
                positional.Add(arg);
            }

            return FormatPlaceholder.Replace(text, match =>
            {
                var key = match.Groups["key"].Value;

                if (int.TryParse(key, out var idx))
                {
                    if (idx >= 0 && idx < positional.Count)
                        return positional[idx]?.ToString() ?? string.Empty;

                    Debug.LogError($"[StringExtension.Format] index {idx} out of range (positional count: {positional.Count}). text: \"{text}\"");
                    return match.Value;
                }

                if (named != null && named.TryGetValue(key, out var value))
                    return value;

                Debug.LogError($"[StringExtension.Format] named key \"{key}\" not found. text: \"{text}\"");
                return match.Value;
            });
        }

        #endregion
        
        #region Text Extension
        
        private const string colorPattern = @"<color=#[0-9a-fA-F]{6}>|</color>";
        private const string colorMatchesPattern = @"<color=#[0-9a-fA-F]{6}>(.*?)</color>";
        private static readonly Regex colorRegex = new Regex(colorPattern, RegexOptions.Compiled);
        private static readonly Regex colorMatchesRegex = new Regex(colorMatchesPattern, RegexOptions.Compiled);
        
        private const string spritePattern =  @"<sprite name=""[^""]*"">";
        private static readonly Regex spriteRegex = new Regex(spritePattern, RegexOptions.Compiled);

        private static string PatternRemove(Match match, string str)
        {
            var sb = new StringBuilder();
            int startIndex = match.Index;
            var lastIndex = match.Index + match.Length;                    
            sb.Append(str, 0, startIndex);                          // 일반 문자 포함
            sb.Append(match.Groups[1].Value);                       // 추출한 값
            sb.Append(str, lastIndex, str.Length - lastIndex);     // 마지막 남은 문자열
            return sb.ToString();
        }
        
        private static string PatternRemoveWhere(MatchCollection matches, string str, Func<string, bool> condition)
        {
            var sb = new StringBuilder();
            int lastIndex = 0;
            foreach (Match match in matches)
            {
                sb.Append(str, lastIndex, match.Index - lastIndex);     // 일반 문자 포함
                if (condition(match.Groups[1].Value))
                    sb.Append(match.Groups[1].Value);                       // 추출한 값
                lastIndex = match.Index + match.Length;                     // 인덱스 갱신
            }
            sb.Append(str, lastIndex, str.Length - lastIndex);     // 마지막 남은 문자열

            return sb.ToString();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="color"></param>
        /// <param name="isRemoveColor">이미 text에 Color가 입혀져 있을경우 해당 Color를 제거할지</param>
        /// <returns></returns>
        public static string ApplyColorTag(this string str, Color color, bool isRemoveColor = true)
        {
            if (isRemoveColor)
            {
                string removed = RemoveColorTag(str);
                return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{removed}</color>";
            }
            else
                return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{str}</color>";
        }
        
        // 모든 컬러 태그 제거
        public static string RemoveColorTag(this string str) => colorRegex.Replace(str, "");
        // N번째 컬러 태그 제거
        public static string RemoveColorTagAt(this string str, int removeIndex)
        {
            var matches = colorMatchesRegex.Matches(str);
            return matches.Count <= removeIndex ? str : str.Replace(matches[removeIndex].Value, matches[removeIndex].Groups[1].Value);
        }

        public static string RemoveColorTagFirst(this string str, string targetSTR)
        {
            var matches = colorMatchesRegex.Matches(str);
            foreach (Match match in matches)
            {
                if (match.Groups[1].Value == targetSTR)
                {
                    return PatternRemove(match, str);
                }
            }
            return str;
        }

        public static string RemoveColorTagAll(this string str)
        {
            bool Condition(string value) => true;
            return PatternRemoveWhere(colorMatchesRegex.Matches(str), str, Condition);
        }
        
        public static string RemoveColorTagWhere(this string str, Func<string, bool> condition) => PatternRemoveWhere(colorMatchesRegex.Matches(str), str, condition);
        
        //-----------------------------------------
        
        public static string ApplySpriteTag(this string str)
        {
            string removed = RemoveSpriteTag(str);
            return $"<sprite name=\"{removed}\">";
        }

        public static string RemoveSpriteTag(this string str)
        {
            var matches = spriteRegex.Matches(str);
            var sb = new StringBuilder();

            int lastIndex = 0;
            foreach (Match m in matches)
            {
                sb.Append(str, lastIndex, m.Index - lastIndex);     // 일반 문자 포함
                sb.Append(m.Groups[1].Value);                         // 추출한 값
                lastIndex = m.Index + m.Length;                     // 인덱스 갱신
            }
            sb.Append(str, lastIndex, str.Length - lastIndex);     // 마지막 남은 문자열

            return sb.ToString();
        }

        public static string RemoveSpriteTagAt(this string str, int index)
        {
            var matches = spriteRegex.Matches(str);
            if (matches.Count <= index) return str;

            var match = matches[index];
            return PatternRemove(match, str);
        }

        public static string RemoveSpriteTagFirst(this string str, string targetSTR)
        {
            var matches = spriteRegex.Matches(str);
            foreach (Match match in matches)
            {
                if (match.Groups[1].Value == targetSTR)
                {
                    return PatternRemove(match, str);
                }
            }
            return str;
        }

        public static string RemoveSpriteTagAll(this string str)
        {
            bool Condition(string value) => true;
            return PatternRemoveWhere(spriteRegex.Matches(str), str, Condition);
        }

        public static string RemoveSpriteTagWhere(this string str, Func<string, bool> condition) => PatternRemoveWhere(spriteRegex.Matches(str), str, condition);
        
        #endregion

        // indentLevel 만큼 탭을 각 줄 앞에 붙인다.
        // (빈 줄은 그대로 두고 싶으면 preserveEmptyLines=false)
        public static string IndentWithTabs(this string text, int indentLevel, bool preserveEmptyLines = true)
        {
            if (string.IsNullOrEmpty(text) || indentLevel <= 0) return text;

            string indent = new string('\t', indentLevel);

            if (preserveEmptyLines)
            {
                // 모든 줄 시작(^)에 indent 추가
                return Regex.Replace(text, @"^", indent, RegexOptions.Multiline);
            }
            else
            {
                // 빈 줄은 제외하고 indent 추가
                return Regex.Replace(text, @"^(?!\r?$)", indent, RegexOptions.Multiline);
            }
        }
    }
}