namespace Weariness.Util
{
    public interface ILootingGroup
    {
        string GroupId         { get; }
        float  SelectionWeight { get; }
    }
}
