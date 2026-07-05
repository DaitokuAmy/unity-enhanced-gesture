using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// Collider を対象にタップイベントを公開するハンドラー
    /// </summary>
    public sealed class TapGestureHandler3D : GestureHandlerBase, ITapGestureHandler, IGestureHitDistanceProvider {
        [SerializeField, Tooltip("タップ対象 Collider 群")]
        private Collider[] _targetColliders = Array.Empty<Collider>();
        [SerializeField, Tooltip("単一タップとして認める最大継続時間")]
        private float _maxTapDuration = 0.25f;
        [SerializeField, Tooltip("単一タップとして認める最大移動量")]
        private float _maxTapMovement = 12.0f;
        [SerializeField, Tooltip("ダブルタップを有効化するかどうか")]
        private bool _enableDoubleTap = false;
        [SerializeField, Tooltip("ダブルタップ成立までの最大待機時間")]
        private float _doubleTapMaxDelay = 0.3f;
        [SerializeField, Tooltip("ダブルタップ成立までの最大位置差")]
        private float _doubleTapMaxMovement = 24.0f;
        [SerializeField, Tooltip("ロングタップを有効化するかどうか")]
        private bool _enableLongTap = false;
        [SerializeField, Tooltip("ロングタップ成立までの待機時間")]
        private float _longTapDuration = 0.5f;
        [SerializeField, Tooltip("ロングタップ成立までの許容移動量")]
        private float _longTapMaxMovement = 12.0f;

        /// <summary>タップ対象 Collider 群</summary>
        public IReadOnlyList<Collider> TargetColliders => _targetColliders;
        /// <inheritdoc/>
        public float MaxTapDuration => _maxTapDuration;
        /// <inheritdoc/>
        public float MaxTapMovement => _maxTapMovement;
        /// <inheritdoc/>
        public bool EnableDoubleTap => _enableDoubleTap;
        /// <inheritdoc/>
        public float DoubleTapMaxDelay => _doubleTapMaxDelay;
        /// <inheritdoc/>
        public float DoubleTapMaxMovement => _doubleTapMaxMovement;
        /// <inheritdoc/>
        public bool EnableLongTap => _enableLongTap;
        /// <inheritdoc/>
        public float LongTapDuration => _longTapDuration;
        /// <inheritdoc/>
        public float LongTapMaxMovement => _longTapMaxMovement;

        /// <summary>単一タップ時に通知するイベント</summary>
        public event Action<TapGestureEvent> TapEvent;
        /// <summary>ダブルタップ時に通知するイベント</summary>
        public event Action<TapGestureEvent> DoubleTapEvent;
        /// <summary>ロングタップ時に通知するイベント</summary>
        public event Action<TapGestureEvent> LongTapEvent;
        /// <summary>ロングタップ進捗時に通知するイベント</summary>
        public event Action<LongTapProgressGestureEvent> LongTapProgressEvent;

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
        public void HandleTap(TapGestureEvent gestureEvent) {
            TapEvent?.Invoke(gestureEvent);
        }

        /// <inheritdoc/>
        public void HandleDoubleTap(TapGestureEvent gestureEvent) {
            DoubleTapEvent?.Invoke(gestureEvent);
        }

        /// <inheritdoc/>
        public void HandleLongTap(TapGestureEvent gestureEvent) {
            LongTapEvent?.Invoke(gestureEvent);
        }

        /// <inheritdoc/>
        public void HandleLongTapProgress(LongTapProgressGestureEvent gestureEvent) {
            LongTapProgressEvent?.Invoke(gestureEvent);
        }
    }
}
