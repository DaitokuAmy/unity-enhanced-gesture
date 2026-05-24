using System;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// RectTransform を対象にタップイベントを公開するハンドラー
    /// </summary>
    public sealed class TapGestureHandlerUI : GestureHandlerBase, ITapGestureHandler {
        [SerializeField, Tooltip("タップ対象 RectTransform")]
        private RectTransform _targetRectTransform = null;
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

        /// <summary>タップ対象 RectTransform</summary>
        public RectTransform TargetRectTransform => _targetRectTransform;
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
            if (_targetRectTransform == null) {
                return false;
            }

            return RectTransformUtility.RectangleContainsScreenPoint(
                _targetRectTransform,
                screenPosition,
                ResolveCanvasCamera(_targetRectTransform));
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
        internal override bool IsSelfUIRaycastTarget(GameObject raycastTarget) {
            return _targetRectTransform != null
                && raycastTarget != null
                && raycastTarget.transform == _targetRectTransform;
        }
    }
}
