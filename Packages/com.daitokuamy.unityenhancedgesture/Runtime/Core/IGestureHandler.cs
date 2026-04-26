using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ジェスチャーハンドラーの共通契約
    /// </summary>
    public interface IGestureHandler {
        /// <summary>
        /// 候補選別時の優先度
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 有効状態かどうか
        /// </summary>
        bool IsActiveAndEnabled { get; }

        /// <summary>
        /// 指定座標でイベント対象になるかどうかを判定
        /// </summary>
        /// <param name="screenPosition">画面座標</param>
        /// <returns>候補になる場合は true</returns>
        bool CanHandle(Vector2 screenPosition);
    }
}
