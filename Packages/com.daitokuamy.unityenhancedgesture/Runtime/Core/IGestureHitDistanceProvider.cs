using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// 候補選別時にヒット距離を提供する契約
    /// </summary>
    public interface IGestureHitDistanceProvider {
        /// <summary>
        /// 指定座標でイベント対象になる場合のヒット距離を取得
        /// </summary>
        /// <param name="screenPosition">画面座標</param>
        /// <param name="eventCamera">座標判定に使用するカメラ</param>
        /// <param name="distance">ヒット距離</param>
        /// <returns>候補になる場合は true</returns>
        bool TryGetHandleDistance(Vector2 screenPosition, Camera eventCamera, out float distance);
    }
}
