using System;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// RectTransform を対象にピンチイベントを公開するハンドラー
    /// </summary>
    public sealed class PinchGestureHandlerUI : GestureHandlerBase, IPinchGestureHandler {
        [SerializeField, Tooltip("ピンチ対象 RectTransform")]
        private RectTransform _targetRectTransform = null;
        [SerializeField, Tooltip("ピンチ開始しきい値")]
        private float _pinchStartThreshold = 8.0f;

        /// <summary>ピンチ対象 RectTransform</summary>
        public RectTransform TargetRectTransform => _targetRectTransform;
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
            if (_targetRectTransform == null) {
                return false;
            }

            return RectTransformUtility.RectangleContainsScreenPoint(
                _targetRectTransform,
                screenPosition,
                ResolveCanvasCamera(_targetRectTransform));
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

        /// <inheritdoc/>
        internal override bool IsSelfUIRaycastTarget(GameObject raycastTarget) {
            return _targetRectTransform != null
                && raycastTarget != null
                && raycastTarget.transform == _targetRectTransform;
        }
    }
}
