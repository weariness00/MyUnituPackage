using System;

namespace Weariness.Util.CSV
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class CSVFieldNameAttribute : Attribute
    {
        public string Name { get; }
        public CSVFieldNameAttribute(string name)
        {
            Name = name;
        }
    }
}