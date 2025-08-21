using System;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using UnityEngine;
using UnityEngine.UI;

namespace Weariness.Util.UI
{
    public partial class UIScaler
    {
        public static void ScaleForceUpdate(Vector3 originScale, RectTransform rectTransform, Canvas rootCanvas, CanvasScaler rootCanvasScaler)
        {
            var sx = rootCanvas.transform.localScale.x;
            var sy = rootCanvas.transform.localScale.y;
            var minSize = Mathf.Min(new[]
            {
                originScale.x, 
                originScale.y, 
                originScale.x * 1f / sx, 
                originScale.y * 1f / sy,
                Screen.width / rootCanvasScaler.referenceResolution.x,
                Screen.height / rootCanvasScaler.referenceResolution.y
            });

            if (rootCanvasScaler.matchWidthOrHeight == 0f &&
                sx < 1f)
            {
                minSize = Mathf.Min(originScale.x, Screen.height / rootCanvasScaler.referenceResolution.y);
            }
            else if (rootCanvasScaler.matchWidthOrHeight == 1f &&
                     sy < 1f)
            {
                minSize = Mathf.Min(originScale.y, Screen.width / rootCanvasScaler.referenceResolution.x);
            }

            originScale.x = minSize;
            originScale.y = minSize;
            rectTransform.localScale = originScale;
        }
    }
    
    /// <summary>
    /// 모바일의 Screen 크기는 기기마다 변동이 심해 대응하기 위해 사용
    /// Parent Canvas가 Screen Space - Overlay 및 Root Canvas의 CanvasScaler가  Scale With Screen Size로 설정되어 있어야 합니다.
    /// </summary>
    public partial class UIScaler : MonoBehaviour
    {
        public RectTransform rectTransform;
        public Canvas rootCanvas;
        public CanvasScaler rootCanvasScaler;
        
        [Space]
        public bool isUpdate = true;
        public Vector3 scale = Vector3.one;
        
        private bool isDestroy = false;
        private bool isStretchX = false;
        private bool isStretchY = false;
        
        public void Reset()
        {
            rectTransform = GetComponent<RectTransform>();
            rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
            rootCanvasScaler = rootCanvas.GetComponent<CanvasScaler>();
            scale = rectTransform.localScale;
        }

        public void Awake()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            rootCanvas = rootCanvas == null ? GetComponentInParent<Canvas>().rootCanvas : rootCanvas.rootCanvas;
            rootCanvasScaler = rootCanvas.GetComponent<CanvasScaler>();

            if (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay &&
                rootCanvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                Destroy(this);
                isDestroy = true;
                return;
            }

            CheckStretch();
        }

        public void Update()
        {
            if (isUpdate)
            {
                ScaleForceUpdate(scale);
            }
        }

        public void CheckStretch()
        {
            isStretchX = Mathf.Abs(rectTransform.anchorMin.x - rectTransform.anchorMax.x) < 0.001f;
            isStretchY = Mathf.Abs(rectTransform.anchorMin.y - rectTransform.anchorMax.y) < 0.001f;
        }

        public void ScaleForceUpdate(Vector3 originScale)
        {
            var sx = rootCanvas.transform.localScale.x;
            var sy = rootCanvas.transform.localScale.y;
            var minSize = Mathf.Min(new[]
            {
                originScale.x, 
                originScale.y, 
                originScale.x * 1f / sx, 
                originScale.y * 1f / sy,
                Screen.width / rootCanvasScaler.referenceResolution.x,
                Screen.height / rootCanvasScaler.referenceResolution.y
            });

            if (rootCanvasScaler.matchWidthOrHeight == 0f &&
                sx < 1f)
            {
                minSize = Mathf.Min(originScale.x, Screen.height / rootCanvasScaler.referenceResolution.y);
            }
            else if (rootCanvasScaler.matchWidthOrHeight == 1f &&
                     sy < 1f)
            {
                minSize = Mathf.Min(originScale.y, Screen.width / rootCanvasScaler.referenceResolution.x);
            }

            originScale.x = minSize;
            originScale.y = minSize;
            rectTransform.localScale = originScale;
        }
    }
}