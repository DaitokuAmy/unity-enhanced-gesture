using System.Collections.Generic;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ドラッグ入力を解釈する認識器
    /// </summary>
    internal sealed class DragGestureRecognizer : IGestureRecognizer {
        /// <summary>
        /// 進行中ドラッグの内部状態
        /// </summary>
        private sealed class DragGestureTrack : IGestureTrack {
            private readonly List<int> _pointerIds = new(1);

            /// <inheritdoc/>
            public IGestureHandler Handler { get; }
            /// <inheritdoc/>
            public IGestureRecognizer Recognizer { get; }
            /// <inheritdoc/>
            public IReadOnlyList<int> PointerIds => _pointerIds;
            /// <inheritdoc/>
            public Camera EventCamera { get; }
            /// <inheritdoc/>
            public bool IsCompleted { get; set; }

            /// <summary>ドラッグ開始位置</summary>
            public Vector2 StartPosition { get; }
            /// <summary>ドラッグ開始時刻</summary>
            public float StartTime { get; }
            /// <summary>開始イベントを送信済みかどうか</summary>
            public bool HasBegun { get; set; }
            /// <summary>ロングタップドラッグがまだ有効かどうか</summary>
            public bool CanBeginLongTapDrag { get; set; } = true;
            /// <summary>ドラッグ開始方式</summary>
            public DragGestureStartMode StartMode { get; set; }

            /// <summary>
            /// トラックを生成
            /// </summary>
            /// <param name="recognizer">生成元認識器</param>
            /// <param name="handler">対象ハンドラー</param>
            /// <param name="input">開始入力</param>
            /// <param name="eventCamera">イベントに紐づくカメラ</param>
            public DragGestureTrack(IGestureRecognizer recognizer, IDragGestureHandler handler, GesturePointerInput input, Camera eventCamera) {
                Recognizer = recognizer;
                Handler = handler;
                EventCamera = eventCamera;
                StartPosition = input.StartPosition;
                StartTime = input.StartTime;
                _pointerIds.Add(input.PointerId);
            }

            /// <summary>
            /// 所有ポインター ID を取得
            /// </summary>
            /// <returns>所有ポインター ID</returns>
            public int GetPointerId() {
                return _pointerIds[0];
            }
        }

        /// <inheritdoc/>
        public bool CanCreateTrack(IGestureHandler handler) {
            return handler is IDragGestureHandler;
        }

        /// <inheritdoc/>
        public IGestureTrack CreateTrack(IGestureHandler handler, GesturePointerInput input, Camera eventCamera) {
            return new DragGestureTrack(this, (IDragGestureHandler)handler, input, eventCamera);
        }

        /// <inheritdoc/>
        public bool TryAddPointer(IGestureTrack track, GesturePointerInput input) {
            return false;
        }

        /// <inheritdoc/>
        public void ProcessTrack(IGestureTrack track, IReadOnlyDictionary<int, GesturePointerInput> inputsByPointerId, float currentTime) {
            var dragTrack = (DragGestureTrack)track;
            var dragHandler = (IDragGestureHandler)dragTrack.Handler;
            var pointerId = dragTrack.GetPointerId();

            if (!inputsByPointerId.TryGetValue(pointerId, out var input)) {
                return;
            }

            var elapsedTime = input.Time - dragTrack.StartTime;
            var totalDistance = Vector2.Distance(dragTrack.StartPosition, input.Position);
            var sentDragEvent = false;

            if (dragHandler.EnableLongTapDrag
                && dragTrack.CanBeginLongTapDrag
                && totalDistance > dragHandler.LongTapDragMaxMovement) {
                dragTrack.CanBeginLongTapDrag = false;
            }

            if (!dragTrack.HasBegun
                && dragHandler.EnableLongTapDrag
                && dragTrack.CanBeginLongTapDrag
                && elapsedTime >= dragHandler.LongTapDragDuration) {
                dragTrack.HasBegun = true;
                dragTrack.StartMode = DragGestureStartMode.LongTap;
                dragHandler.HandleBeginDrag(CreateEvent(dragTrack, input, GestureEventPhase.Began, inputsByPointerId.Count));

                if (input.Phase == GestureInputPhase.Moved && input.Delta != Vector2.zero) {
                    dragHandler.HandleDrag(CreateEvent(dragTrack, input, GestureEventPhase.Updated, inputsByPointerId.Count));
                    sentDragEvent = true;
                }
            }

            if (!dragTrack.HasBegun && totalDistance >= dragHandler.DragStartThreshold) {
                dragTrack.HasBegun = true;
                dragTrack.StartMode = DragGestureStartMode.Immediate;
                dragHandler.HandleBeginDrag(CreateEvent(dragTrack, input, GestureEventPhase.Began, inputsByPointerId.Count));

                if (input.Phase == GestureInputPhase.Moved && input.Delta != Vector2.zero) {
                    dragHandler.HandleDrag(CreateEvent(dragTrack, input, GestureEventPhase.Updated, inputsByPointerId.Count));
                    sentDragEvent = true;
                }
            }

            if (input.Phase == GestureInputPhase.Canceled) {
                if (dragTrack.HasBegun) {
                    dragHandler.HandleCancelDrag(CreateEvent(dragTrack, input, GestureEventPhase.Canceled, inputsByPointerId.Count));
                }

                dragTrack.IsCompleted = true;
                return;
            }

            if (input.Phase == GestureInputPhase.Ended) {
                if (dragTrack.HasBegun) {
                    dragHandler.HandleEndDrag(CreateEvent(dragTrack, input, GestureEventPhase.Completed, inputsByPointerId.Count));
                }

                dragTrack.IsCompleted = true;
                return;
            }

            if (dragTrack.HasBegun
                && !sentDragEvent
                && input.Phase == GestureInputPhase.Moved
                && input.Delta != Vector2.zero) {
                dragHandler.HandleDrag(CreateEvent(dragTrack, input, GestureEventPhase.Updated, inputsByPointerId.Count));
            }
        }

        /// <summary>
        /// ドラッグイベント引数を生成
        /// </summary>
        /// <param name="track">対象トラック</param>
        /// <param name="input">現在入力</param>
        /// <param name="phase">イベントフェーズ</param>
        /// <param name="activePointerCount">現在有効なポインター数</param>
        /// <returns>生成したイベント引数</returns>
        private DragGestureEvent CreateEvent(DragGestureTrack track, GesturePointerInput input, GestureEventPhase phase, int activePointerCount) {
            return new DragGestureEvent(
                phase,
                track.StartMode,
                track.StartPosition,
                input.Position,
                input.Delta,
                input.Position - track.StartPosition,
                input.Samples,
                input.Time - track.StartTime,
                activePointerCount,
                track.EventCamera);
        }
    }
}
