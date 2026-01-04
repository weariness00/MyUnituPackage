using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Weariness.Util
{
    public enum ObjectGridSortType
    {
        LeftUpForward,      // (-1, 1, 1)
        LeftDownForward,    // (-1, -1, 1)
        LeftUpBack,         // (-1, 1, -1)
        LeftDownBack,       // (-1, -1, -1)

        RightUpForward,     // (1, 1, 1)
        RightDownForward,   // (1, -1, 1)
        RightUpBack,        // (1, 1, -1)
        RightDownBack,      // (1, -1, -1)
    }

    public partial class ObjectGrid : MonoBehaviour
    {
        public RectOffset padding;
        public Vector3Int gridCount = Vector3Int.one;
        public Vector3 cellBoxSize = Vector3.one;
        public Vector3 spacing;
        [Space] public Vector3 cellAngle;
        public Vector3 cellScale = Vector3.one;

        public bool isUseX = true;
        public bool isUseY = true;
        public bool isUseZ = true;
        public ObjectGridSortType sortType;

        [Space] public bool isUpdate = true;

        public void FixedUpdate()
        {
            if (isUpdate)
                ForceUpdate();
        }

        private void OnDrawGizmosSelected()
        {
            DrawGridGizmos();
        }

        public void ForceUpdate()
        {
            Quaternion rotation = Quaternion.Euler(cellAngle);
        
            var roCellBoxSize = rotation * cellBoxSize;
            roCellBoxSize.x *= cellScale.x;
            roCellBoxSize.y *= cellScale.y;
            roCellBoxSize.z *= cellScale.z;
        
            var roSpacing = rotation * spacing;
            roSpacing.x *= cellScale.x;
            roSpacing.y *= cellScale.y;
            roSpacing.z *= cellScale.z;
        
            var children = GetAllChildren();
        
            int capacity = Mathf.Max(0, gridCount.x) * Mathf.Max(0, gridCount.y) * Mathf.Max(0, gridCount.z);
            int placeCount = capacity > 0 ? Mathf.Min(children.Count, capacity) : 0;
        
            GetSortAxis(sortType, out int xDir, out int yDir, out int zDir);
            bool reverseX = xDir > 0; // Right*
            bool reverseY = yDir < 0; // *Down*
            bool reverseZ = zDir < 0; // *Back*
        
            // ✅ padding 적용 (RectOffset은 x/y만 의미가 자연스럽고, z는 대응이 없어 0으로 둠)
            int padL = padding != null ? padding.left : 0;
            int padR = padding != null ? padding.right : 0;
            int padT = padding != null ? padding.top : 0;
            int padB = padding != null ? padding.bottom : 0;
        
            Vector3 padOffset = new Vector3(
                reverseX ? -padR : padL,   // Right 시작이면 오른쪽 여백만큼 왼쪽으로 밀기
                reverseY ? -padT : padB,   // Top 시작이면 위쪽 여백만큼 아래로 밀기
                0f
            );
        
            for (int i = 0; i < placeCount; i++)
            {
                var child = children[i];
        
                // 기본 인덱스: x 빠름 -> z -> y
                int x0 = (i % gridCount.x);
                int z0 = (i / gridCount.x) % gridCount.z;
                int y0 = (i / (gridCount.x * gridCount.z)) % gridCount.y;
        
                // sortType에 따라 인덱스 뒤집기
                int xIndex = reverseX ? (gridCount.x - 1 - x0) : x0;
                int yIndex = reverseY ? (gridCount.y - 1 - y0) : y0;
                int zIndex = reverseZ ? (gridCount.z - 1 - z0) : z0;
        
                // 기존 좌표식 유지
                var x = isUseX ? (-roCellBoxSize.x + (roCellBoxSize.x + roSpacing.x) * xIndex) : 0;
                var y = isUseY ? ((roCellBoxSize.y + roSpacing.y) * yIndex) : 0;
                var z = isUseZ ? (roCellBoxSize.z + (-roCellBoxSize.z + roSpacing.z) * zIndex) : 0;
        
                child.localPosition = new Vector3(x, y, z) + padOffset;
                child.localEulerAngles = cellAngle;
                child.localScale = cellScale;
            }
        }

        private List<Transform> GetAllChildren()
        {
            var list = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var t = transform.GetChild(i);
                if (t.gameObject.activeSelf)
                    list.Add(t);
            }
            return list;
        }
        
        private void DrawGridGizmos()
        {
            // 유효성
            int gx = Mathf.Max(1, gridCount.x);
            int gy = Mathf.Max(1, gridCount.y);
            int gz = Mathf.Max(1, gridCount.z);

            Quaternion rotation = Quaternion.Euler(cellAngle);

            // ForceUpdate와 동일한 계산
            Vector3 roCellBoxSize = rotation * cellBoxSize;
            roCellBoxSize.x *= cellScale.x;
            roCellBoxSize.y *= cellScale.y;
            roCellBoxSize.z *= cellScale.z;

            Vector3 roSpacing = rotation * spacing;
            roSpacing.x *= cellScale.x;
            roSpacing.y *= cellScale.y;
            roSpacing.z *= cellScale.z;

            GetSortAxis(sortType, out int xDir, out int yDir, out int zDir);
            bool reverseX = xDir > 0;
            bool reverseY = yDir < 0;
            bool reverseZ = zDir < 0;

            int padL = padding != null ? padding.left : 0;
            int padR = padding != null ? padding.right : 0;
            int padT = padding != null ? padding.top : 0;
            int padB = padding != null ? padding.bottom : 0;

            Vector3 padOffsetLocal = new Vector3(
                reverseX ? -padR : padL,
                reverseY ? -padT : padB,
                0f
            );

            // 그리드의 모든 셀 "위치(ForceUpdate의 child.localPosition)" 기준으로 bounds 계산
            bool boundsInit = false;
            Bounds worldBounds = default;

            // 셀 박스 표시용(너무 많으면 느릴 수 있어서 일부만 그리고 싶으면 샘플링도 가능)
            for (int y = 0; y < gy; y++)
            for (int z = 0; z < gz; z++)
            for (int x = 0; x < gx; x++)
            {
                int xIndex = reverseX ? (gx - 1 - x) : x;
                int yIndex = reverseY ? (gy - 1 - y) : y;
                int zIndex = reverseZ ? (gz - 1 - z) : z;

                float lx = isUseX ? (-roCellBoxSize.x + (roCellBoxSize.x + roSpacing.x) * xIndex) : 0;
                float ly = isUseY ? ((roCellBoxSize.y + roSpacing.y) * yIndex) : 0;
                float lz = isUseZ ? (roCellBoxSize.z + (-roCellBoxSize.z + roSpacing.z) * zIndex) : 0;

                Vector3 localPos = new Vector3(lx, ly, lz) + padOffsetLocal;
                Vector3 worldPos = transform.TransformPoint(localPos);

                // 셀 하나의 "대략적 크기"를 world bounds에 반영
                // (ForceUpdate 로직은 사실상 축별 크기가 roCellBoxSize의 절대값에 가까우므로 그 기준으로 캡슐화)
                Vector3 cellExt = new Vector3(
                    Mathf.Abs(roCellBoxSize.x),
                    Mathf.Abs(roCellBoxSize.y),
                    Mathf.Abs(roCellBoxSize.z)
                );

                // world space에서 AABB로 포함(회전은 무시하고 AABB로 포함)
                var cellBounds = new Bounds(worldPos, cellExt);

                if (!boundsInit)
                {
                    worldBounds = cellBounds;
                    boundsInit = true;
                }
                else
                {
                    worldBounds.Encapsulate(cellBounds);
                }

#if UNITY_EDITOR
                Gizmos.DrawSphere(worldPos, HandleUtility.GetHandleSize(worldPos) * 0.02f);
#endif

                // 셀 박스는 "로컬 회전 cellAngle"을 적용한 것처럼 보이게 하고 싶으면
                // Handles.matrix로 회전 박스를 그릴 수 있음. 여기선 간단히 AABB wire cube로 표시.
                Gizmos.DrawWireCube(worldPos, cellExt);
            }

            if (!boundsInit) return;

            // 그리드 전체 외곽
            Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);

#if UNITY_EDITOR
            // 라벨(선택사항)
            Handles.Label(worldBounds.center, $"GridBounds\n{worldBounds.size}");
#endif
        }
    }
}
