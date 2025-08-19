using System;
using UnityEngine;

namespace Weariness.Util.Mobile
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaApplier : MonoBehaviour
    {
        public RectTransform rectTransform; // 비워두면 자기 자신

        [Header("축별 적용")] public bool affectX = true;
        public bool affectY = true;

        public Vector2 areaSize; // 셀 크기 (비율로 적용)
        public RectOffset padding; // 여백

        private bool isApplied = true;

        private Vector2 originOffsetMin;
        private Vector2 originOffsetMax;

        public void Reset()
        {
            rectTransform = GetComponent<RectTransform>();
            areaSize = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
        }

        public void OnValidate()
        {
            Apply();
        }

        public void Awake()
        {
            originOffsetMin = rectTransform.offsetMin;
            originOffsetMax = rectTransform.offsetMax;

            isApplied = false;
        }

        public void OnEnable()
        {
            Apply();
        }

        public void OnRectTransformDimensionsChange()
        {
            Apply();
        }

#if UNITY_EDITOR
        public void FixedUpdate()
        {
            // 에디터에서 값을 수정하면 Screen이 Game Window의 View 크기를 가져오는 문제를 해결하기 위해 사용
            Apply();
        }
#endif

        public void Apply()
        {
            const float epsilon = 0.001f;

            // 대응 ui 적용 안할경우
            if ((affectX || affectY) == false) return;

            if (isApplied) return;
            isApplied = true;

            try
            {
                var rt = rectTransform ? rectTransform : (RectTransform)transform;
                ;
                if (!rt) return;

                //safeArea를 받아서 min 앵커와 max 앵커에 Position 부여
                //픽셀로 반환되니 앵커에 넣기 위해서는 비율로 변환 필요
                var safeArea = Screen.safeArea;
                var screen = new Vector2(Screen.width, Screen.height);

                Vector2 minAnchor = safeArea.position;
                Vector2 maxAnchor = safeArea.position + safeArea.size;

                var minX = minAnchor.x / screen.x;
                var minY = minAnchor.y / screen.y;
                var maxX = maxAnchor.x / screen.x;
                var maxY = maxAnchor.y / screen.y;

                var rootRectTransform = GetComponentInParent<Canvas>().rootCanvas.GetComponent<RectTransform>();
                var canvasSize = rootRectTransform.sizeDelta;

                var left = minX * canvasSize.x;
                var right = maxX * canvasSize.x;
                var bottom = minY * canvasSize.y;
                var top = maxY * canvasSize.y;

                var stretchRight = canvasSize.x - right;
                var stretchTop = canvasSize.y - top;

                // 확장형 UI 인지
                bool stretchX = Mathf.Abs(rt.anchorMin.x - rt.anchorMax.x) > epsilon;
                bool stretchY = Mathf.Abs(rt.anchorMin.y - rt.anchorMax.y) > epsilon;

                if (stretchX && stretchY &&
                    affectX && affectY)
                {
                    rt.offsetMin = new Vector2(left + padding.left, bottom + padding.bottom);
                    rt.offsetMax = new Vector2(-stretchRight - padding.right, -stretchTop - padding.top);
                }
                else if (stretchX && affectX)
                {
                    rt.offsetMin = new Vector2(left + padding.left, rt.offsetMin.y);
                    rt.offsetMax = new Vector2(-stretchRight - padding.right, rt.offsetMax.y);

                    var ap = rt.anchoredPosition;
                    ap.y = (-padding.bottom + padding.top) * (1f - rt.pivot.y);
                    rt.anchoredPosition = ap;

                    var sd = rt.sizeDelta;
                    sd.y = areaSize.y - padding.vertical;
                    rt.sizeDelta = sd;
                }
                else if (stretchY && affectY)
                {
                    rt.offsetMin = new Vector2(rt.offsetMin.x, bottom + padding.bottom);
                    rt.offsetMax = new Vector2(rt.offsetMax.x, -stretchTop - padding.top);

                    var ap = rt.anchoredPosition;
                    ap.x = (-padding.left + padding.right) * (1f - rt.pivot.x);
                    rt.anchoredPosition = ap;

                    var sd = rt.sizeDelta;
                    sd.x = areaSize.x - padding.horizontal;
                    rt.sizeDelta = sd;
                }
                else
                {
                    // SafeArea(패딩 포함) 경계 in canvas px
                    float safeLeft = left + padding.left;
                    float safeRight = right - padding.right;
                    float safeBottom = bottom + padding.bottom;
                    float safeTop = top - padding.top;

                    float safeW = Mathf.Max(0f, safeRight - safeLeft);
                    float safeH = Mathf.Max(0f, safeTop - safeBottom);

                    // 1) 크기 보정: 넘치면 줄이고, 넘치지 않으면 그대로 둔다 (절대 확장하지 않음)
                    var sd = rt.sizeDelta;
                    if (affectX)
                        sd.x = Mathf.Min(sd.x, safeW);
                    if (affectY)
                        sd.y = Mathf.Min(sd.y, safeH);
                    rt.sizeDelta = sd;

                    // 2) 위치 보정: 크기 반영 후 중심을 SafeArea 내부로만 이동 (pivot/anchor 고려)
                    float w = rt.rect.width;
                    float h = rt.rect.height;

                    float anchorX = rt.anchorMin.x; // non-stretch: min==max
                    float anchorY = rt.anchorMin.y;

                    var ap = rt.anchoredPosition;

                    if (affectX)
                    {
                        // 현재 중심 X (캔버스 좌표)
                        float centerX = anchorX * canvasSize.x + ap.x + (0.5f - rt.pivot.x) * w;

                        // 허용 중심 범위
                        float minCX = safeLeft + 0.5f * w;
                        float maxCX = safeRight - 0.5f * w;

                        // 중심을 경계 내로만 이동 (범위가 뒤집히더라도 Clamp가 맞춰줌)
                        float clamped = Mathf.Clamp(centerX, minCX, maxCX);
                        ap.x += (clamped - centerX);
                    }

                    if (affectY)
                    {
                        float centerY = anchorY * canvasSize.y + ap.y + (0.5f - rt.pivot.y) * h;

                        float minCY = safeBottom + 0.5f * h;
                        float maxCY = safeTop - 0.5f * h;

                        float clamped = Mathf.Clamp(centerY, minCY, maxCY);
                        ap.y += (clamped - centerY);
                    }

                    rt.anchoredPosition = ap;
                }
            }
            finally
            {
                isApplied = false;
            }
        }
    }
}