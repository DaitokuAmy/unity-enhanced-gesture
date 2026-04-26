using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ジェスチャー認識器の共通契約
    /// </summary>
    internal interface IGestureRecognizer {
        /// <summary>
        /// 指定ハンドラーからトラックを生成できるかどうかを判定
        /// </summary>
        /// <param name="handler">対象ハンドラー</param>
        /// <returns>生成可能な場合は true</returns>
        bool CanCreateTrack(IGestureHandler handler);

        /// <summary>
        /// 指定ハンドラーに対応する入力トラックを生成
        /// </summary>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="pointerId">ポインター ID</param>
        /// <param name="startPosition">開始位置</param>
        /// <param name="startTime">開始時刻</param>
        /// <returns>生成したトラック</returns>
        IGestureTrack CreateTrack(IGestureHandler handler, int pointerId, Vector2 startPosition, float startTime);

        /// <summary>
        /// 入力トラックを更新
        /// </summary>
        /// <param name="track">対象トラック</param>
        /// <param name="input">現在の入力情報</param>
        void ProcessTrack(IGestureTrack track, GesturePointerInput input);
    }
}
