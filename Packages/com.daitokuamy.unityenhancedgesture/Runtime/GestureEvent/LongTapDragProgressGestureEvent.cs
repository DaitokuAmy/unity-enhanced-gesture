using System;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ロングタップドラッグ進捗通知に使用する引数
    /// </summary>
    public readonly struct LongTapDragProgressGestureEvent {
        /// <summary>
        /// ロングタップドラッグ進捗イベント引数を生成
        /// </summary>
        /// <param name="phase">イベントフェーズ</param>
        /// <param name="startPosition">開始位置</param>
        /// <param name="position">現在位置</param>
        /// <param name="samples">開始から現在までの時系列サンプル列</param>
        /// <param name="duration">開始からの経過時間</param>
        /// <param name="requiredDuration">ロングタップドラッグ成立までの待機時間</param>
        /// <param name="maxMovement">ロングタップドラッグ成立までの許容移動量</param>
        /// <param name="eventCamera">イベントに紐づくカメラ</param>
        public LongTapDragProgressGestureEvent(
            GestureEventPhase phase,
            Vector2 startPosition,
            Vector2 position,
            GesturePointerSample[] samples,
            float duration,
            float requiredDuration,
            float maxMovement,
            Camera eventCamera) {
            Phase = phase;
            StartPosition = startPosition;
            Position = position;
            Samples = samples ?? Array.Empty<GesturePointerSample>();
            Duration = duration;
            RequiredDuration = requiredDuration;
            MaxMovement = maxMovement;
            EventCamera = eventCamera;
            Progress = requiredDuration <= Mathf.Epsilon ? 1.0f : Mathf.Clamp01(duration / requiredDuration);
        }

        /// <summary>イベントフェーズ</summary>
        public GestureEventPhase Phase { get; }
        /// <summary>ロングタップドラッグ成立までの進捗率</summary>
        public float Progress { get; }
        /// <summary>開始位置</summary>
        public Vector2 StartPosition { get; }
        /// <summary>現在位置</summary>
        public Vector2 Position { get; }
        /// <summary>開始から現在までの時系列サンプル列</summary>
        public GesturePointerSample[] Samples { get; }
        /// <summary>開始からの経過時間</summary>
        public float Duration { get; }
        /// <summary>ロングタップドラッグ成立までの待機時間</summary>
        public float RequiredDuration { get; }
        /// <summary>ロングタップドラッグ成立までの許容移動量</summary>
        public float MaxMovement { get; }
        /// <summary>イベントに紐づくカメラ</summary>
        public Camera EventCamera { get; }
    }
}
