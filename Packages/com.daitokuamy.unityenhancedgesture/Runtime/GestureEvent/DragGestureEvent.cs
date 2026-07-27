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

        /// <summary>
        /// 末尾静止時間 50ms、最小移動量 1px として直近のサンプルから速度を推定
        /// </summary>
        /// <param name="windowSeconds">速度推定に使用する最大時間範囲</param>
        /// <param name="velocity">推定速度</param>
        /// <returns>有効なサンプルから推定できた場合は true</returns>
        public bool TryGetRecentVelocity(float windowSeconds, out Vector2 velocity) {
            return TryGetRecentVelocity(windowSeconds, 0.05f, 1.0f, out velocity);
        }

        /// <summary>
        /// 直近のサンプルから速度を推定
        /// </summary>
        /// <param name="windowSeconds">速度推定に使用する最大時間範囲</param>
        /// <param name="maximumStationaryDuration">速度をゼロとみなす末尾静止時間</param>
        /// <param name="minimumMovement">移動とみなす最小距離</param>
        /// <param name="velocity">推定速度</param>
        /// <returns>有効なサンプルから推定できた場合は true</returns>
        public bool TryGetRecentVelocity(
            float windowSeconds,
            float maximumStationaryDuration,
            float minimumMovement,
            out Vector2 velocity) {
            velocity = Vector2.zero;

            if (Samples == null
                || Samples.Length < 2
                || windowSeconds <= Mathf.Epsilon
                || maximumStationaryDuration < 0.0f
                || minimumMovement < 0.0f) {
                return false;
            }

            var lastIndex = Samples.Length - 1;
            var lastSample = Samples[lastIndex];
            var minimumMovementSquared = minimumMovement * minimumMovement;
            var movementEndIndex = lastIndex;

            while (movementEndIndex > 0
                && (lastSample.Position - Samples[movementEndIndex - 1].Position).sqrMagnitude <= minimumMovementSquared) {
                movementEndIndex--;
            }

            var movementEndSample = Samples[movementEndIndex];
            var stationaryDuration = lastSample.ElapsedTime - movementEndSample.ElapsedTime;

            if (stationaryDuration < 0.0f) {
                return false;
            }

            if (stationaryDuration > maximumStationaryDuration) {
                return true;
            }

            for (var i = movementEndIndex - 1; i >= 0; i--) {
                var sample = Samples[i];
                var elapsedTime = movementEndSample.ElapsedTime - sample.ElapsedTime;

                if (elapsedTime > windowSeconds) {
                    break;
                }

                if (elapsedTime <= Mathf.Epsilon
                    || (movementEndSample.Position - sample.Position).sqrMagnitude <= minimumMovementSquared) {
                    continue;
                }

                velocity = (movementEndSample.Position - sample.Position) / elapsedTime;
            }

            return true;
        }
    }
}
