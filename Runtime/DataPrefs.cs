using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Weariness.Util
{
    public static class DataPrefs
    {
        public static readonly string AddressableKey = "Weariness.Util/DataPrefs";
        private static string Name = "DataPrefs";
        private static Dictionary<string, string> prefs;

        static DataPrefs()
        {
            prefs = new Dictionary<string, string>();
#if UNITY_EDITOR
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if(Directory.Exists(Path.Combine(Application.dataPath, "Resources/DataPrefs")) == false)
                AssetDatabase.CreateFolder("Assets/Resources", "DataPrefs");
#endif
            LoadPrefs();
        }
        
        public static void SavePrefs()
        {
            string json = JsonUtility.ToJson(new Serialization<string, string>(prefs));

#if UNITY_EDITOR
            var path = Path.Join(Application.dataPath, "Resources/DataPrefs/DataPrefs.json");
            File.WriteAllText(path,json);
            AssetDatabase.Refresh();
            Debug.Log($"DataPrefs의 내용이 갱신되었습니다.\n경로 : {path}");
#else
            // 저장 기능 없음
            Debug.Log("DataPrefs의 저장은 Editor에서만 가능합니다.");
#endif
        }

        public static void LoadPrefs()
        {
            string json = "";

            // 1. Resources에서 로드 시도
            var textAsset = Resources.Load<TextAsset>("DataPrefs/DataPrefs");
            if (textAsset != null)
                json = textAsset.text;

#if UNITY_EDITOR
    // 2. Resources.Load 실패 시 파일 직접 읽기
    if (string.IsNullOrEmpty(json))
    {
        string filePath = Path.Combine(Application.dataPath, "Resources/DataPrefs/DataPrefs.json");
        if (File.Exists(filePath))
        {
            try
            {
                json = File.ReadAllText(filePath);
                Debug.Log("[DataPrefs] Fallback 로딩 성공: 파일 직접 읽음");
            }
            catch (IOException e)
            {
                Debug.LogWarning($"[DataPrefs] 직접 로딩 실패: {e.Message}");
            }
        }
    }
#endif

            // 3. 불러온 JSON으로 prefs 덮어쓰기
            if (!string.IsNullOrEmpty(json))
            {
                prefs = JsonUtility.FromJson<Serialization<string, string>>(json).ToDictionary();
            }
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
