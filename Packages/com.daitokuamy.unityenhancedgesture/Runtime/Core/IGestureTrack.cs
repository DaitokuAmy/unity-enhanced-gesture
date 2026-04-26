using System.Collections.Generic;

namespace UnityEnhancedGesture {
    /// <summary>
    /// 進行中入力トラックの共通契約
    /// </summary>
    internal interface IGestureTrack {
        /// <summary>
        /// 配送先ハンドラー
        /// </summary>
        IGestureHandler Handler { get; }

        /// <summary>
        /// 処理を担当する認識器
        /// </summary>
        IGestureRecognizer Recognizer { get; }

        /// <summary>
        /// 対応するポインター ID
        /// </summary>
        IReadOnlyList<int> PointerIds { get; }

        /// <summary>
        /// この入力系列に紐づくイベントカメラ
        /// </summary>
        UnityEngine.Camera EventCamera { get; }

        /// <summary>
        /// 処理完了済みかどうか
        /// </summary>
        bool IsCompleted { get; }
    }
}
