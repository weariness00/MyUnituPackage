using System;
using System.Collections.Generic;

namespace Weariness.Util
{
    public static class UniqueExtensions
    {
        public static void Init<T>(this UniqueList<T> uniqueList, T start, T end, IUniqueListMaker<T> maker = null)
        {
            maker ??= UniqueList<T>.GetRegisteredMaker();

            if (maker == null)
                throw new Exception("UniqueList를 만들어줄 Maker가 없습니다. UniqueList<T>.RegisterMaker()로 등록하거나 maker 파라미터로 직접 전달해주세요.");

            maker.UniqueMake(uniqueList, start, end);
        }

        public static void Init<T>(this UniqueList<T> uniqueList, IEnumerable<T> source)
        {
            uniqueList.Clear();
            foreach (var item in source)
                uniqueList.Add(item);
        }

        public static UniqueList<T> ToUniqueList<T>(this IEnumerable<T> source)
        {
            var uniqueList = new UniqueList<T>();
            foreach (var value in source)
                uniqueList.Add(value);
            return uniqueList;
        }
    }
}
