using System;
using System.Collections.Generic;
using UnityEngine;

namespace Weariness.Util
{
    [Serializable]
    public class Stat
    {
        public static implicit operator float(Stat stat)
        {
            return stat.Value;
        }
        
        [SerializeField] private float baseValue;
        [SerializeField] private List<StatModifier> modifierList = new List<StatModifier>();

        public float Value
        {
            get => GetValue();
            set => baseValue = value;
        }
        
        public Stat(float baseValue = default)
        {
            this.baseValue = baseValue;
        }

        public void AddModifier(StatModifier modifier)
        {
            modifierList.Add(modifier);
        }

        public void RemoveModifier(StatModifier modifier)
        {
            modifierList.Remove(modifier);
        }

        private float GetValue()
        {
            float finalValue = baseValue;
            float percentAdd = 0f;

            foreach (var mod in modifierList)
            {
                if (mod.type == StatModifier.ModifierType.Flat)
                    finalValue += mod.value;
                else if (mod.type == StatModifier.ModifierType.Percent)
                    percentAdd += mod.value;
            }

            finalValue *= (1 + percentAdd);
            return finalValue;
        }

        public override string ToString()
        {
            return $"Base({baseValue}) Real({Value})";
        }
    }

}