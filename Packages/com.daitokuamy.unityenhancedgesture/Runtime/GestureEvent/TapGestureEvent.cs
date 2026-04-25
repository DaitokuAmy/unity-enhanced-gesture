using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// タップ系イベント通知に使用する引数
    /// </summary>
    public readonly struct TapGestureEvent {
        /// <summary>
        /// タップイベント引数を初期化
        /// </summary>
        /// <param name="kind">タップ種別</param>
        /// <param name="phase">通知フェーズ</param>
        /// <param name="startPosition">開始位置</param>
        /// <param name="position">終了または通知位置</param>
        /// <param name="duration">開始からの経過時間</param>
        /// <param name="tapCount">タップ回数</param>
        public TapGestureEvent(
            TapGestureKind kind,
            GestureEventPhase phase,
            Vector2 startPosition,
            Vector2 position,
            float duration,
            int tapCount) {
            Kind = kind;
            Phase = phase;
            StartPosition = startPosition;
            Position = position;
            Duration = duration;
            TapCount = tapCount;
        }

        /// <summary>タップ種別</summary>
        public TapGestureKind Kind { get; }
        /// <summary>通知フェーズ</summary>
        public GestureEventPhase Phase { get; }
        /// <summary>開始位置</summary>
        public Vector2 StartPosition { get; }
        /// <summary>終了または通知位置</summary>
        public Vector2 Position { get; }
        /// <summary>開始からの経過時間</summary>
        public float Duration { get; }
        /// <summary>タップ回数</summary>
        public int TapCount { get; }
    }
}
