using System;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// Collider を対象にドラッグイベントを公開するハンドラー
    /// </summary>
    public sealed class DragGestureHandler3D : GestureHandlerBase, IDragGestureHandler {
        [SerializeField, Tooltip("ドラッグ対象 Collider")]
        private Collider _targetCollider = null;
        [SerializeField, Tooltip("ドラッグ開始とみなす移動量")]
        private float _dragStartThreshold = 12.0f;
        [SerializeField, Tooltip("ロングタップドラッグを有効化するかどうか")]
        private bool _enableLongTapDrag = false;
        [SerializeField, Tooltip("ロングタップドラッグ開始までの待機時間")]
        private float _longTapDragDuration = 0.5f;
        [SerializeField, Tooltip("ロングタップドラッグ成立までの許容移動量")]
        private float _longTapDragMaxMovement = 12.0f;

        /// <summary>ドラッグ対象 Collider</summary>
        public Collider TargetCollider => _targetCollider;
        /// <inheritdoc/>
        public float DragStartThreshold => _dragStartThreshold;
        /// <inheritdoc/>
        public bool EnableLongTapDrag => _enableLongTapDrag;
        /// <inheritdoc/>
        public float LongTapDragDuration => _longTapDragDuration;
        /// <inheritdoc/>
        public float LongTapDragMaxMovement => _longTapDragMaxMovement;

        /// <summary>ドラッグ開始時に通知するイベント</summary>
        public event Action<DragGestureEvent> BeginDragEvent;
        /// <summary>ドラッグ更新時に通知するイベント</summary>
        public event Action<DragGestureEvent> DragEvent;
        /// <summary>ドラッグ終了時に通知するイベント</summary>
        public event Action<DragGestureEvent> EndDragEvent;
        /// <summary>ドラッグキャンセル時に通知するイベント</summary>
        public event Action<DragGestureEvent> CancelDragEvent;

        /// <inheritdoc/>
        public override bool CanHandle(Vector2 screenPosition, Camera eventCamera) {
            if (_targetCollider == null || eventCamera == null) {
                return false;
            }

            var ray = eventCamera.ScreenPointToRay(screenPosition);
            return _targetCollider.Raycast(ray, out _, float.PositiveInfinity);
        }

        /// <inheritdoc/>
        public void HandleBeginDrag(DragGestureEvent gestureEvent) {
            BeginDragEvent?.Invoke(gestureEvent);
        }

        /// <inheritdoc/>
        public void HandleDrag(DragGestureEvent gestureEvent) {
            DragEvent?.Invoke(gestureEvent);
        }

        /// <inheritdoc/>
        public void HandleEndDrag(DragGestureEvent gestureEvent) {
            EndDragEvent?.Invoke(gestureEvent);
        }

        /// <inheritdoc/>
        public void HandleCancelDrag(DragGestureEvent gestureEvent) {
            CancelDragEvent?.Invoke(gestureEvent);
        }
    }
}
