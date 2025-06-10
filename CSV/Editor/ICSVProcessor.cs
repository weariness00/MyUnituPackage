using UnityEngine;

namespace Util.CSV.Editor
{
    public interface ICSVProcessor
    {
        public void Process(TextAsset textAsset, string path);
    }
}