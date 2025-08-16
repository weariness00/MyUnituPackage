using System;

namespace Weariness.Util
{
    [Serializable]
    public partial class StatModifier
    {
        public enum ModifierType { Flat, Percent }

        public float value = default;
        public ModifierType type;

        public bool isActive;

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
    }
}