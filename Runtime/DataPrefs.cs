using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Weariness.Util
{
    public static class DataPrefs
    {
        public static readonly string AddressableKey = "Weariness.Util/DataPrefs";
        private static string Name = "DataPrefs";
        private static readonly string DefaultFilePath = "Packages/com.weariness.dataprefs/Setting/DataPrefs.json";
        private static Dictionary<string, string> prefs;

        static DataPrefs()
        {
            prefs = new Dictionary<string, string>();
            LoadPrefs();
        }
        
        private static void SavePrefs()
        {
            string json = JsonUtility.ToJson(new Serialization<string, string>(prefs));

#if UNITY_EDITOR
            File.WriteAllText(
                File.Exists(
                    Path.Join(Application.dataPath, "Packages/com.weariness.dataprefs/Setting/DataPrefs.json")) ?
                    DefaultFilePath :
                    Path.Join(Application.dataPath, "Scripts/Setting/DataPrefs.json")
                ,json);
            AssetDatabase.ImportAsset(DefaultFilePath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
#else
            // 어드레서블로 불러와서 저장
            
#endif
            Debug.Log($"[Editor] {DefaultFilePath} 내용이 갱신되었습니다.");
        }

        public static void LoadPrefs()
        {
            string json = "";
#if UNITY_EDITOR
            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(DefaultFilePath) ?? AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Scripts/Setting/DataPrefs.json");
            if(textAsset != null)
                json = textAsset.text;
#else
            var handle = Addressables.LoadAssetAsync<TextAsset>(AddressableKey);
            handle.WaitForCompletion();
            var textAsset = handle.Result;
            json = textAsset.text;
#endif
            if(json == "") return;
            prefs = JsonUtility.FromJson<Serialization<string, string>>(json).ToDictionary();
        }
        
        public static bool HasKey(string key) => prefs.ContainsKey(key);
        
        public static void SetString(string key, string value)
        {
            prefs[key] = value;
            SavePrefs();
        }

        public static string GetString(string key, string defaultValue = "")
        {
            return prefs.TryGetValue(key, out var pref) ? pref : defaultValue;
        }

        public static void SetInt(string key, int value)
        {
            prefs[key] = value.ToString();
            SavePrefs();
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            if (prefs.ContainsKey(key) && int.TryParse(prefs[key], out int value))
            {
                return value;
            }

            return defaultValue;
        }

        public static void SetFloat(string key, float value)
        {
            prefs[key] = value.ToString();
            SavePrefs();
        }

        public static float GetFloat(string key, float defaultValue = 0.0f)
        {
            if (prefs.ContainsKey(key) && float.TryParse(prefs[key], out float value))
            {
                return value;
            }

            return defaultValue;
        }

        public static void SetBool(string key, bool value)
        {
            prefs[key] = value.ToString();
            SavePrefs();
        }

        public static bool GetBool(string key, bool defaultValue = true)
        {
            if (prefs.ContainsKey(key) && bool.TryParse(prefs[key], out bool value))
            {
                return value;
            }

            return defaultValue;
        }

        [System.Serializable]
        private class Serialization<TKey, TValue> // Helper class to serialize the dictionary
        {
            public List<TKey> keys = new List<TKey>();
            public List<TValue> values = new List<TValue>();

            public Serialization(Dictionary<TKey, TValue> dict)
            {
                foreach (KeyValuePair<TKey, TValue> pair in dict)
                {
                    keys.Add(pair.Key);
                    values.Add(pair.Value);
                }
            }

            public Dictionary<TKey, TValue> ToDictionary()
            {
                Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();
                for (int i = 0; i < keys.Count; i++)
                {
                    dict[keys[i]] = values[i];
                }

                return dict;
            }
        }
    }
}
