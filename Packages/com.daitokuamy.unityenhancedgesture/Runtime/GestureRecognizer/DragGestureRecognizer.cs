using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ドラッグ入力を解析する認識器
    /// </summary>
    internal sealed class DragGestureRecognizer : IGestureRecognizer {
        /// <summary>
        /// 進行中ドラッグの内部状態
        /// </summary>
        private sealed class DragGestureTrack : IGestureTrack {
            /// <inheritdoc/>
            public IGestureHandler Handler { get; }

            /// <inheritdoc/>
            public IGestureRecognizer Recognizer { get; }

            /// <inheritdoc/>
            public int PointerId { get; }

            /// <inheritdoc/>
            public Camera EventCamera { get; }

            /// <inheritdoc/>
            public bool IsCompleted { get; set; }

            /// <summary>
            /// ドラッグ開始位置
            /// </summary>
            public Vector2 StartPosition { get; }

            /// <summary>
            /// ドラッグ開始時刻
            /// </summary>
            public float StartTime { get; }

            /// <summary>
            /// 開始通知送信済みかどうか
            /// </summary>
            public bool HasBegun { get; set; }

            /// <summary>
            /// トラックを初期化
            /// </summary>
            /// <param name="recognizer">処理担当認識器</param>
            /// <param name="handler">配送対象ハンドラー</param>
            /// <param name="pointerId">ポインター ID</param>
            /// <param name="startPosition">開始位置</param>
            /// <param name="startTime">開始時刻</param>
            /// <param name="eventCamera">イベントに紐づくカメラ</param>
            public DragGestureTrack(IGestureRecognizer recognizer, IDragGestureHandler handler, int pointerId, Vector2 startPosition, float startTime, Camera eventCamera) {
                Recognizer = recognizer;
                Handler = handler;
                PointerId = pointerId;
                StartPosition = startPosition;
                StartTime = startTime;
                EventCamera = eventCamera;
            }
        }

        /// <inheritdoc/>
        public bool CanCreateTrack(IGestureHandler handler) {
            return handler is IDragGestureHandler;
        }

        /// <inheritdoc/>
        public IGestureTrack CreateTrack(IGestureHandler handler, int pointerId, Vector2 startPosition, float startTime, Camera eventCamera) {
            return new DragGestureTrack(this, (IDragGestureHandler)handler, pointerId, startPosition, startTime, eventCamera);
        }

        /// <inheritdoc/>
        public void ProcessTrack(IGestureTrack track, GesturePointerInput input) {
            var dragTrack = (DragGestureTrack)track;
            var dragHandler = (IDragGestureHandler)dragTrack.Handler;
            var sentDragEvent = false;
            var totalDistance = Vector2.Distance(dragTrack.StartPosition, input.Position);

            if (!dragTrack.HasBegun && totalDistance >= dragHandler.DragStartThreshold) {
                dragTrack.HasBegun = true;
                dragHandler.HandleBeginDrag(CreateEvent(dragTrack, input, GestureEventPhase.Began));

                if (input.Phase == GestureInputPhase.Moved && input.Delta != Vector2.zero) {
                    dragHandler.HandleDrag(CreateEvent(dragTrack, input, GestureEventPhase.Updated));
                    sentDragEvent = true;
                }
            }

            if (input.Phase == GestureInputPhase.Canceled) {
                if (dragTrack.HasBegun) {
                    dragHandler.HandleCancelDrag(CreateEvent(dragTrack, input, GestureEventPhase.Canceled));
                }

                dragTrack.IsCompleted = true;
                return;
            }

            if (input.Phase == GestureInputPhase.Ended) {
                if (dragTrack.HasBegun) {
                    dragHandler.HandleEndDrag(CreateEvent(dragTrack, input, GestureEventPhase.Completed));
                }

                dragTrack.IsCompleted = true;
                return;
            }

            if (dragTrack.HasBegun && !sentDragEvent && input.Phase == GestureInputPhase.Moved && input.Delta != Vector2.zero) {
                dragHandler.HandleDrag(CreateEvent(dragTrack, input, GestureEventPhase.Updated));
            }
        }

        /// <summary>
        /// ドラッグ通知用イベント引数を生成
        /// </summary>
        /// <param name="track">対象トラック</param>
        /// <param name="input">現在入力</param>
        /// <param name="phase">通知フェーズ</param>
        /// <returns>イベント引数</returns>
        private DragGestureEvent CreateEvent(DragGestureTrack track, GesturePointerInput input, GestureEventPhase phase) {
            return new DragGestureEvent(
                phase,
                track.StartPosition,
                input.Position,
                input.Delta,
                input.Position - track.StartPosition,
                input.Samples,
                input.Time - track.StartTime,
                track.EventCamera);
        }
    }
}
