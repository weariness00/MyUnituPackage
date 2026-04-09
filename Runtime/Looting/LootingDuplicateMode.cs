namespace Weariness.Util
{
    /// <summary>
    /// N개 추첨 시 중복 허용 여부.
    /// </summary>
    public enum LootingDuplicateMode
    {
        Allow,    // 중복 허용 (같은 항목이 여러 번 뽑힐 수 있음)
        Disallow, // 중복 불허 (한 번 뽑힌 항목은 풀에서 제거)
    }
}
