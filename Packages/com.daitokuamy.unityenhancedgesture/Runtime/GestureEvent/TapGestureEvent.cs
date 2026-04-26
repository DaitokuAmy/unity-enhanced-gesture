using System;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// タップ系イベント通知に使用する引数
    /// </summary>
    public readonly struct TapGestureEvent {
        /// <summary>
        /// タップイベント引数を生成
        /// </summary>
        /// <param name="type">タップ種別</param>
        /// <param name="tapCount">タップ回数</param>
        /// <param name="firstTapPosition">最初のタップ位置</param>
        /// <param name="startPosition">現在タップの開始位置</param>
        /// <param name="position">現在タップの終了位置</param>
        /// <param name="samples">現在タップのサンプル列</param>
        /// <param name="duration">現在タップの継続時間</param>
        /// <param name="interval">前回タップからの間隔</param>
        /// <param name="eventCamera">イベントに紐づくカメラ</param>
        public TapGestureEvent(
            TapGestureType type,
            int tapCount,
            Vector2 firstTapPosition,
            Vector2 startPosition,
            Vector2 position,
            GesturePointerSample[] samples,
            float duration,
            float interval,
            Camera eventCamera) {
            Type = type;
            TapCount = tapCount;
            FirstTapPosition = firstTapPosition;
            StartPosition = startPosition;
            Position = position;
            Samples = samples ?? Array.Empty<GesturePointerSample>();
            Duration = duration;
            Interval = interval;
            EventCamera = eventCamera;
        }

        /// <summary>タップ種別</summary>
        public TapGestureType Type { get; }
        /// <summary>タップ回数</summary>
        public int TapCount { get; }
        /// <summary>最初のタップ位置</summary>
        public Vector2 FirstTapPosition { get; }
        /// <summary>現在タップの開始位置</summary>
        public Vector2 StartPosition { get; }
        /// <summary>現在タップの終了位置</summary>
        public Vector2 Position { get; }
        /// <summary>現在タップのサンプル列</summary>
        public GesturePointerSample[] Samples { get; }
        /// <summary>現在タップの継続時間</summary>
        public float Duration { get; }
        /// <summary>前回タップからの間隔</summary>
        public float Interval { get; }
        /// <summary>イベントに紐づくカメラ</summary>
        public Camera EventCamera { get; }
    }
}
