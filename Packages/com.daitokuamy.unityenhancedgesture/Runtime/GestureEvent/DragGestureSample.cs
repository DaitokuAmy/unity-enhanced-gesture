using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ドラッグ軌跡を表す時系列サンプル
    /// </summary>
    public readonly struct DragGestureSample {
        /// <summary>
        /// サンプル位置
        /// </summary>
        public Vector2 Position { get; }

        /// <summary>
        /// 開始からの経過時間
        /// </summary>
        public float ElapsedTime { get; }

        /// <summary>
        /// サンプルを初期化
        /// </summary>
        /// <param name="position">サンプル位置</param>
        /// <param name="elapsedTime">開始からの経過時間</param>
        public DragGestureSample(Vector2 position, float elapsedTime) {
            Position = position;
            ElapsedTime = elapsedTime;
        }
    }
}
