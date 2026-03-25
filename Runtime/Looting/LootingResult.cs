namespace Project.Core
{
    /// <summary>
    /// 추첨 결과 하나. 디버깅용 SourceGroupId 포함.
    /// </summary>
    public class LootingResult<T>
    {
        public T      Payload       { get; }
        public int    Count         { get; }
        public string SourceGroupId { get; }

        public LootingResult(T payload, int count, string sourceGroupId)
        {
            Payload       = payload;
            Count         = count;
            SourceGroupId = sourceGroupId;
        }
    }
}
