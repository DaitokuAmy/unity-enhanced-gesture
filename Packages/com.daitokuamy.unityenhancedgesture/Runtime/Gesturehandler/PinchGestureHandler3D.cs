using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// Collider を対象にピンチイベントを公開するハンドラー
    /// </summary>
    public sealed class PinchGestureHandler3D : GestureHandlerBase, IPinchGestureHandler, IGestureHitDistanceProvider {
        [SerializeField, Tooltip("ピンチ対象 Collider 群")]
        private Collider[] _targetColliders = Array.Empty<Collider>();
        [SerializeField, Tooltip("ピンチ開始しきい値")]
        private float _pinchStartThreshold = 8.0f;

        /// <summary>ピンチ対象 Collider 群</summary>
        public IReadOnlyList<Collider> TargetColliders => _targetColliders;
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
            return TryGetHandleDistance(screenPosition, eventCamera, out _);
        }

        /// <inheritdoc/>
        public bool TryGetHandleDistance(Vector2 screenPosition, Camera eventCamera, out float distance) {
            distance = float.PositiveInfinity;

            if (eventCamera == null) {
                return false;
            }

            var ray = eventCamera.ScreenPointToRay(screenPosition);
            return TryGetClosestColliderHitDistance(_targetColliders, ray, out distance);
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
