using System;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ドラッグ系イベント通知に使用する引数
    /// </summary>
    public readonly struct DragGestureEvent {
        /// <summary>
        /// ドラッグイベント引数を初期化
        /// </summary>
        /// <param name="phase">通知フェーズ</param>
        /// <param name="startPosition">開始位置</param>
        /// <param name="position">現在位置</param>
        /// <param name="delta">前回通知からの差分</param>
        /// <param name="totalDelta">開始位置からの差分</param>
        /// <param name="positions">開始から現在までの座標列</param>
        /// <param name="duration">開始からの経過時間</param>
        public DragGestureEvent(
            GestureEventPhase phase,
            Vector2 startPosition,
            Vector2 position,
            Vector2 delta,
            Vector2 totalDelta,
            Vector2[] positions,
            float duration) {
            Phase = phase;
            StartPosition = startPosition;
            Position = position;
            Delta = delta;
            TotalDelta = totalDelta;
            Positions = positions ?? Array.Empty<Vector2>();
            Duration = duration;
        }

        /// <summary>通知フェーズ</summary>
        public GestureEventPhase Phase { get; }
        /// <summary>ドラッグ開始位置</summary>
        public Vector2 StartPosition { get; }
        /// <summary>現在位置</summary>
        public Vector2 Position { get; }
        /// <summary>前回通知からの差分</summary>
        public Vector2 Delta { get; }
        /// <summary>開始位置からの差分</summary>
        public Vector2 TotalDelta { get; }
        /// <summary>開始から現在までの座標列</summary>
        public Vector2[] Positions { get; }
        /// <summary>開始からの経過時間</summary>
        public float Duration { get; }
    }
}
