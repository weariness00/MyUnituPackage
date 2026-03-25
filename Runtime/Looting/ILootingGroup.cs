namespace Project.Core
{
    public interface ILootingGroup
    {
        string GroupId         { get; }
        float  SelectionWeight { get; }
    }
}
