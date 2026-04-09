using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Weariness.Util
{
    [Serializable]
    public class ThresholdTracker : ISerializationCallbackReceiver
    {
        public enum SetupMode
        {
            ByDivision, // 균등 분할
            ByRatio,    // 비율 기반 (0~1)
            ByValues    // 절댓값 직접 지정
        }

        [SerializeField] public ThresholdDirection direction = ThresholdDirection.Ascending;
        [SerializeField] public SetupMode setupMode = SetupMode.ByDivision;
        [SerializeField] public float maxValue = 1f;
        [SerializeField] public int divisions = 4;
        [SerializeField] public float[] ratioList = { 0.25f, 0.5f, 0.75f, 1f };
        [SerializeField] public float[] valueList = { };

        public UnityEvent<int> OnThresholdCrossed = new();
        public UnityEvent<int> OnThresholdUncrossed = new();

        private List<ThresholdEntry> entryList;

        public IReadOnlyList<ThresholdEntry> Entries => entryList;
        public int CrossedCount { get; private set; }
        public bool IsAllCrossed => CrossedCount >= entryList.Count;

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() => Init();

        public void Init()
        {
            float[] values;

            switch (setupMode)
            {
                case SetupMode.ByDivision:
                    values = new float[divisions];
                    float step = maxValue / divisions;
                    for (int i = 0; i < divisions; i++)
                        values[i] = maxValue - step * (i + 1);
                    break;

                case SetupMode.ByRatio:
                    values = new float[ratioList.Length];
                    for (int i = 0; i < ratioList.Length; i++)
                        values[i] = maxValue * ratioList[i];
                    break;

                default: // ByValues
                    values = valueList;
                    break;
            }

            var sorted = new float[values.Length];
            Array.Copy(values, sorted, values.Length);

            if (direction == ThresholdDirection.Descending)
                Array.Sort(sorted, (a, b) => b.CompareTo(a));
            else
                Array.Sort(sorted, (a, b) => a.CompareTo(b));

            entryList = new List<ThresholdEntry>(sorted.Length);
            for (int i = 0; i < sorted.Length; i++)
                entryList.Add(new ThresholdEntry { Index = i, Value = sorted[i], IsCrossed = false });

            CrossedCount = 0;
        }

        public void Update(float currentValue)
        {
            for (int i = 0; i < entryList.Count; i++)
            {
                var entry = entryList[i];

                bool crossed = direction == ThresholdDirection.Descending
                    ? currentValue <= entry.Value
                    : currentValue >= entry.Value;

                if (!entry.IsCrossed && crossed)
                {
                    entry.IsCrossed = true;
                    entryList[i] = entry;
                    CrossedCount++;
                    OnThresholdCrossed.Invoke(entry.Index);
                }
                else if (entry.IsCrossed && !crossed)
                {
                    entry.IsCrossed = false;
                    entryList[i] = entry;
                    CrossedCount--;
                    OnThresholdUncrossed.Invoke(entry.Index);
                }
            }
        }

        public void Reset()
        {
            for (int i = 0; i < entryList.Count; i++)
            {
                var entry = entryList[i];
                entry.IsCrossed = false;
                entryList[i] = entry;
            }
            CrossedCount = 0;
        }
    }
}
