using System;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// Collider を対象にピンチイベントを公開するハンドラー
    /// </summary>
    public sealed class PinchGestureHandler3D : GestureHandlerBase, IPinchGestureHandler {
        [SerializeField, Tooltip("ピンチ対象 Collider")]
        private Collider _targetCollider = null;
        [SerializeField, Tooltip("ピンチ開始しきい値")]
        private float _pinchStartThreshold = 8.0f;

        /// <summary>ピンチ対象 Collider</summary>
        public Collider TargetCollider => _targetCollider;
        /// <inheritdoc/>
        public float PinchStartThreshold => _pinchStartThreshold;

        /// <summary>ピンチ開始時に通知するイベント</summary>
        public event Action<PinchGestureEvent> BeginPinchEvent;
        /// <summary>ピンチ更新時に通知するイベント</summary>
        public event Action<PinchGestureEvent> PinchEvent;
        /// <summary>ピンチ終了時に通知するイベント</summary>
        public event Action<PinchGestureEvent> EndPinchEvent;
        /// <summary>ピンチキャンセル時に通知するイベント</summary>
        public event Action<PinchGestureEvent> CancelPinchEvent;

        /// <inheritdoc/>
        public override bool CanHandle(Vector2 screenPosition, Camera eventCamera) {
            if (_targetCollider == null || eventCamera == null) {
                return false;
            }

            var ray = eventCamera.ScreenPointToRay(screenPosition);
            return _targetCollider.Raycast(ray, out _, float.PositiveInfinity);
        }

        /// <inheritdoc/>
        public void HandleBeginPinch(PinchGestureEvent gestureEvent) {
            BeginPinchEvent?.Invoke(gestureEvent);
        }

        /// <inheritdoc/>
        public void HandlePinch(PinchGestureEvent gestureEvent) {
            PinchEvent?.Invoke(gestureEvent);
        }

        /// <inheritdoc/>
        public void HandleEndPinch(PinchGestureEvent gestureEvent) {
            EndPinchEvent?.Invoke(gestureEvent);
        }

        /// <inheritdoc/>
        public void HandleCancelPinch(PinchGestureEvent gestureEvent) {
            CancelPinchEvent?.Invoke(gestureEvent);
        }
    }
}
