using System.Globalization;
using UnityEngine;

namespace Weariness.Util.Extensions
{
    public static class ColorExtension
    {
        public static Color32 ParseHex(string hex)
        {
            if (hex.StartsWith("#")) hex = hex.Substring(1);
            if (hex.Length != 6)
            {
                Debug.LogError($"[{hex}]는 제대로 된 컬러값이 아닙니다. FFFFFF 형태로 입력해주세요.");
                return Color.white;
            }
            byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            return new Color32(r, g, b, 255);
        }   
    }
}