using UnityEngine;

namespace Util
{
    public static class MathExtension
    {
        public static int Random(this int value) => UnityEngine.Random.Range(0, value);
        
        public static float GetNearValue(this float value, float min, float max) => Mathf.Abs(value - min) < Mathf.Abs(value - max) ? min : max;
        public static float GetFarValue(this float value, float min, float max) => Mathf.Abs(value - min) > Mathf.Abs(value - max) ? min : max;
        
        public static int GetDigitCount(this int number)
        {
            if (number == 0)
                return 1;

            return (int)Mathf.Log10(Mathf.Abs(number)) + 1 + (number < 0 ? 1 : 0);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <param name="decimalPlaces">소수점 자릿수 최대치</param>
        /// <returns></returns>
        public static int GetDigitCount(this float number, int decimalPlaces = 3)
        {
            if (number == 0)
                return 1;
            
            // 정수 부분 자릿수 계산
            int integerDigits = (int)Mathf.Log10(Mathf.Abs(number)) + 1;

            // 소수점 부분 자릿수 계산
            string numStr = number.ToString();
            int decimalDigits = numStr.Contains(".") ? numStr.Split('.')[1].Length : 0;
            decimalDigits = decimalDigits > decimalPlaces ? decimalPlaces : decimalDigits;

            return decimalDigits + integerDigits + (number < 0 ? 1 : 0);
        }
        
        /// <summary>
        /// 0~maxProbability에서 랜덤으로 뽑은 숫자가 probability보다 낮으면 성공
        /// </summary>
        /// <param name="probability">현재 확률</param>
        /// <param name="maxProbability">확률의 최대치</param>
        /// <returns></returns>
        public static bool IsProbability(this float probability, float maxProbability = 1f)
        {
            var value = UnityEngine.Random.Range(0f, maxProbability);
            
            return value < probability;
        }
        
        /// <summary>
        /// 0~maxProbability에서 랜덤으로 뽑은 숫자가 probability보다 낮으면 성공
        /// </summary>
        /// <param name="probability">현재 확률</param>
        /// <param name="maxProbability">확률의 최대치</param>
        /// <returns></returns>
        public static bool IsProbability(this int probability, int maxProbability = 1)
        {
            var value = UnityEngine.Random.Range(0, maxProbability);
            
            return value < probability;
        }
    }
}