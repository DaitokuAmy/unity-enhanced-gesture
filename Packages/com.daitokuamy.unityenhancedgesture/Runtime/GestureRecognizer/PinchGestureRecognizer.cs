using System.Collections.Generic;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ピンチ入力を解釈する認識器
    /// </summary>
    internal sealed class PinchGestureRecognizer : IGestureRecognizer {
        /// <summary>
        /// 進行中ピンチの内部状態
        /// </summary>
        private sealed class PinchGestureTrack : IGestureTrack {
            private readonly List<int> _pointerIds = new(2);

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

            /// <summary>1本目の現在位置</summary>
            public Vector2 FirstPosition { get; set; }
            /// <summary>2本目の現在位置</summary>
            public Vector2 SecondPosition { get; set; }
            /// <summary>開始中心位置</summary>
            public Vector2 StartCenter { get; private set; }
            /// <summary>開始距離</summary>
            public float StartDistance { get; private set; }
            /// <summary>開始角度</summary>
            public float StartAngle { get; private set; }
            /// <summary>開始時刻</summary>
            public float StartTime { get; private set; }
            /// <summary>前回中心位置</summary>
            public Vector2 PreviousCenter { get; set; }
            /// <summary>前回距離</summary>
            public float PreviousDistance { get; set; }
            /// <summary>前回角度</summary>
            public float PreviousAngle { get; set; }
            /// <summary>開始イベント送信済みかどうか</summary>
            public bool HasBegun { get; set; }

            /// <summary>
            /// トラックを生成
            /// </summary>
            /// <param name="recognizer">生成元認識器</param>
            /// <param name="handler">対象ハンドラー</param>
            /// <param name="input">開始入力</param>
            /// <param name="eventCamera">イベントに紐づくカメラ</param>
            public PinchGestureTrack(IGestureRecognizer recognizer, IPinchGestureHandler handler, GesturePointerInput input, Camera eventCamera) {
                Recognizer = recognizer;
                Handler = handler;
                EventCamera = eventCamera;
                _pointerIds.Add(input.PointerId);
                FirstPosition = input.Position;
            }

            /// <summary>
            /// 2本目のポインターを追加
            /// </summary>
            /// <param name="input">開始入力</param>
            public void AttachSecondPointer(GesturePointerInput input) {
                _pointerIds.Add(input.PointerId);
                SecondPosition = input.Position;
                StartCenter = (FirstPosition + SecondPosition) * 0.5f;
                StartDistance = Vector2.Distance(FirstPosition, SecondPosition);
                var vector = SecondPosition - FirstPosition;
                StartAngle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
                PreviousCenter = StartCenter;
                PreviousDistance = StartDistance;
                PreviousAngle = StartAngle;
                StartTime = input.StartTime;
            }

            /// <summary>
            /// ピンチ開始基準を現在状態で再初期化
            /// </summary>
            /// <param name="center">開始中心位置</param>
            /// <param name="distance">開始距離</param>
            /// <param name="angle">開始角度</param>
            /// <param name="time">開始時刻</param>
            public void BeginGesture(Vector2 center, float distance, float angle, float time) {
                StartCenter = center;
                StartDistance = distance;
                StartAngle = angle;
                StartTime = time;
                PreviousCenter = center;
                PreviousDistance = distance;
                PreviousAngle = angle;
            }

            /// <summary>
            /// 1本目のポインター ID を取得
            /// </summary>
            /// <returns>1本目のポインター ID</returns>
            public int GetFirstPointerId() {
                return _pointerIds[0];
            }

            /// <summary>
            /// 2本目のポインター ID を取得
            /// </summary>
            /// <returns>2本目のポインター ID</returns>
            public int GetSecondPointerId() {
                return _pointerIds[1];
            }
        }

        /// <inheritdoc/>
        public bool CanCreateTrack(IGestureHandler handler) {
            return handler is IPinchGestureHandler;
        }

        /// <inheritdoc/>
        public IGestureTrack CreateTrack(IGestureHandler handler, GesturePointerInput input, Camera eventCamera) {
            return new PinchGestureTrack(this, (IPinchGestureHandler)handler, input, eventCamera);
        }

        /// <inheritdoc/>
        public bool TryAddPointer(IGestureTrack track, GesturePointerInput input) {
            var pinchTrack = (PinchGestureTrack)track;

            if (input.Phase != GestureInputPhase.Began || pinchTrack.PointerIds.Count != 1) {
                return false;
            }

            if (!pinchTrack.Handler.CanHandle(input.Position, pinchTrack.EventCamera)) {
                return false;
            }

            pinchTrack.AttachSecondPointer(input);
            return true;
        }

        /// <inheritdoc/>
        public void ProcessTrack(IGestureTrack track, IReadOnlyDictionary<int, GesturePointerInput> inputsByPointerId, float currentTime) {
            var pinchTrack = (PinchGestureTrack)track;
            var pinchHandler = (IPinchGestureHandler)pinchTrack.Handler;
            var firstPointerId = pinchTrack.GetFirstPointerId();

            if (!inputsByPointerId.TryGetValue(firstPointerId, out var firstInput)) {
                return;
            }

            pinchTrack.FirstPosition = firstInput.Position;

            if (pinchTrack.PointerIds.Count == 1) {
                if (firstInput.Phase == GestureInputPhase.Canceled || firstInput.Phase == GestureInputPhase.Ended) {
                    pinchTrack.IsCompleted = true;
                }

                return;
            }

            var secondPointerId = pinchTrack.GetSecondPointerId();

            if (!inputsByPointerId.TryGetValue(secondPointerId, out var secondInput)) {
                return;
            }

            pinchTrack.SecondPosition = secondInput.Position;

            if (firstInput.Phase == GestureInputPhase.Canceled || secondInput.Phase == GestureInputPhase.Canceled) {
                if (pinchTrack.HasBegun) {
                    pinchHandler.HandleCancelPinch(CreateEvent(pinchTrack, GestureEventPhase.Canceled, currentTime));
                }

                pinchTrack.IsCompleted = true;
                return;
            }

            var center = GetCenter(pinchTrack.FirstPosition, pinchTrack.SecondPosition);
            var distance = Vector2.Distance(pinchTrack.FirstPosition, pinchTrack.SecondPosition);
            var angle = GetAngle(pinchTrack.FirstPosition, pinchTrack.SecondPosition);
            var shouldBegin = pinchHandler.PinchStartThreshold <= 0.0f
                || Mathf.Abs(distance - pinchTrack.StartDistance) >= pinchHandler.PinchStartThreshold;

            if (!pinchTrack.HasBegun && shouldBegin) {
                pinchTrack.BeginGesture(center, distance, angle, currentTime);
                pinchTrack.HasBegun = true;
                pinchHandler.HandleBeginPinch(CreateEvent(pinchTrack, GestureEventPhase.Began, currentTime));

                if (pinchTrack.IsCompleted) {
                    return;
                }

                pinchTrack.PreviousCenter = center;
                pinchTrack.PreviousDistance = distance;
                pinchTrack.PreviousAngle = angle;
            }

            if (firstInput.Phase == GestureInputPhase.Ended || secondInput.Phase == GestureInputPhase.Ended) {
                if (pinchTrack.HasBegun) {
                    pinchHandler.HandleEndPinch(CreateEvent(pinchTrack, GestureEventPhase.Completed, currentTime));
                }

                pinchTrack.IsCompleted = true;
                return;
            }

            if (!pinchTrack.HasBegun) {
                return;
            }

            if (center != pinchTrack.PreviousCenter
                || !Mathf.Approximately(distance, pinchTrack.PreviousDistance)
                || !Mathf.Approximately(angle, pinchTrack.PreviousAngle)) {
                pinchHandler.HandlePinch(CreateEvent(pinchTrack, GestureEventPhase.Updated, currentTime));
                pinchTrack.PreviousCenter = center;
                pinchTrack.PreviousDistance = distance;
                pinchTrack.PreviousAngle = angle;
            }
        }

        /// <inheritdoc/>
        public void CancelTrack(IGestureTrack track, float currentTime) {
            var pinchTrack = (PinchGestureTrack)track;
            var pinchHandler = (IPinchGestureHandler)pinchTrack.Handler;

            if (pinchTrack.HasBegun) {
                pinchHandler.HandleCancelPinch(CreateEvent(pinchTrack, GestureEventPhase.Canceled, currentTime));
            }

            pinchTrack.IsCompleted = true;
        }

        /// <summary>
        /// ピンチイベント引数を生成
        /// </summary>
        /// <param name="track">対象トラック</param>
        /// <param name="phase">イベントフェーズ</param>
        /// <param name="currentTime">現在時刻</param>
        /// <returns>生成したイベント引数</returns>
        private PinchGestureEvent CreateEvent(PinchGestureTrack track, GestureEventPhase phase, float currentTime) {
            var center = GetCenter(track.FirstPosition, track.SecondPosition);
            var distance = Vector2.Distance(track.FirstPosition, track.SecondPosition);
            var angle = GetAngle(track.FirstPosition, track.SecondPosition);
            var deltaAngle = phase == GestureEventPhase.Began ? 0.0f : Mathf.DeltaAngle(track.PreviousAngle, angle);
            var deltaDistance = phase == GestureEventPhase.Began ? 0.0f : distance - track.PreviousDistance;
            var centerDelta = phase == GestureEventPhase.Began ? Vector2.zero : center - track.PreviousCenter;
            var scale = track.StartDistance <= Mathf.Epsilon ? 1.0f : distance / track.StartDistance;

            return new PinchGestureEvent(
                phase,
                track.StartCenter,
                center,
                centerDelta,
                track.StartDistance,
                distance,
                deltaDistance,
                scale,
                track.StartAngle,
                angle,
                deltaAngle,
                Mathf.DeltaAngle(track.StartAngle, angle),
                track.FirstPosition,
                track.SecondPosition,
                currentTime - track.StartTime,
                track.EventCamera);
        }

        /// <summary>
        /// 2点の中心位置を取得
        /// </summary>
        /// <param name="firstPosition">1点目</param>
        /// <param name="secondPosition">2点目</param>
        /// <returns>中心位置</returns>
        private Vector2 GetCenter(Vector2 firstPosition, Vector2 secondPosition) {
            return (firstPosition + secondPosition) * 0.5f;
        }

        /// <summary>
        /// 2点から角度を取得
        /// </summary>
        /// <param name="firstPosition">1点目</param>
        /// <param name="secondPosition">2点目</param>
        /// <returns>角度</returns>
        private float GetAngle(Vector2 firstPosition, Vector2 secondPosition) {
            var vector = secondPosition - firstPosition;
            return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        }
    }
}
