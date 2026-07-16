using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// タップ入力を解釈する認識器
    /// </summary>
    internal sealed class TapGestureRecognizer : IGestureRecognizer {
        /// <summary>
        /// 進行中タップの内部状態
        /// </summary>
        private sealed class TapGestureTrack : IGestureTrack {
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

            /// <summary>現在タップの開始位置</summary>
            public Vector2 ActiveStartPosition { get; private set; }
            /// <summary>現在タップの開始時刻</summary>
            public float ActiveStartTime { get; private set; }
            /// <summary>ダブルタップ待機中かどうか</summary>
            public bool IsWaitingForSecondTap { get; set; }
            /// <summary>2回目タップ中かどうか</summary>
            public bool IsSecondTap { get; set; }
            /// <summary>ロングタップ進捗通知を開始済みかどうか</summary>
            public bool HasLongTapProgressBegun { get; set; }
            /// <summary>ロングタップ候補として継続できるかどうか</summary>
            public bool CanContinueLongTap { get; set; } = true;
            /// <summary>最初のタップ位置</summary>
            public Vector2 FirstTapPosition { get; private set; }
            /// <summary>最初のタップ終了位置</summary>
            public Vector2 FirstTapEndPosition { get; private set; }
            /// <summary>最初のタップ継続時間</summary>
            public float FirstTapDuration { get; private set; }
            /// <summary>最初のタップ完了時刻</summary>
            public float FirstTapCompletedTime { get; private set; }
            /// <summary>最初のタップ履歴</summary>
            public GesturePointerSample[] FirstTapSamples { get; private set; } = Array.Empty<GesturePointerSample>();
            /// <summary>最後に確認した現在位置</summary>
            public Vector2 LastPosition { get; private set; }
            /// <summary>最後に確認したサンプル列</summary>
            public GesturePointerSample[] LastSamples { get; private set; } = Array.Empty<GesturePointerSample>();
            /// <summary>最後に確認した時刻</summary>
            public float LastTime { get; private set; }

            /// <summary>
            /// トラックを生成
            /// </summary>
            /// <param name="recognizer">生成元認識器</param>
            /// <param name="handler">対象ハンドラー</param>
            /// <param name="input">開始入力</param>
            /// <param name="eventCamera">イベントに紐づくカメラ</param>
            public TapGestureTrack(IGestureRecognizer recognizer, ITapGestureHandler handler, GesturePointerInput input, Camera eventCamera) {
                Recognizer = recognizer;
                Handler = handler;
                EventCamera = eventCamera;
                ActivatePointer(input);
            }

            /// <summary>
            /// 現在タップを開始
            /// </summary>
            /// <param name="input">開始入力</param>
            public void ActivatePointer(GesturePointerInput input) {
                _pointerIds.Clear();
                _pointerIds.Add(input.PointerId);
                ActiveStartPosition = input.StartPosition;
                ActiveStartTime = input.StartTime;
                HasLongTapProgressBegun = false;
                CanContinueLongTap = true;
                UpdateLastInput(input);
            }

            /// <summary>
            /// 最後に確認した入力状態を更新
            /// </summary>
            /// <param name="input">現在入力</param>
            public void UpdateLastInput(GesturePointerInput input) {
                LastPosition = input.Position;
                LastSamples = input.Samples;
                LastTime = input.Time;
            }

            /// <summary>
            /// 現在タップを解除
            /// </summary>
            public void ClearPointer() {
                _pointerIds.Clear();
            }

            /// <summary>
            /// 最初のタップ情報を保存
            /// </summary>
            /// <param name="input">終了入力</param>
            /// <param name="duration">継続時間</param>
            public void StoreFirstTap(GesturePointerInput input, float duration) {
                FirstTapPosition = ActiveStartPosition;
                FirstTapEndPosition = input.Position;
                FirstTapDuration = duration;
                FirstTapCompletedTime = input.Time;
                FirstTapSamples = input.Samples;
                IsWaitingForSecondTap = true;
                IsSecondTap = false;
                ClearPointer();
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
            return handler is ITapGestureHandler;
        }

        /// <inheritdoc/>
        public IGestureTrack CreateTrack(IGestureHandler handler, GesturePointerInput input, Camera eventCamera) {
            return new TapGestureTrack(this, (ITapGestureHandler)handler, input, eventCamera);
        }

        /// <inheritdoc/>
        public bool TryAddPointer(IGestureTrack track, GesturePointerInput input) {
            var tapTrack = (TapGestureTrack)track;
            var tapHandler = (ITapGestureHandler)tapTrack.Handler;

            if (!tapTrack.IsWaitingForSecondTap || !tapHandler.EnableDoubleTap || input.Phase != GestureInputPhase.Began) {
                return false;
            }

            if ((input.Time - tapTrack.FirstTapCompletedTime) > tapHandler.DoubleTapMaxDelay) {
                return false;
            }

            if (Vector2.Distance(tapTrack.FirstTapPosition, input.StartPosition) > tapHandler.DoubleTapMaxMovement) {
                return false;
            }

            if (!tapTrack.Handler.CanHandle(input.Position, tapTrack.EventCamera)) {
                return false;
            }

            tapTrack.ActivatePointer(input);
            tapTrack.IsWaitingForSecondTap = false;
            tapTrack.IsSecondTap = true;
            return true;
        }

        /// <inheritdoc/>
        public void ProcessTrack(IGestureTrack track, IReadOnlyDictionary<int, GesturePointerInput> inputsByPointerId, float currentTime) {
            var tapTrack = (TapGestureTrack)track;
            var tapHandler = (ITapGestureHandler)tapTrack.Handler;

            if (tapTrack.PointerIds.Count == 0) {
                if (tapTrack.IsWaitingForSecondTap
                    && (currentTime - tapTrack.FirstTapCompletedTime) >= tapHandler.DoubleTapMaxDelay) {
                    tapHandler.HandleTap(CreateStoredSingleTapEvent(tapTrack));
                    tapTrack.IsCompleted = true;
                }

                return;
            }

            var pointerId = tapTrack.GetPointerId();

            if (!inputsByPointerId.TryGetValue(pointerId, out var input)) {
                return;
            }

            tapTrack.UpdateLastInput(input);

            var duration = Mathf.Max(0.0f, currentTime - tapTrack.ActiveStartTime);
            var movement = Vector2.Distance(tapTrack.ActiveStartPosition, input.Position);
            var canBecomeTap = movement <= tapHandler.MaxTapMovement && duration <= tapHandler.MaxTapDuration;

            if (tapTrack.CanContinueLongTap && movement > tapHandler.LongTapMaxMovement) {
                tapTrack.CanContinueLongTap = false;
            }

            var canBecomeLongTap = tapHandler.EnableLongTap
                && !tapTrack.IsSecondTap
                && tapTrack.CanContinueLongTap;
            var sentLongTapProgressBegan = false;

            if (input.Phase == GestureInputPhase.Canceled) {
                tapTrack.IsCompleted = true;
                CancelLongTapProgressIfNeeded(tapTrack, tapHandler, input, duration);
                return;
            }

            if (tapTrack.HasLongTapProgressBegun && !canBecomeLongTap) {
                CancelLongTapProgressIfNeeded(tapTrack, tapHandler, input, duration);

                if (tapTrack.IsCompleted) {
                    return;
                }
            }

            if (!canBecomeTap && !canBecomeLongTap) {
                tapTrack.IsCompleted = true;
                CancelLongTapProgressIfNeeded(tapTrack, tapHandler, input, duration);
                return;
            }

            if (canBecomeLongTap && !tapTrack.HasLongTapProgressBegun) {
                tapTrack.HasLongTapProgressBegun = true;
                sentLongTapProgressBegan = true;
                tapHandler.HandleLongTapProgress(CreateLongTapProgressEvent(tapTrack, tapHandler, input, GestureEventPhase.Began, duration));

                if (tapTrack.IsCompleted) {
                    return;
                }
            }

            if (canBecomeLongTap && duration >= tapHandler.LongTapDuration) {
                tapTrack.IsCompleted = true;
                tapTrack.ClearPointer();
                CompleteLongTapProgressIfNeeded(tapTrack, tapHandler, input, duration);
                tapHandler.HandleLongTap(CreateCurrentTapEvent(tapTrack, input, TapGestureType.LongTap, 1, duration, 0.0f));
                return;
            }

            if (input.Phase != GestureInputPhase.Ended) {
                if (canBecomeLongTap && tapTrack.HasLongTapProgressBegun && !sentLongTapProgressBegan) {
                    tapHandler.HandleLongTapProgress(CreateLongTapProgressEvent(tapTrack, tapHandler, input, GestureEventPhase.Updated, duration));

                    if (tapTrack.IsCompleted) {
                        return;
                    }
                }

                return;
            }

            CancelLongTapProgressIfNeeded(tapTrack, tapHandler, input, duration);

            if (tapTrack.IsCompleted) {
                return;
            }

            if (!canBecomeTap) {
                tapTrack.IsCompleted = true;
                return;
            }

            if (tapTrack.IsSecondTap) {
                tapHandler.HandleDoubleTap(CreateCurrentTapEvent(
                    tapTrack,
                    input,
                    TapGestureType.DoubleTap,
                    2,
                    duration,
                    input.StartTime - tapTrack.FirstTapCompletedTime));
                tapTrack.IsCompleted = true;
                tapTrack.ClearPointer();
                return;
            }

            if (!tapHandler.EnableDoubleTap) {
                tapHandler.HandleTap(CreateCurrentTapEvent(tapTrack, input, TapGestureType.SingleTap, 1, duration, 0.0f));
                tapTrack.IsCompleted = true;
                tapTrack.ClearPointer();
                return;
            }

            tapTrack.StoreFirstTap(input, duration);
        }

        /// <inheritdoc/>
        public void CancelTrack(IGestureTrack track, float currentTime) {
            var tapTrack = (TapGestureTrack)track;
            var tapHandler = (ITapGestureHandler)tapTrack.Handler;

            CancelLongTapProgressIfNeeded(tapTrack, tapHandler, currentTime);
            tapTrack.ClearPointer();
            tapTrack.IsCompleted = true;
        }

        /// <summary>
        /// 現在タップからイベント引数を生成
        /// </summary>
        /// <param name="track">対象トラック</param>
        /// <param name="input">現在入力</param>
        /// <param name="type">タップ種別</param>
        /// <param name="tapCount">タップ回数</param>
        /// <param name="duration">継続時間</param>
        /// <param name="interval">前回タップからの間隔</param>
        /// <returns>生成したイベント引数</returns>
        private TapGestureEvent CreateCurrentTapEvent(
            TapGestureTrack track,
            GesturePointerInput input,
            TapGestureType type,
            int tapCount,
            float duration,
            float interval) {
            var firstTapPosition = tapCount > 1 ? track.FirstTapPosition : track.ActiveStartPosition;
            return new TapGestureEvent(
                type,
                tapCount,
                firstTapPosition,
                track.ActiveStartPosition,
                input.Position,
                input.Samples,
                duration,
                interval,
                track.EventCamera);
        }

        /// <summary>
        /// ロングタップ進捗イベント引数を生成
        /// </summary>
        /// <param name="track">対象トラック</param>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="input">現在入力</param>
        /// <param name="phase">イベントフェーズ</param>
        /// <param name="duration">開始からの経過時間</param>
        /// <returns>生成したイベント引数</returns>
        private LongTapProgressGestureEvent CreateLongTapProgressEvent(
            TapGestureTrack track,
            ITapGestureHandler handler,
            GesturePointerInput input,
            GestureEventPhase phase,
            float duration) {
            return new LongTapProgressGestureEvent(
                phase,
                track.ActiveStartPosition,
                input.Position,
                input.Samples,
                duration,
                handler.LongTapDuration,
                handler.LongTapMaxMovement,
                track.EventCamera);
        }

        /// <summary>
        /// 保存済み入力状態からロングタップ進捗イベント引数を生成
        /// </summary>
        /// <param name="track">対象トラック</param>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="phase">イベントフェーズ</param>
        /// <param name="currentTime">現在時刻</param>
        /// <returns>生成したイベント引数</returns>
        private LongTapProgressGestureEvent CreateStoredLongTapProgressEvent(
            TapGestureTrack track,
            ITapGestureHandler handler,
            GestureEventPhase phase,
            float currentTime) {
            var duration = Mathf.Max(0.0f, currentTime - track.ActiveStartTime);
            return new LongTapProgressGestureEvent(
                phase,
                track.ActiveStartPosition,
                track.LastPosition,
                track.LastSamples,
                duration,
                handler.LongTapDuration,
                handler.LongTapMaxMovement,
                track.EventCamera);
        }

        /// <summary>
        /// ロングタップ進捗を完了通知
        /// </summary>
        /// <param name="track">対象トラック</param>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="input">現在入力</param>
        /// <param name="duration">開始からの経過時間</param>
        private void CompleteLongTapProgressIfNeeded(TapGestureTrack track, ITapGestureHandler handler, GesturePointerInput input, float duration) {
            if (!track.HasLongTapProgressBegun) {
                return;
            }

            track.HasLongTapProgressBegun = false;
            handler.HandleLongTapProgress(CreateLongTapProgressEvent(track, handler, input, GestureEventPhase.Completed, duration));
        }

        /// <summary>
        /// ロングタップ進捗をキャンセル通知
        /// </summary>
        /// <param name="track">対象トラック</param>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="input">現在入力</param>
        /// <param name="duration">開始からの経過時間</param>
        private void CancelLongTapProgressIfNeeded(TapGestureTrack track, ITapGestureHandler handler, GesturePointerInput input, float duration) {
            if (!track.HasLongTapProgressBegun) {
                return;
            }

            track.HasLongTapProgressBegun = false;
            handler.HandleLongTapProgress(CreateLongTapProgressEvent(track, handler, input, GestureEventPhase.Canceled, duration));
        }

        /// <summary>
        /// ロングタップ進捗を保存済み入力状態からキャンセル通知
        /// </summary>
        /// <param name="track">対象トラック</param>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="currentTime">現在時刻</param>
        private void CancelLongTapProgressIfNeeded(TapGestureTrack track, ITapGestureHandler handler, float currentTime) {
            if (!track.HasLongTapProgressBegun) {
                return;
            }

            track.HasLongTapProgressBegun = false;
            handler.HandleLongTapProgress(CreateStoredLongTapProgressEvent(track, handler, GestureEventPhase.Canceled, currentTime));
        }

        /// <summary>
        /// 保存済み単一タップからイベント引数を生成
        /// </summary>
        /// <param name="track">対象トラック</param>
        /// <returns>生成したイベント引数</returns>
        private TapGestureEvent CreateStoredSingleTapEvent(TapGestureTrack track) {
            return new TapGestureEvent(
                TapGestureType.SingleTap,
                1,
                track.FirstTapPosition,
                track.FirstTapPosition,
                track.FirstTapEndPosition,
                track.FirstTapSamples,
                track.FirstTapDuration,
                0.0f,
                track.EventCamera);
        }
    }
}
