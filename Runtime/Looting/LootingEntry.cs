namespace Weariness.Util
{
    /// <summary>
    /// 추첨 항목 하나. Payload(보상 데이터)는 제네릭 T로 받는다.
    /// </summary>
    public class LootingEntry<T> : ILootingEntry
    {
        public T     Payload  { get; }
        public float Weight   { get; }
        public int   CountMin { get; }
        public int   CountMax { get; }

        public LootingEntry(T payload, float weight, int countMin, int countMax)
        {
            Payload  = payload;
            Weight   = weight;
            CountMin = countMin;
            CountMax = countMax;
        }
    }
}
