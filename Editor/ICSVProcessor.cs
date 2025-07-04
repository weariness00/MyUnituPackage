using UnityEngine;

namespace Weariness.Util.CSV.Editor
{
    public interface ICSVProcessor
    {
        public string CSV_Name { get; set; }
        public void Process(TextAsset textAsset, string path);
    }
}