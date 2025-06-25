using System.IO;
using SimpleJSON;
using UnityEditor;
using UnityEngine;

namespace Weariness.FMOD
{
    [InitializeOnLoad]
    public static class FMOD_ScriptGenerator
    {
        static FMOD_ScriptGenerator() => EditorApplication.delayCall += Init;

        public static void Init()
        {
            CreateScriptFile();
            UpdateFMOD_Assemble();
            AddSymbol();
        }
         
        private static void CreateScriptFile()
        {
            // 원하는 경로 (예: Assets/Scripts/Auto/MyNewClass.cs)
            string folderPath = Path.Combine(Application.dataPath, "Plugins/FMOD/src");

            // C# 스크립트 기본 내용
            var path = Path.Combine(Application.dataPath, "Scripts/Editor/Temp~").Replace("\\", "/");
            var extension = "*.cs";
            string[] csFiles = Directory.GetFiles(path, extension, SearchOption.TopDirectoryOnly);

            foreach (var csFile in csFiles)
            {
                var file = Path.GetFileName(csFile);
                var targetPath = Path.Combine(folderPath, file).Replace("\\", "/");
                

                if (File.Exists(targetPath))
                    File.Copy(csFile, targetPath, true); // b.txt가 있으면 덮어씀
                else
                {
                    string classContent = File.ReadAllText(csFile);
                    File.WriteAllText(targetPath, classContent);
                }
            }

            // 프로젝트 뷰 갱신
            AssetDatabase.Refresh();

            Debug.Log($"FMOD Occlusion Sound Detecting Script 생성 완료 : {folderPath}");
        }

        private static void UpdateFMOD_Assemble()
        {
            string assemblePath = Application.dataPath + "/Plugins/FMOD/FMODUnity.asmdef";
            string referenceAsmdefName = "Weariness.Singleton.Runtime";
    
            // 1. .asmdef 파일 읽기
            if (!File.Exists(assemblePath))
            {
                Debug.LogError($"{assemblePath} 파일이 없습니다.");
                return;
            }
            string json = File.ReadAllText(assemblePath);
            var root = JSON.Parse(json);

            // 2. references 배열 얻기 (없으면 새로 생성)
            var refs = root["references"].AsArray;
            if (refs == null)
            {
                refs = new JSONArray();
                root["references"] = refs;
            }

            // 3. 이미 있는지 확인
            bool found = false;
            for (int i = 0; i < refs.Count; i++)
            {
                if (refs[i].Value == referenceAsmdefName)
                {
                    found = true;
                    Debug.Log($"{referenceAsmdefName} 이미 등록되어 있음.");
                    break;
                }
            }
            if (!found)
            {
                refs.Add(referenceAsmdefName);
                Debug.Log($"{referenceAsmdefName} 참조 추가 완료.");
            }

            // 4. 저장
            File.WriteAllText(assemblePath, root.ToString(2)); // 들여쓰기 2칸
            AssetDatabase.Refresh();
        }

        private static void AddSymbol()
        {
            var targetGroup = BuildTargetGroup.Standalone;
            var symbol = "WEARINESS_FMOD_OCCLUSION";
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            var defineList = new System.Collections.Generic.List<string>(defines.Split(';'));
            if (!defineList.Contains(symbol))
            {
                defineList.Add(symbol);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defineList));
            }
        }
    }
}