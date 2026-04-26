using System;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// Collider を対象にタップイベントを公開するハンドラー
    /// </summary>
    public sealed class TapGestureHandler3D : GestureHandlerBase, ITapGestureHandler {
        [SerializeField, Tooltip("タップ対象 Collider")]
        private Collider _targetCollider = null;
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

        /// <summary>タップ対象 Collider</summary>
        public Collider TargetCollider => _targetCollider;
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

        /// <inheritdoc/>
        public override bool CanHandle(Vector2 screenPosition, Camera eventCamera) {
            if (_targetCollider == null || eventCamera == null) {
                return false;
            }

            var ray = eventCamera.ScreenPointToRay(screenPosition);
            return _targetCollider.Raycast(ray, out _, float.PositiveInfinity);
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
    }
}
