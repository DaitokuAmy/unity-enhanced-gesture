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
            /// <summary>ロングタップドラッグ進捗を開始済みかどうか</summary>
            public bool HasLongTapDragProgressBegun { get; set; }
            /// <summary>ドラッグ開始方式</summary>
            public DragGestureStartMode StartMode { get; set; }
            /// <summary>最後に確認した現在位置</summary>
            public Vector2 LastPosition { get; private set; }
            /// <summary>最後に確認した差分量</summary>
            public Vector2 LastDelta { get; private set; }
            /// <summary>最後に確認したサンプル列</summary>
            public GesturePointerSample[] LastSamples { get; private set; }
            /// <summary>最後に確認した時刻</summary>
            public float LastTime { get; private set; }
            /// <summary>最後に確認した有効ポインター数</summary>
            public int LastActivePointerCount { get; private set; } = 1;

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
                UpdateLastInput(input, 1);
            }

            /// <summary>
            /// 最後に確認した入力状態を更新
            /// </summary>
            /// <param name="input">現在入力</param>
            /// <param name="activePointerCount">現在有効なポインター数</param>
            public void UpdateLastInput(GesturePointerInput input, int activePointerCount) {
                LastPosition = input.Position;
                LastDelta = input.Delta;
                LastSamples = input.Samples;
                LastTime = input.Time;
                LastActivePointerCount = activePointerCount;
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

            dragTrack.UpdateLastInput(input, inputsByPointerId.Count);

            var elapsedTime = input.Time - dragTrack.StartTime;
            var totalDistance = Vector2.Distance(dragTrack.StartPosition, input.Position);
            var sentDragEvent = false;
            var sentLongTapDragProgressBegan = false;

            if (dragHandler.EnableLongTapDrag
                && dragTrack.CanBeginLongTapDrag
                && totalDistance > dragHandler.LongTapDragMaxMovement) {
                dragTrack.CanBeginLongTapDrag = false;
            }

            var canBeginLongTapDrag = dragHandler.EnableLongTapDrag
                && dragTrack.CanBeginLongTapDrag
                && !dragTrack.HasBegun;

            if (dragTrack.HasLongTapDragProgressBegun && !canBeginLongTapDrag) {
                CancelLongTapDragProgressIfNeeded(dragTrack, dragHandler, input, elapsedTime);

                if (dragTrack.IsCompleted) {
                    return;
                }
            }

            if (canBeginLongTapDrag && !dragTrack.HasLongTapDragProgressBegun) {
                dragTrack.HasLongTapDragProgressBegun = true;
                sentLongTapDragProgressBegan = true;
                dragHandler.HandleLongTapDragProgress(CreateLongTapDragProgressEvent(dragTrack, dragHandler, input, GestureEventPhase.Began, elapsedTime));

                if (dragTrack.IsCompleted) {
                    return;
                }
            }

            if (!dragTrack.HasBegun
                && canBeginLongTapDrag
                && elapsedTime >= dragHandler.LongTapDragDuration) {
                dragTrack.HasBegun = true;
                dragTrack.StartMode = DragGestureStartMode.LongTap;
                CompleteLongTapDragProgressIfNeeded(dragTrack, dragHandler, input, elapsedTime);

                if (dragTrack.IsCompleted) {
                    return;
                }

                dragHandler.HandleBeginDrag(CreateEvent(dragTrack, input, GestureEventPhase.Began, inputsByPointerId.Count));

                if (dragTrack.IsCompleted) {
                    return;
                }

                if (input.Phase == GestureInputPhase.Moved && input.Delta != Vector2.zero) {
                    dragHandler.HandleDrag(CreateEvent(dragTrack, input, GestureEventPhase.Updated, inputsByPointerId.Count));
                    sentDragEvent = true;

                    if (dragTrack.IsCompleted) {
                        return;
                    }
                }
            }

            if (!dragTrack.HasBegun && totalDistance >= dragHandler.DragStartThreshold) {
                CancelLongTapDragProgressIfNeeded(dragTrack, dragHandler, input, elapsedTime);

                if (dragTrack.IsCompleted) {
                    return;
                }

                dragTrack.HasBegun = true;
                dragTrack.StartMode = DragGestureStartMode.Immediate;
                dragHandler.HandleBeginDrag(CreateEvent(dragTrack, input, GestureEventPhase.Began, inputsByPointerId.Count));

                if (dragTrack.IsCompleted) {
                    return;
                }

                if (input.Phase == GestureInputPhase.Moved && input.Delta != Vector2.zero) {
                    dragHandler.HandleDrag(CreateEvent(dragTrack, input, GestureEventPhase.Updated, inputsByPointerId.Count));
                    sentDragEvent = true;

                    if (dragTrack.IsCompleted) {
                        return;
                    }
                }
            }

            if (input.Phase == GestureInputPhase.Canceled) {
                if (dragTrack.HasBegun) {
                    dragHandler.HandleCancelDrag(CreateEvent(dragTrack, input, GestureEventPhase.Canceled, inputsByPointerId.Count));
                } else {
                    CancelLongTapDragProgressIfNeeded(dragTrack, dragHandler, input, elapsedTime);
                }

                dragTrack.IsCompleted = true;
                return;
            }

            if (input.Phase == GestureInputPhase.Ended) {
                if (dragTrack.HasBegun) {
                    dragHandler.HandleEndDrag(CreateEvent(dragTrack, input, GestureEventPhase.Completed, inputsByPointerId.Count));
                } else {
                    CancelLongTapDragProgressIfNeeded(dragTrack, dragHandler, input, elapsedTime);
                }

                dragTrack.IsCompleted = true;
                return;
            }

            if (!dragTrack.HasBegun
                && canBeginLongTapDrag
                && dragTrack.HasLongTapDragProgressBegun
                && !sentLongTapDragProgressBegan) {
                dragHandler.HandleLongTapDragProgress(CreateLongTapDragProgressEvent(dragTrack, dragHandler, input, GestureEventPhase.Updated, elapsedTime));

                if (dragTrack.IsCompleted) {
                    return;
                }
            }

            if (dragTrack.HasBegun
                && !sentDragEvent
                && input.Phase == GestureInputPhase.Moved
                && input.Delta != Vector2.zero) {
                dragHandler.HandleDrag(CreateEvent(dragTrack, input, GestureEventPhase.Updated, inputsByPointerId.Count));
            }
        }

        /// <inheritdoc/>
        public void CancelTrack(IGestureTrack track, float currentTime) {
            var dragTrack = (DragGestureTrack)track;
            var dragHandler = (IDragGestureHandler)dragTrack.Handler;

            CancelLongTapDragProgressIfNeeded(dragTrack, dragHandler, currentTime);

            if (dragTrack.HasBegun) {
                dragHandler.HandleCancelDrag(CreateStoredEvent(dragTrack, GestureEventPhase.Canceled));
            }

            dragTrack.IsCompleted = true;
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

        /// <summary>
        /// 保存済み入力状態からドラッグイベント引数を生成
        /// </summary>
        /// <param name="track">対象トラック</param>
        /// <param name="phase">イベントフェーズ</param>
        /// <returns>生成したイベント引数</returns>
        private DragGestureEvent CreateStoredEvent(DragGestureTrack track, GestureEventPhase phase) {
            return new DragGestureEvent(
                phase,
                track.StartMode,
                track.StartPosition,
                track.LastPosition,
                track.LastDelta,
                track.LastPosition - track.StartPosition,
                track.LastSamples,
                track.LastTime - track.StartTime,
                track.LastActivePointerCount,
                track.EventCamera);
        }

        /// <summary>
        /// ロングタップドラッグ進捗イベント引数を生成
        /// </summary>
        /// <param name="track">対象トラック</param>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="input">現在入力</param>
        /// <param name="phase">イベントフェーズ</param>
        /// <param name="duration">開始からの経過時間</param>
        /// <returns>生成したイベント引数</returns>
        private LongTapDragProgressGestureEvent CreateLongTapDragProgressEvent(
            DragGestureTrack track,
            IDragGestureHandler handler,
            GesturePointerInput input,
            GestureEventPhase phase,
            float duration) {
            return new LongTapDragProgressGestureEvent(
                phase,
                track.StartPosition,
                input.Position,
                input.Samples,
                duration,
                handler.LongTapDragDuration,
                handler.LongTapDragMaxMovement,
                track.EventCamera);
        }

        /// <summary>
        /// 保存済み入力状態からロングタップドラッグ進捗イベント引数を生成
        /// </summary>
        /// <param name="track">対象トラック</param>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="phase">イベントフェーズ</param>
        /// <param name="currentTime">現在時刻</param>
        /// <returns>生成したイベント引数</returns>
        private LongTapDragProgressGestureEvent CreateStoredLongTapDragProgressEvent(
            DragGestureTrack track,
            IDragGestureHandler handler,
            GestureEventPhase phase,
            float currentTime) {
            var duration = Mathf.Max(0.0f, currentTime - track.StartTime);
            return new LongTapDragProgressGestureEvent(
                phase,
                track.StartPosition,
                track.LastPosition,
                track.LastSamples,
                duration,
                handler.LongTapDragDuration,
                handler.LongTapDragMaxMovement,
                track.EventCamera);
        }

        /// <summary>
        /// ロングタップドラッグ進捗を完了通知
        /// </summary>
        /// <param name="track">対象トラック</param>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="input">現在入力</param>
        /// <param name="duration">開始からの経過時間</param>
        private void CompleteLongTapDragProgressIfNeeded(DragGestureTrack track, IDragGestureHandler handler, GesturePointerInput input, float duration) {
            if (!track.HasLongTapDragProgressBegun) {
                return;
            }

            track.HasLongTapDragProgressBegun = false;
            handler.HandleLongTapDragProgress(CreateLongTapDragProgressEvent(track, handler, input, GestureEventPhase.Completed, duration));
        }

        /// <summary>
        /// ロングタップドラッグ進捗をキャンセル通知
        /// </summary>
        /// <param name="track">対象トラック</param>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="input">現在入力</param>
        /// <param name="duration">開始からの経過時間</param>
        private void CancelLongTapDragProgressIfNeeded(DragGestureTrack track, IDragGestureHandler handler, GesturePointerInput input, float duration) {
            if (!track.HasLongTapDragProgressBegun) {
                return;
            }

            track.HasLongTapDragProgressBegun = false;
            handler.HandleLongTapDragProgress(CreateLongTapDragProgressEvent(track, handler, input, GestureEventPhase.Canceled, duration));
        }

        /// <summary>
        /// ロングタップドラッグ進捗を保存済み入力状態からキャンセル通知
        /// </summary>
        /// <param name="track">対象トラック</param>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="currentTime">現在時刻</param>
        private void CancelLongTapDragProgressIfNeeded(DragGestureTrack track, IDragGestureHandler handler, float currentTime) {
            if (!track.HasLongTapDragProgressBegun) {
                return;
            }

            track.HasLongTapDragProgressBegun = false;
            handler.HandleLongTapDragProgress(CreateStoredLongTapDragProgressEvent(track, handler, GestureEventPhase.Canceled, currentTime));
        }
    }
}
