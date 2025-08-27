using System;
using System.Collections.Generic;

namespace Weariness.Util
{
    [Serializable]
    public partial class StatModifier : IDisposable
    {
        public enum ModifierType { Flat, Percent }

        public float value = default;
        public ModifierType type;

        public bool isActive;

        [NonSerialized] private List<Stat> refStatContainer = new(); // 해당 수정자를 참조하고 있는 Stat 들
        [NonSerialized] private bool isDisposed = false;

        public StatModifier(ModifierType t)
        {
            value = default;
            type = ModifierType.Flat;
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

        ~StatModifier()
        {
            Dispose();
        }

        public void AddRefStat(Stat stat)
        {
            refStatContainer.Add(stat);
        }
        
        public void RemoveRefStat(Stat stat)
        {
            refStatContainer.Remove(stat);
        }

        public void Dispose()
        {
            if (isDisposed == false)
            {
                isDisposed = true;
                foreach (var stat in refStatContainer)
                    stat.RemoveModifier(this);
                refStatContainer.Clear();
            }
        }
    }
}