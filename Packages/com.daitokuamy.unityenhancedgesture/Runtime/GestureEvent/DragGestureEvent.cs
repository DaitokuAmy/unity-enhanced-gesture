using System;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ドラッグイベント通知に使用する引数
    /// </summary>
    public readonly struct DragGestureEvent {
        /// <summary>
        /// ドラッグイベント引数を生成
        /// </summary>
        /// <param name="phase">イベントフェーズ</param>
        /// <param name="startMode">開始方式</param>
        /// <param name="startPosition">開始位置</param>
        /// <param name="position">現在位置</param>
        /// <param name="delta">前回イベントからの差分量</param>
        /// <param name="totalDelta">開始位置からの差分量</param>
        /// <param name="samples">開始から現在までの時系列サンプル列</param>
        /// <param name="duration">開始からの経過時間</param>
        /// <param name="activePointerCount">現在有効なポインター数</param>
        /// <param name="eventCamera">イベントに紐づくカメラ</param>
        public DragGestureEvent(
            GestureEventPhase phase,
            DragGestureStartMode startMode,
            Vector2 startPosition,
            Vector2 position,
            Vector2 delta,
            Vector2 totalDelta,
            GesturePointerSample[] samples,
            float duration,
            int activePointerCount,
            Camera eventCamera) {
            Phase = phase;
            StartMode = startMode;
            StartPosition = startPosition;
            Position = position;
            Delta = delta;
            TotalDelta = totalDelta;
            Samples = samples ?? Array.Empty<GesturePointerSample>();
            Duration = duration;
            ActivePointerCount = activePointerCount;
            EventCamera = eventCamera;
        }

        /// <summary>イベントフェーズ</summary>
        public GestureEventPhase Phase { get; }
        /// <summary>開始方式</summary>
        public DragGestureStartMode StartMode { get; }
        /// <summary>ドラッグ開始位置</summary>
        public Vector2 StartPosition { get; }
        /// <summary>現在位置</summary>
        public Vector2 Position { get; }
        /// <summary>前回イベントからの差分量</summary>
        public Vector2 Delta { get; }
        /// <summary>開始位置からの差分量</summary>
        public Vector2 TotalDelta { get; }
        /// <summary>開始から現在までの時系列サンプル列</summary>
        public GesturePointerSample[] Samples { get; }
        /// <summary>開始からの経過時間</summary>
        public float Duration { get; }
        /// <summary>現在有効なポインター数</summary>
        public int ActivePointerCount { get; }
        /// <summary>イベントに紐づくカメラ</summary>
        public Camera EventCamera { get; }
    }
}
