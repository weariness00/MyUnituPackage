namespace Weariness.Util
{
    public interface ILootingEntry
    {
        float Weight   { get; }
        int   CountMin { get; }
        int   CountMax { get; }
    }
}
