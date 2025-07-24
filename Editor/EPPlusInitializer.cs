using System.IO;
using UnityEditor;
using UnityEngine;

namespace Weariness.Util.CSV.Editor
{
    [InitializeOnLoad]
    public static class EPPlusInitializer
    {
        static EPPlusInitializer() =>
            EditorApplication.delayCall += Init;
        
        public static void Init()
        {
            if(!Directory.Exists(Application.dataPath + "/Plugins/EPPlus"))
            {
                string sourcePath = Application.dataPath + "Scripts/EPPlus~";

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
    }
}