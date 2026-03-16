using System;
using UnityEngine;

namespace Weariness.Util.Container
{
    // Enum을 인스펙터에서 직렬화 할 때 enum의 순서 즉 index값이 바뀌면 인스펙터의 값이 유지가 안되어 바뀌는 문제를 해결하기 위해 사용
    [Serializable]
    public struct EnumSerialized<TEnum> : ISerializationCallbackReceiver
        where TEnum : struct, Enum
    {
        [SerializeField] private TEnum value;
        [SerializeField] private string valueString; // enum 값의 이름을 저장
        
        public static implicit operator TEnum(EnumSerialized<TEnum> enumSerialized)
        {
            return enumSerialized.value;
        }

        public static implicit operator EnumSerialized<TEnum>(TEnum enumValue)
        {
            return new EnumSerialized<TEnum>
            {
                value = enumValue,
                valueString = enumValue.ToString()
            };
        }
        
        public void OnBeforeSerialize()
        {
            // string이 유효하고, 현재 value가 string과 다르면 string로 복원 시도
            if (string.IsNullOrEmpty(valueString))
                return;

            // 현재 value가 이미 문자열과 동일하면 OK
            if (value.ToString() == valueString)
                return;

            // string 기반 복원
            // ignoreCase=true는 원하면 false로 바꿔도 됩니다.
            if (Enum.TryParse<TEnum>(valueString, ignoreCase: false, out var parsed))
            {
                value = parsed;
            }
            else
            {
                Debug.LogError($"{valueString} 타입이 enum {typeof(TEnum).Name}에 존재하지 않습니다.");
                valueString = value.ToString();
            }
            // else: 이름이 바뀐(리네임) 케이스는 복원 불가 → 그대로 둠
        }

        public void OnAfterDeserialize()
        {
        }
    }
}