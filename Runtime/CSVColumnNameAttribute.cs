using System;

namespace Weariness.Util.CSV
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class CSVColumnNameAttribute : Attribute
    {
        public string Name { get; }
        public CSVColumnNameAttribute(string name)
        {
            Name = name;
        }
    }
}