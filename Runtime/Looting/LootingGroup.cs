using System.Collections.Generic;

namespace Project.Core
{
    /// <summary>
    /// 같은 group_id에 속하는 LootingEntry들의 묶음.
    /// </summary>
    public class LootingGroup<T> : ILootingGroup
    {
        public string                         GroupId         { get; }
        public float                          SelectionWeight { get; }
        public IReadOnlyList<LootingEntry<T>> EntryList       { get; }

        public LootingGroup(string groupId, float selectionWeight, List<LootingEntry<T>> entryList)
        {
            GroupId         = groupId;
            SelectionWeight = selectionWeight;
            EntryList       = entryList;
        }
    }
}
