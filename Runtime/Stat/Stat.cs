using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Weariness.Util
{
    [Serializable]
    public class Stat : IDisposable
    {
        public static implicit operator float(Stat stat)
        {
            return stat.Value;
        }
        
        [SerializeField] private float baseValue;
        [NonSerialized] private float cachedValue; // 캐싱된 최종 값
        [SerializeField] private List<StatModifier> modifierContainer = new (); // 스탯이 소지한 다른 Modifier들
        // 스탯 자체를 다른 스탯의 수정자로 사용하고 싶을때 사용
        // 런타임 전용
        [NonSerialized] private Dictionary<StatModifier.ModifierType, StatModifier> modifiers = new(); 
        
        // 최초 1회만 사용
        [NonSerialized] private bool isDirty = false; // 값이 변경되었는지 여부
        
        public float BaseValue
        {
            get => baseValue;
            set
            {
                baseValue = value;
                cachedValue = GetValue();
            }
        }

        public float Value
        {
            get
            {
                if(isDirty == false) 
                    cachedValue = GetValue();
                return cachedValue;
            }
        }

        public Stat(float baseValue = default)
        {
            this.baseValue = baseValue;
            cachedValue = GetValue();
        }

        public void AddModifier(StatModifier modifier)
        {
            modifierContainer.Add(modifier);
            modifier.AddRefStat(this);
            cachedValue = GetValue();
        }

        public void RemoveModifier(StatModifier modifier)
        {
            modifierContainer.Remove(modifier);
            modifier.RemoveRefStat(this);
            cachedValue = GetValue();
        }

        public void ClearModifier(StatModifier modifier)
        {
            foreach (var statModifier in modifierContainer)
                modifier.RemoveRefStat(this);
            modifierContainer.Clear();
            cachedValue = GetValue();
        }

        private float GetValue()
        {
            isDirty = true;
            
            float finalValue = baseValue;
            float percentAdd = 0f;
            float percentMul = 1f;

#if UNITY_EDITOR
            modifierContainer ??= new();
#endif

            foreach (var mod in modifierContainer)
            {
                if(mod == null) continue;
                switch (mod.type)
                {
                    case StatModifier.ModifierType.Flat:
                        finalValue += mod.Value;
                        break;
                    case StatModifier.ModifierType.PercentAdditive:
                        percentAdd += mod.Value;
                        break;
                    case StatModifier.ModifierType.PercentMultiply:
                        percentMul *= (1 + mod.Value);
                        break;
                }
            }

            finalValue *= (1 + percentAdd) * percentMul;
            
            // Inspector 표시용으로 GetValue를 사용하고 있다.
            // Modifiers는 런타임 전용임으로 Editor에서는 제외
#if !UNITY_EDITOR
            foreach (var (key, modifier) in modifiers)
                modifier.Value = finalValue;
#endif
            return finalValue;
        }

        // Stat자체를 Modifier로 변환해주는 함수
        public StatModifier AsModifier(StatModifier.ModifierType modifierType)
        {
            modifiers ??= new();
            if (!modifiers.TryGetValue(modifierType, out var modifier))
            {
                modifier = new(modifierType, GetValue());
                modifiers[modifierType] = modifier;
            }
            return modifier;
        }

        public override string ToString()
        {
            return $"Base({baseValue}) Real({Value})";
        }

        public void Dispose()
        {
            if (modifierContainer != null)
            {
                foreach (var modifier in modifierContainer)
                    modifier.RemoveRefStat(this);
                modifierContainer.Clear();
            }
            if (modifiers != null)
            {
                foreach (var (key, modifier) in modifiers)
                    modifier.Dispose();
                modifiers.Clear();
            }

            baseValue = 0;
            cachedValue = 0;

            isDirty = false;
        }
    }

}