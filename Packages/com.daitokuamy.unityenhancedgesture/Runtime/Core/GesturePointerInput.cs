using System;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// 解析済みの単一ポインター入力
    /// </summary>
    internal readonly struct GesturePointerInput {
        /// <summary>
        /// ポインター ID
        /// </summary>
        public int PointerId { get; }

        /// <summary>
        /// 現在フェーズ
        /// </summary>
        public GestureInputPhase Phase { get; }

        /// <summary>
        /// 開始位置
        /// </summary>
        public Vector2 StartPosition { get; }

        /// <summary>
        /// 現在位置
        /// </summary>
        public Vector2 Position { get; }

        /// <summary>
        /// 前回からの差分
        /// </summary>
        public Vector2 Delta { get; }

        /// <summary>
        /// 開始から現在までの時系列サンプル列
        /// </summary>
        public DragGestureSample[] Samples { get; }

        /// <summary>
        /// 開始時刻
        /// </summary>
        public float StartTime { get; }

        /// <summary>
        /// 現在時刻
        /// </summary>
        public float Time { get; }

        /// <summary>
        /// 入力データを初期化
        /// </summary>
        /// <param name="pointerId">ポインター ID</param>
        /// <param name="phase">現在フェーズ</param>
        /// <param name="startPosition">開始位置</param>
        /// <param name="position">現在位置</param>
        /// <param name="delta">前回からの差分</param>
        /// <param name="samples">開始から現在までの時系列サンプル列</param>
        /// <param name="startTime">開始時刻</param>
        /// <param name="time">現在時刻</param>
        public GesturePointerInput(
            int pointerId,
            GestureInputPhase phase,
            Vector2 startPosition,
            Vector2 position,
            Vector2 delta,
            DragGestureSample[] samples,
            float startTime,
            float time) {
            PointerId = pointerId;
            Phase = phase;
            StartPosition = startPosition;
            Position = position;
            Delta = delta;
            Samples = samples ?? Array.Empty<DragGestureSample>();
            StartTime = startTime;
            Time = time;
        }
    }
}
