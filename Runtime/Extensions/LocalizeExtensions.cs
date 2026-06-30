using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Weariness.Util.Extensions
{
    public static class LocalizeExtensions
    {
        public static async UniTask<string> LocalizeAsync(this string token, string table, params string[] args)
        {
            var handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(table, token);
            await handle;                  // AsyncOperationHandle은 직접 await 가능
            if(handle.Status == AsyncOperationStatus.Succeeded)
                return handle.Result;
            else
            {
                Debug.LogError($"{table}에 Key:{token} 이 존재하지 않습니다.");
                return token;
            }
        }
        
        public static string Localize(this string token, string table, params string[] args)
        {
            var handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(table, token);
            handle.WaitForCompletion();
            if(handle.Status == AsyncOperationStatus.Succeeded)
                return handle.Result;
            else
            {
                Debug.LogError($"{table}에 Key:{token} 이 존재하지 않습니다.");
                return token;
            }
        }
    }
}