using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Weariness.Util.CSV.Editor
{
    [InitializeOnLoad]
    public static class EPPlusInitializer
    {
        private static readonly string SymbolName = "INCLUDE_WEARINESS_CSV_EXCEL";
        
        static EPPlusInitializer() =>
            EditorApplication.delayCall += Init;
        
        public static void Init()
        {
            if(!Directory.Exists(Application.dataPath + "/Plugins/EPPlus"))
            {
                string sourcePath = Application.dataPath + "/02.Script/EPPlus~";

                if (!Directory.Exists(sourcePath))
                {
                    string packageName = "com.weariness.csv";
                    string packagePath = UnityEditor.PackageManager.PackageInfo.FindForAssetPath($"Packages/{packageName}")?.resolvedPath;
                    if (string.IsNullOrEmpty(packagePath))
                    {
                        Debug.LogError("com.weariness.csv 패키지를 찾을 수 없습니다.");
                        return;
                    }
                    
                    sourcePath = Path.Combine(packagePath, "EPPlus~");
                }
                

                string destinationPath = Application.dataPath + "/Plugins/EPPlus";
#if UNITY_6000_0_OR_NEWE || UNITY_2023_1_OR_NEWER || UNITY_2023_2_OR_NEWER || UNITY_2023_3_OR_NEWER || UNITY_2022_1_OR_NEWER || UNITY_2022_2_OR_NEWER || UNITY_2022_3_OR_NEWER
                destinationPath += "/net40";
                #else
                destinationPath += "/net35";
#endif
                CopyAll(new DirectoryInfo(sourcePath), new DirectoryInfo(destinationPath));
                AssetDatabase.Refresh();
                Debug.Log("복사 완료");
                AddSymbol(SymbolName);
            }
        }
        
        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (!target.Exists)
                target.Create();

            foreach (FileInfo fi in source.GetFiles())
            {
                if (fi.Extension == ".meta") continue;
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            foreach (DirectoryInfo subDir in source.GetDirectories())
            {
                CopyAll(subDir, target.CreateSubdirectory(subDir.Name));
            }
        }

        #region Syboml Function

        /// <summary>
        /// 현재 에디터에 설치된 빌드 모듈(실제 지원되는 타겟)에 한해서 심볼 추가
        /// </summary>
        public static void AddSymbol(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol)) return;

            foreach (var nbt in GetSupportedNamedBuildTargets())
                AddSymbol(nbt, symbol);
        }

        /// <summary>
        /// 현재 에디터에 설치된 빌드 모듈(실제 지원되는 타겟)에 한해서 심볼 제거
        /// </summary>
        public static void RemoveSymbol(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol)) return;

            foreach (var nbt in GetSupportedNamedBuildTargets())
                RemoveSymbol(nbt, symbol);
        }

        // ---- 내부 구현 ---------------------------------------------------------
        static IEnumerable<NamedBuildTarget> GetSupportedNamedBuildTargets()
        {
            // BuildTarget 단위로 "지원 여부"를 검사하고,
            // 그 결과를 BuildTargetGroup으로 승격 → 중복 제거 → NamedBuildTarget 변환
            var groups = new HashSet<BuildTargetGroup>();

            foreach (BuildTarget target in Enum.GetValues(typeof(BuildTarget)))
            {
                if (target == BuildTarget.NoTarget) continue;

                var group = BuildPipeline.GetBuildTargetGroup(target);
                if (group == BuildTargetGroup.Unknown) continue;

                if (BuildPipeline.IsBuildTargetSupported(group, target))
                    groups.Add(group);
            }

            return groups.Select(NamedBuildTarget.FromBuildTargetGroup);
        }

        static void AddSymbol(NamedBuildTarget nbt, string symbol)
        {
            var set = GetDefineSet(nbt);
            // 정확히 동일한 토큰만 판단 (대소문자 구분)
            if (set.Add(symbol))
                SaveDefineSet(nbt, set);
        }

        static void RemoveSymbol(NamedBuildTarget nbt, string symbol)
        {
            var set = GetDefineSet(nbt);
            if (set.Remove(symbol))
                SaveDefineSet(nbt, set);
        }

        static HashSet<string> GetDefineSet(NamedBuildTarget nbt)
        {
            // 새 API: string 반환 (세미콜론 구분)
            var defines = PlayerSettings.GetScriptingDefineSymbols(nbt) ?? string.Empty;

            return new HashSet<string>(
                defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => s.Length > 0),
                StringComparer.Ordinal // Unity는 define 토큰을 대소문자 구분함
            );
        }

        static void SaveDefineSet(NamedBuildTarget nbt, HashSet<string> set)
        {
            // 정렬은 선택 사항이지만, 깔끔한 diff를 위해 해둠
            var joined = string.Join(";", set.OrderBy(s => s, StringComparer.Ordinal));
            PlayerSettings.SetScriptingDefineSymbols(nbt, joined);
        }

        #endregion

    }
}