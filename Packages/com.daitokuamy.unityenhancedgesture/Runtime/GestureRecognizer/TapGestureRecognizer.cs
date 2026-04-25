using System.Collections.Generic;
using UnityEngine;
using InputTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace UnityEnhancedGesture {
    /// <summary>
    /// タップ成立を判定する内部認識器
    /// </summary>
    internal sealed class TapGestureRecognizer : GestureRecognizerBase {
        /// <summary>
        /// 保留中のタップ情報
        /// </summary>
        private sealed class PendingTap {
            /// <summary>
            /// 保留中タップ情報を初期化
            /// </summary>
            /// <param name="handler">通知先ハンドラー</param>
            /// <param name="startPosition">開始位置</param>
            /// <param name="position">完了位置</param>
            /// <param name="duration">継続時間</param>
            /// <param name="completedTime">完了時刻</param>
            public PendingTap(TapGestureHandler handler, Vector2 startPosition, Vector2 position, float duration, float completedTime) {
                Handler = handler;
                StartPosition = startPosition;
                Position = position;
                Duration = duration;
                CompletedTime = completedTime;
            }

            /// <summary>通知先ハンドラー</summary>
            public TapGestureHandler Handler { get; }
            /// <summary>開始位置</summary>
            public Vector2 StartPosition { get; }
            /// <summary>完了位置</summary>
            public Vector2 Position { get; }
            /// <summary>継続時間</summary>
            public float Duration { get; }
            /// <summary>完了時刻</summary>
            public float CompletedTime { get; }
        }

        private readonly List<PendingTap> _pendingTaps = new();

        /// <inheritdoc/>
        public override GestureRecognitionType RecognitionType => GestureRecognitionType.Tap;

        /// <summary>
        /// ダブルタップ待機中のタップを確定
        /// </summary>
        /// <param name="currentTime">現在時刻</param>
        public void FlushPending(float currentTime) {
            for (int i = _pendingTaps.Count - 1; i >= 0; i--) {
                PendingTap pendingTap = _pendingTaps[i];

                if (pendingTap.Handler == null || !pendingTap.Handler.isActiveAndEnabled) {
                    _pendingTaps.RemoveAt(i);
                    continue;
                }

                if (currentTime - pendingTap.CompletedTime < pendingTap.Handler.DoubleTapTime) {
                    continue;
                }

                pendingTap.Handler.RaiseTap(CreateEvent(
                    TapGestureKind.SingleTap,
                    GestureEventPhase.Completed,
                    pendingTap.StartPosition,
                    pendingTap.Position,
                    pendingTap.Duration,
                    1));
                _pendingTaps.RemoveAt(i);
            }
        }

        /// <summary>
        /// タップ完了を判定
        /// </summary>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="inputSnapshot">入力スナップショット</param>
        /// <returns>タップまたはダブルタップとして処理した場合は true</returns>
        public bool TryCompleteTap(TapGestureHandler handler, GestureInputSnapshot inputSnapshot) {
            if (inputSnapshot.TouchCount != 1 || inputSnapshot.PrimaryTravelDistance > handler.MaxTapMovement) {
                return false;
            }

            InputTouch touch = inputSnapshot.PrimaryTouch;
            float duration = (float)(touch.time - touch.startTime);

            if (duration > handler.MaxTapDuration) {
                return false;
            }

            if (handler.EnableDoubleTap) {
                int pendingTapIndex = FindPendingTap(handler, touch.screenPosition, (float)touch.time);

                if (pendingTapIndex >= 0) {
                    _pendingTaps.RemoveAt(pendingTapIndex);
                    handler.RaiseDoubleTap(CreateEvent(
                        TapGestureKind.DoubleTap,
                        GestureEventPhase.Completed,
                        touch.startScreenPosition,
                        touch.screenPosition,
                        duration,
                        2));
                    return true;
                }

                _pendingTaps.Add(new PendingTap(handler, touch.startScreenPosition, touch.screenPosition, duration, (float)touch.time));
                return true;
            }

            handler.RaiseTap(CreateEvent(
                TapGestureKind.SingleTap,
                GestureEventPhase.Completed,
                touch.startScreenPosition,
                touch.screenPosition,
                duration,
                1));
            return true;
        }

        /// <summary>
        /// タップキャンセル通知を送出
        /// </summary>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="inputSnapshot">入力スナップショット</param>
        public void RaiseCanceled(TapGestureHandler handler, GestureInputSnapshot inputSnapshot) {
            InputTouch touch = inputSnapshot.PrimaryTouch;
            handler.RaiseCancelTap(CreateEvent(
                TapGestureKind.SingleTap,
                GestureEventPhase.Canceled,
                touch.startScreenPosition,
                touch.screenPosition,
                (float)(touch.time - touch.startTime),
                0));
        }

        /// <summary>
        /// ダブルタップ候補となる保留タップを検索
        /// </summary>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="position">現在位置</param>
        /// <param name="currentTime">現在時刻</param>
        /// <returns>一致したインデックス。未検出の場合は -1</returns>
        private int FindPendingTap(TapGestureHandler handler, Vector2 position, float currentTime) {
            for (int i = _pendingTaps.Count - 1; i >= 0; i--) {
                PendingTap pendingTap = _pendingTaps[i];

                if (pendingTap.Handler != handler) {
                    continue;
                }

                if (currentTime - pendingTap.CompletedTime > handler.DoubleTapTime) {
                    _pendingTaps.RemoveAt(i);
                    continue;
                }

                if (Vector2.Distance(pendingTap.Position, position) <= handler.MaxTapMovement) {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// タップ通知用イベント引数を生成
        /// </summary>
        /// <param name="kind">タップ種別</param>
        /// <param name="phase">通知フェーズ</param>
        /// <param name="startPosition">開始位置</param>
        /// <param name="position">通知位置</param>
        /// <param name="duration">継続時間</param>
        /// <param name="tapCount">タップ回数</param>
        /// <returns>イベント引数</returns>
        private static TapGestureEvent CreateEvent(
            TapGestureKind kind,
            GestureEventPhase phase,
            Vector2 startPosition,
            Vector2 position,
            float duration,
            int tapCount) {
            return new TapGestureEvent(kind, phase, startPosition, position, duration, tapCount);
        }
    }
}
