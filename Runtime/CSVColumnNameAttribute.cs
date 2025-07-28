using System;

namespace Weariness.Util.CSV
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class CSVColumnNameAttribute : Attribute
    {
        public string Name { get; }
        public CSVColumnNameAttribute(string name)
        {
            Name = name;
        }
    }
}