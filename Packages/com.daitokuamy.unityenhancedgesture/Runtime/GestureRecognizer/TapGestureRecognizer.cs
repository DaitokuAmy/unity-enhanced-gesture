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

            var duration = input.Time - tapTrack.ActiveStartTime;
            var movement = Vector2.Distance(tapTrack.ActiveStartPosition, input.Position);
            var canBecomeTap = movement <= tapHandler.MaxTapMovement && duration <= tapHandler.MaxTapDuration;
            var canBecomeLongTap = tapHandler.EnableLongTap
                && !tapTrack.IsSecondTap
                && movement <= tapHandler.LongTapMaxMovement;

            if (input.Phase == GestureInputPhase.Canceled) {
                tapTrack.IsCompleted = true;
                return;
            }

            if (!canBecomeTap && !canBecomeLongTap) {
                tapTrack.IsCompleted = true;
                return;
            }

            if (canBecomeLongTap && duration >= tapHandler.LongTapDuration) {
                tapHandler.HandleLongTap(CreateCurrentTapEvent(tapTrack, input, TapGestureType.LongTap, 1, 0.0f));
                tapTrack.IsCompleted = true;
                tapTrack.ClearPointer();
                return;
            }

            if (input.Phase != GestureInputPhase.Ended) {
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
                    input.StartTime - tapTrack.FirstTapCompletedTime));
                tapTrack.IsCompleted = true;
                tapTrack.ClearPointer();
                return;
            }

            if (!tapHandler.EnableDoubleTap) {
                tapHandler.HandleTap(CreateCurrentTapEvent(tapTrack, input, TapGestureType.SingleTap, 1, 0.0f));
                tapTrack.IsCompleted = true;
                tapTrack.ClearPointer();
                return;
            }

            tapTrack.StoreFirstTap(input, duration);
        }

        /// <summary>
        /// 現在タップからイベント引数を生成
        /// </summary>
        /// <param name="track">対象トラック</param>
        /// <param name="input">現在入力</param>
        /// <param name="type">タップ種別</param>
        /// <param name="tapCount">タップ回数</param>
        /// <param name="interval">前回タップからの間隔</param>
        /// <returns>生成したイベント引数</returns>
        private TapGestureEvent CreateCurrentTapEvent(TapGestureTrack track, GesturePointerInput input, TapGestureType type, int tapCount, float interval) {
            var firstTapPosition = tapCount > 1 ? track.FirstTapPosition : track.ActiveStartPosition;
            return new TapGestureEvent(
                type,
                tapCount,
                firstTapPosition,
                track.ActiveStartPosition,
                input.Position,
                input.Samples,
                input.Time - track.ActiveStartTime,
                interval,
                track.EventCamera);
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
