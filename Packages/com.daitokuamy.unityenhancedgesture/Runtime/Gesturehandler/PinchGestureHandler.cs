using System;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ピンチ通知を公開するハンドラー
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PinchGestureHandler : GestureHandlerBase {
        [SerializeField, Tooltip("ピンチ開始とみなす距離差")]
        private float _pinchStartThreshold = 16f;

        /// <summary>ピンチ開始しきい値</summary>
        public float PinchStartThreshold => _pinchStartThreshold;

        /// <summary>ピンチ開始時に発火されるイベント</summary>
        public event Action<PinchGestureEvent> BeginPinchEvent;
        /// <summary>ピンチ更新時に発火されるイベント</summary>
        public event Action<PinchGestureEvent> PinchEvent;
        /// <summary>ピンチ終了時に発火されるイベント</summary>
        public event Action<PinchGestureEvent> EndPinchEvent;
        /// <summary>ピンチキャンセル時に発火されるイベント</summary>
        public event Action<PinchGestureEvent> CancelPinchEvent;

        /// <summary>
        /// ピンチ開始イベントを発火
        /// </summary>
        /// <param name="gestureEvent">通知内容</param>
        internal void RaiseBeginPinch(PinchGestureEvent gestureEvent) {
            BeginPinchEvent?.Invoke(gestureEvent);
        }

        /// <summary>
        /// ピンチ更新イベントを発火
        /// </summary>
        /// <param name="gestureEvent">通知内容</param>
        internal void RaisePinch(PinchGestureEvent gestureEvent) {
            PinchEvent?.Invoke(gestureEvent);
        }

        /// <summary>
        /// ピンチ終了イベントを発火
        /// </summary>
        /// <param name="gestureEvent">通知内容</param>
        internal void RaiseEndPinch(PinchGestureEvent gestureEvent) {
            EndPinchEvent?.Invoke(gestureEvent);
        }

        /// <summary>
        /// ピンチキャンセルイベントを発火
        /// </summary>
        /// <param name="gestureEvent">通知内容</param>
        internal void RaiseCancelPinch(PinchGestureEvent gestureEvent) {
            CancelPinchEvent?.Invoke(gestureEvent);
        }
    }
}
