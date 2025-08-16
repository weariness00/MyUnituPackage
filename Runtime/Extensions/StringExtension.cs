using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Weariness.Util.Extensions
{
    public static class StringExtension
    {
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

            return str;
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
    }
}