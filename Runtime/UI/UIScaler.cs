using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Weariness.Util.UI
{
    /// <summary>
    /// 모바일의 Screen 크기는 기기마다 변동이 심해 대응하기 위해 사용
    /// PC에서도 사용 가능
    /// Parent Canvas가 Screen Space - Overlay 및 Root Canvas의 CanvasScaler가  Scale With Screen Size로 설정되어 있어야 합니다.
    /// </summary>
    public partial class UIScaler : MonoBehaviour
    {
        public RectTransform rectTransform;
        public Canvas rootCanvas;
        [HideInInspector] public RectTransform rootCanvasRectTransform;
        public CanvasScaler rootCanvasScaler;
        
        [Space]
        public bool isUpdate = true;
        private Vector2 originPosition;
        private Vector2 scalingPosition;
        public Vector3 scale = Vector3.one;
        private Vector2 screenSize;
        
        private bool isStretchX = false;
        private bool isStretchY = false;
        
        public void Reset()
        {
            rectTransform = GetComponent<RectTransform>();
            rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
            rootCanvasRectTransform = rootCanvas.GetComponent<RectTransform>();
            rootCanvasScaler = rootCanvas.GetComponent<CanvasScaler>();
            scale = rectTransform.localScale;
        }

        public void Awake()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            rootCanvas = rootCanvas == null ? GetComponentInParent<Canvas>().rootCanvas : rootCanvas.rootCanvas;
            rootCanvasRectTransform = rootCanvas == null ? null : rootCanvas.GetComponent<RectTransform>();
            rootCanvasScaler = rootCanvas.GetComponent<CanvasScaler>();
            originPosition = rectTransform.anchoredPosition;
            screenSize = Util.Resolution;

            if (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay &&
                rootCanvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                Destroy(this);
                return;
            }

            CheckStretch();
            ScaleForceUpdate(scale);
        }

        public void Update()
        {
            if (isUpdate &&
                screenSize != Util.Resolution
                )
            {
                screenSize = Util.Resolution;
                StartCoroutine(ForceUpdateEnumerator());
            }
        }

        public void CheckStretch()
        {
            isStretchX = Mathf.Abs(rectTransform.anchorMin.x - rectTransform.anchorMax.x) >= 1;
            isStretchY = Mathf.Abs(rectTransform.anchorMin.y - rectTransform.anchorMax.y) >= 1;

            if (isStretchX && isStretchY)
            {
                var diff = rectTransform.offsetMin - rectTransform.offsetMax;
                
                var half = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMin = half;
                rectTransform.anchorMax = half;
                rectTransform.pivot = half;

                rectTransform.sizeDelta = rootCanvasScaler.referenceResolution - diff;
            }
        }

        public void PositionForceUpdate(Vector2 originPosition)
        {
            this.originPosition = originPosition;
            PositionForceUpdate();
        }
        
        public void PositionForceUpdate()
        {
            var ratioX = rootCanvasScaler.matchWidthOrHeight;
            var ratioY = 1f - rootCanvasScaler.matchWidthOrHeight;
            if(ratioX > 0f) scalingPosition.x = originPosition.x * rectTransform.localScale.x * ratioX;
            if(ratioY > 0f) scalingPosition.y = originPosition.y * rectTransform.localScale.y * ratioY;
            rectTransform.anchoredPosition = scalingPosition;
        }

        public void ScaleForceUpdate(Vector3 originScale)
        {
            const float tol = 0.001f;
            
            var canvasLocalScaleX = rootCanvas.transform.localScale.x;
            var canvasLocalScaleY = rootCanvas.transform.localScale.y;
            var diffSizeToReferenceResolutionX = rootCanvasRectTransform.rect.width / rootCanvasScaler.referenceResolution.x;
            var diffSizeToReferenceResolutionY =rootCanvasRectTransform.rect.height / rootCanvasScaler.referenceResolution.y;

            var minSize = Mathf.Min(new[]
            {
                originScale.x, 
                originScale.y, 
                originScale.x * 1f / canvasLocalScaleX, 
                originScale.y * 1f / canvasLocalScaleY,
                diffSizeToReferenceResolutionX,
                diffSizeToReferenceResolutionY
            });
            
            // minSize의 보정값이 1이하일 경우 다른 보정값이 1보다는 작지만 MinSize보다 클 수 있을떄
            if (minSize < 1f)
            {
                minSize = Mathf.Max(
                    minSize,
                    diffSizeToReferenceResolutionX < 1f ? diffSizeToReferenceResolutionX : 0f,
                    diffSizeToReferenceResolutionY < 1f ? diffSizeToReferenceResolutionY : 0f
                );
            }

            // CanvasScaler의 확장타입이 width 기준일 경우
            if (rootCanvasScaler.matchWidthOrHeight == 0f)
            {
                if (canvasLocalScaleX < 1f)
                {
                    minSize = Mathf.Min(
                        originScale.x,
                        rootCanvasRectTransform.rect.height / rootCanvasScaler.referenceResolution.y);
                }
                else if (rootCanvasRectTransform.sizeDelta.y >= rootCanvasScaler.referenceResolution.y)
                    minSize = 1f;
            }
            // CanvasScaler의 확장타입이 height 기준일 경우
            else if (rootCanvasScaler.matchWidthOrHeight == 1f)
            {
                if (canvasLocalScaleY < 1f)
                {
                    minSize = Mathf.Min(
                        originScale.y,
                        rootCanvasRectTransform.rect.width / rootCanvasScaler.referenceResolution.x);
                }
                else if (rootCanvasRectTransform.sizeDelta.x >= rootCanvasScaler.referenceResolution.x)
                    minSize = 1f;
            }
            // TO DO : 확장 기준이 일정 비율일 경우에 대해 만들어주기

            var prevScale = rectTransform.localScale;
            
            originScale.x = minSize;
            originScale.y = minSize;
            rectTransform.localScale = originScale;
            if (Vector3.Distance(scalingPosition, rectTransform.anchoredPosition) > tol ||
                Math.Abs(minSize - prevScale.x) > tol)
            {
                PositionForceUpdate();
            }
        }

        private IEnumerator ForceUpdateEnumerator()
        {
            yield return new WaitForEndOfFrame();
            ScaleForceUpdate(scale);
        }
    }
}