using System;

namespace Weariness.Util.CSV
{
    /// <summary>
    /// CSV데이터를 파싱에 제외할 변수나 프로퍼티
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class CSVIgnoreAttribute : Attribute
    {
        
    }
}