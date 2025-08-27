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
        [SerializeField] private List<StatModifier> modifierContainer = new ();
        
        // 최초 1회만 사용
        [NonSerialized] private bool isDirty = true; // 값이 변경되었는지 여부
        
        [NonSerialized] private bool isDisposed = false;
        
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

        ~Stat()
        {
            Dispose();
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

        private float GetValue()
        {
            isDirty = true;
            
            float finalValue = baseValue;
            float percentAdd = 0f;

            foreach (var mod in modifierContainer)
            {
                if(mod == null) continue;
                switch (mod.type)
                {
                    case StatModifier.ModifierType.Flat:
                        finalValue += mod.value;
                        break;
                    case StatModifier.ModifierType.Percent:
                        percentAdd += mod.value;
                        break;
                }
            }

            finalValue *= (1 + percentAdd);
            return finalValue;
        }

        public override string ToString()
        {
            return $"Base({baseValue}) Real({Value})";
        }

        public void Dispose()
        {
            if(isDisposed == false)
            {
                isDisposed = true;
                foreach (var mod in modifierContainer)
                    mod.RemoveRefStat(this);
                modifierContainer.Clear();
            }
        }
    }

}