using System.Collections.Generic;
using UnityEngine;

// ── 그룹 추첨 ──────────────────────────────────────────────────────
// [단일 그룹 반환] 여러 그룹 중 SelectionWeight 기반으로 1개 선택
// var group = resolver.PickGroup(groups);

// [N개 그룹 반환 - 중복 허용] 같은 그룹이 여러 번 뽑힐 수 있음
// var pickedGroups = resolver.PickGroups(groups, count: 3, LootingDuplicateMode.Allow);

// [N개 그룹 반환 - 중복 불허] 한 번 뽑힌 그룹은 풀에서 제거
// var pickedGroups = resolver.PickGroups(groups, count: 3, LootingDuplicateMode.Disallow);

// [전체 그룹 반환] 모든 그룹을 전부 반환 (중복 없음 고정)
// var allGroups = resolver.PickAllGroups(groups);

// ── 항목 추첨 ──────────────────────────────────────────────────────
// [단일 추첨] 그룹 내에서 1개 추첨
// var result = resolver.Resolve(group);

// [N개 추첨 - 중복 허용]
// var results = resolver.Resolve(group, count: 3, LootingDuplicateMode.Allow);

// [N개 추첨 - 중복 불허]
// var results = resolver.Resolve(group, count: 3, LootingDuplicateMode.Disallow);

// [전체 추첨] 그룹 내 모든 항목을 전부 추첨
// var results = resolver.ResolveAll(group);

// ── 조합 사용 예시 ─────────────────────────────────────────────────
// [그룹 1개 선택 후 단일 추첨] 약탈 구간 보상
// var group  = resolver.PickGroup(sectionGroups);
// var result = resolver.Resolve(group);

// [그룹 N개 선택 후 각각 단일 추첨] 복수 구간 독립시행
// var picked  = resolver.PickGroups(sectionGroups, count: 5, LootingDuplicateMode.Disallow);
// var results = picked.Select(g => resolver.Resolve(g)).ToList();

namespace Weariness.Util
{
    /// <summary>
    /// 실제 추첨 실행기. 그룹 추첨과 항목 추첨 두 축으로 구성된다.
    /// DataTable 연동 없는 순수 Core 클래스. MonoBehaviour 의존 없음.
    /// </summary>
    public static class LootingResolver
    {
        // ─── 그룹 추첨 ────────────────────────────────────────────────────

        /// <summary>
        /// 여러 그룹 중 SelectionWeight 기반으로 1개 선택하여 반환.
        /// </summary>
        public static LootingGroup<T> PickGroup<T>(List<LootingGroup<T>> groups)
        {
            if (groups == null || groups.Count == 0) return null;
            return PickGroupFrom(groups);
        }

        /// <summary>
        /// 여러 그룹 중 count개를 SelectionWeight 기반으로 추첨.
        /// Disallow일 때 count가 그룹 수를 초과하면 그룹 수만큼만 반환.
        /// </summary>
        public static List<LootingGroup<T>> PickGroups<T>(
            List<LootingGroup<T>> groups,
            int count,
            LootingDuplicateMode duplicateMode)
        {
            var result = new List<LootingGroup<T>>();
            if (groups == null || groups.Count == 0) return result;

            if (duplicateMode == LootingDuplicateMode.Allow)
            {
                for (int i = 0; i < count; i++)
                {
                    var picked = PickGroupFrom(groups);
                    if (picked != null) result.Add(picked);
                }
            }
            else
            {
                var pool = new List<LootingGroup<T>>(groups);
                int pickCount = Mathf.Min(count, pool.Count);
                for (int i = 0; i < pickCount; i++)
                {
                    var picked = PickGroupFrom(pool);
                    if (picked == null) break;
                    result.Add(picked);
                    pool.Remove(picked);
                }
            }

            return result;
        }

        /// <summary>
        /// 모든 그룹을 SelectionWeight 순서 없이 전부 반환 (중복 없음 고정).
        /// </summary>
        public static List<LootingGroup<T>> PickAllGroups<T>(List<LootingGroup<T>> groups)
        {
            if (groups == null) return new List<LootingGroup<T>>();
            return new List<LootingGroup<T>>(groups);
        }

        // ─── 항목 추첨 ────────────────────────────────────────────────────

        /// <summary>
        /// 그룹 내 항목들 중 Weight 기반으로 1개 선택.
        /// </summary>
        public static LootingResult<T> Resolve<T>(LootingGroup<T> group)
        {
            if (group == null || group.EntryList == null || group.EntryList.Count == 0) return null;

            var picked = PickEntryFrom(group.EntryList);
            if (picked == null) return null;

            int count = ResolveCount(picked);
            return new LootingResult<T>(picked.Payload, count, group.GroupId);
        }

        /// <summary>
        /// 그룹 내에서 count개를 추첨.
        /// Disallow일 때 count가 항목 수를 초과하면 항목 수만큼만 추첨.
        /// </summary>
        public static List<LootingResult<T>> Resolve<T>(
            LootingGroup<T> group,
            int count,
            LootingDuplicateMode duplicateMode)
        {
            var result = new List<LootingResult<T>>();
            if (group == null || group.EntryList == null || group.EntryList.Count == 0) return result;

            if (duplicateMode == LootingDuplicateMode.Allow)
            {
                for (int i = 0; i < count; i++)
                {
                    var r = Resolve(group);
                    if (r != null) result.Add(r);
                }
            }
            else
            {
                var pool = new List<LootingEntry<T>>(group.EntryList);
                int pickCount = Mathf.Min(count, pool.Count);
                for (int i = 0; i < pickCount; i++)
                {
                    var picked = PickEntryFrom(pool);
                    if (picked == null) break;
                    result.Add(new LootingResult<T>(picked.Payload, ResolveCount(picked), group.GroupId));
                    pool.Remove(picked);
                }
            }

            return result;
        }

        /// <summary>
        /// 그룹 내 모든 항목을 전부 추첨 (중복 없음 고정).
        /// 각 항목별 Count는 CountMin~CountMax 범위 내에서 결정.
        /// </summary>
        public static List<LootingResult<T>> ResolveAll<T>(LootingGroup<T> group)
        {
            var result = new List<LootingResult<T>>();
            if (group == null || group.EntryList == null) return result;

            foreach (var entry in group.EntryList)
                result.Add(new LootingResult<T>(entry.Payload, ResolveCount(entry), group.GroupId));

            return result;
        }

        // ─── 내부 헬퍼 ───────────────────────────────────────────────────

        static LootingGroup<T> PickGroupFrom<T>(List<LootingGroup<T>> pool)
        {
            float totalWeight = 0f;
            foreach (var g in pool) totalWeight += g.SelectionWeight;
            if (totalWeight == 0f) return null;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;
            foreach (var g in pool)
            {
                cumulative += g.SelectionWeight;
                if (roll < cumulative) return g;
            }
            return pool[pool.Count - 1];
        }

        static LootingEntry<T> PickEntryFrom<T>(IReadOnlyList<LootingEntry<T>> pool)
        {
            float totalWeight = 0f;
            foreach (var e in pool) totalWeight += e.Weight;
            if (totalWeight == 0f) return null;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;
            foreach (var e in pool)
            {
                cumulative += e.Weight;
                if (roll < cumulative) return e;
            }
            return pool[pool.Count - 1];
        }

        static LootingEntry<T> PickEntryFrom<T>(List<LootingEntry<T>> pool)
        {
            float totalWeight = 0f;
            foreach (var e in pool) totalWeight += e.Weight;
            if (totalWeight == 0f) return null;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;
            foreach (var e in pool)
            {
                cumulative += e.Weight;
                if (roll < cumulative) return e;
            }
            return pool[pool.Count - 1];
        }

        static int ResolveCount(ILootingEntry entry)
        {
            return entry.CountMin == entry.CountMax
                ? entry.CountMin
                : Random.Range(entry.CountMin, entry.CountMax + 1);
        }
    }
}
