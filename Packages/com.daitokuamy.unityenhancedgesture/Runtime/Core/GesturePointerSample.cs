using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ポインター入力の時系列サンプル
    /// </summary>
    public readonly struct GesturePointerSample {
        /// <summary>サンプル位置</summary>
        public Vector2 Position { get; }
        /// <summary>開始からの経過時間</summary>
        public float ElapsedTime { get; }

        /// <summary>
        /// サンプルを生成
        /// </summary>
        /// <param name="position">サンプル位置</param>
        /// <param name="elapsedTime">開始からの経過時間</param>
        public GesturePointerSample(Vector2 position, float elapsedTime) {
            Position = position;
            ElapsedTime = elapsedTime;
        }
    }
}
