using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEnhancedGesture {
    /// <summary>
    /// 開始地点から対象ハンドラーを解決する補助クラス
    /// </summary>
    internal static class GestureRaycastResolver {
        private static readonly List<RaycastResult> s_RaycastResults = new();
        private static readonly List<GestureHandlerBase> s_HandlerBuffer = new();

        /// <summary>
        /// 開始地点の所有対象を解決
        /// </summary>
        /// <param name="entriesByRectTransform">登録対象一覧</param>
        /// <param name="screenPosition">画面座標</param>
        /// <param name="targetEntry">解決結果</param>
        /// <returns>解決に成功した場合は true</returns>
        public static bool TryResolveTarget(
            IReadOnlyDictionary<RectTransform, GestureTargetEntry> entriesByRectTransform,
            Vector2 screenPosition,
            out GestureTargetEntry targetEntry) {
            if (EventSystem.current != null) {
                PointerEventData pointerEventData = new(EventSystem.current) {
                    position = screenPosition,
                };

                s_RaycastResults.Clear();
                EventSystem.current.RaycastAll(pointerEventData, s_RaycastResults);

                foreach (RaycastResult raycastResult in s_RaycastResults) {
                    GestureTargetEntry raycastTargetEntry = FindTargetEntry(entriesByRectTransform, raycastResult.gameObject);

                    if (raycastTargetEntry != null) {
                        targetEntry = raycastTargetEntry;
                        return true;
                    }

                    targetEntry = null;
                    return false;
                }
            }

            return TryResolveFallback(entriesByRectTransform, screenPosition, out targetEntry);
        }

        /// <summary>
        /// レイキャスト対象から登録済みエントリを検索
        /// </summary>
        /// <param name="entriesByRectTransform">登録対象一覧</param>
        /// <param name="gameObject">ヒットした GameObject</param>
        /// <returns>対応する判定対象</returns>
        private static GestureTargetEntry FindTargetEntry(
            IReadOnlyDictionary<RectTransform, GestureTargetEntry> entriesByRectTransform,
            GameObject gameObject) {
            s_HandlerBuffer.Clear();
            gameObject.GetComponentsInParent(true, s_HandlerBuffer);

            foreach (GestureHandlerBase handler in s_HandlerBuffer) {
                if (handler != null
                    && handler.isActiveAndEnabled
                    && entriesByRectTransform.TryGetValue(handler.TargetRectTransform, out GestureTargetEntry targetEntry)) {
                    return targetEntry;
                }
            }

            return null;
        }

        /// <summary>
        /// EventSystem が使えない場合のフォールバック解決
        /// </summary>
        /// <param name="entriesByRectTransform">登録対象一覧</param>
        /// <param name="screenPosition">画面座標</param>
        /// <param name="targetEntry">解決結果</param>
        /// <returns>解決に成功した場合は true</returns>
        private static bool TryResolveFallback(
            IReadOnlyDictionary<RectTransform, GestureTargetEntry> entriesByRectTransform,
            Vector2 screenPosition,
            out GestureTargetEntry targetEntry) {
            targetEntry = null;

            foreach (GestureTargetEntry candidate in entriesByRectTransform.Values) {
                Camera eventCamera = GetEventCamera(candidate.RectTransform);

                if (!RectTransformUtility.RectangleContainsScreenPoint(candidate.RectTransform, screenPosition, eventCamera)) {
                    continue;
                }

                if (targetEntry == null || CompareOrder(candidate.RectTransform, targetEntry.RectTransform) > 0) {
                    targetEntry = candidate;
                }
            }

            return targetEntry != null;
        }

        /// <summary>
        /// RectTransform 判定に使用するカメラを取得
        /// </summary>
        /// <param name="rectTransform">対象 RectTransform</param>
        /// <returns>使用カメラ</returns>
        private static Camera GetEventCamera(RectTransform rectTransform) {
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();

            if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                return null;
            }

            return canvas.worldCamera;
        }

        /// <summary>
        /// 2 つの RectTransform の前後関係を比較
        /// </summary>
        /// <param name="left">比較対象 1</param>
        /// <param name="right">比較対象 2</param>
        /// <returns>left が前面なら正値</returns>
        private static int CompareOrder(RectTransform left, RectTransform right) {
            Canvas leftCanvas = left.GetComponentInParent<Canvas>();
            Canvas rightCanvas = right.GetComponentInParent<Canvas>();

            if (leftCanvas != null && rightCanvas != null && leftCanvas.rootCanvas != rightCanvas.rootCanvas) {
                int sortingOrderComparison = leftCanvas.rootCanvas.sortingOrder.CompareTo(rightCanvas.rootCanvas.sortingOrder);

                if (sortingOrderComparison != 0) {
                    return sortingOrderComparison;
                }
            }

            return CompareTransformOrder(left, right);
        }

        /// <summary>
        /// Transform の階層順で前後関係を比較
        /// </summary>
        /// <param name="left">比較対象 1</param>
        /// <param name="right">比較対象 2</param>
        /// <returns>left が後方でなければ正値</returns>
        private static int CompareTransformOrder(Transform left, Transform right) {
            List<Transform> leftParents = GetTransformPath(left);
            List<Transform> rightParents = GetTransformPath(right);
            int commonLength = Mathf.Min(leftParents.Count, rightParents.Count);

            for (int i = 0; i < commonLength; i++) {
                if (leftParents[i] == rightParents[i]) {
                    continue;
                }

                return leftParents[i].GetSiblingIndex().CompareTo(rightParents[i].GetSiblingIndex());
            }

            return leftParents.Count.CompareTo(rightParents.Count);
        }

        /// <summary>
        /// ルートから対象までの Transform パスを取得
        /// </summary>
        /// <param name="transform">対象 Transform</param>
        /// <returns>Transform パス</returns>
        private static List<Transform> GetTransformPath(Transform transform) {
            List<Transform> path = new();
            Transform current = transform;

            while (current != null) {
                path.Insert(0, current);
                current = current.parent;
            }

            return path;
        }
    }
}
