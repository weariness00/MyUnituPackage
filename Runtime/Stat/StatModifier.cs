using System;
using System.Collections.Generic;
using UnityEngine;

namespace Weariness.Util
{
    [Serializable]
    public partial class StatModifier : IDisposable
    {
        public enum ModifierType { Flat, PercentAdditive, PercentMultiply }

        [SerializeField] protected float value = default;
        public ModifierType type;

        public bool isActive;
        [NonSerialized] private List<Stat> refStatContainer; // 해당 수정자를 참조하고 있는 Stat 들

        public virtual float Value
        {
            get => value;
            set => this.value = value;
        }

        public StatModifier(ModifierType t)
        {
            value = default;
            type = t;
        }
        
        public StatModifier(ModifierType t, float value)
        {
            this.value = value;
            type = t;
        }
        
        public StatModifier(ModifierType t, bool isActive)
        {
            this.value = default;
            type = t;
            this.isActive = isActive;
        }

        public void AddRefStat(Stat stat)
        {
            refStatContainer ??= new();
            refStatContainer.Add(stat);
        }
        
        public void RemoveRefStat(Stat stat)
        {
            refStatContainer?.Remove(stat);
        }

        public void Dispose()
        {
            if (refStatContainer != null)
            {
                var copyRefStatContainer = refStatContainer.ToArray();
                foreach (var stat in copyRefStatContainer)
                    stat.RemoveModifier(this);
                refStatContainer.Clear();
            }
        }
    }
}