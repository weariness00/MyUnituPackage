namespace Weariness.Util
{
    /// <summary>
    /// 추첨 개수 방식.
    /// </summary>
    public enum LootingPickMode
    {
        Single, // 1개만 추첨
        Count,  // N개 추첨 (호출 시 count 인자로 지정)
        All,    // 전체 추첨
    }
}
