using System;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// RectTransform を対象にドラッグイベントを公開するハンドラー
    /// </summary>
    public sealed class DragGestureHandlerUI : GestureHandlerBase, IDragGestureHandler {
        [SerializeField, Tooltip("ドラッグ対象 RectTransform")]
        private RectTransform _targetRectTransform = null;
        [SerializeField, Tooltip("ドラッグ開始とみなす移動量")]
        private float _dragStartThreshold = 12.0f;
        [SerializeField, Tooltip("ロングタップドラッグを有効化するかどうか")]
        private bool _enableLongTapDrag = false;
        [SerializeField, Tooltip("ロングタップドラッグ開始までの待機時間")]
        private float _longTapDragDuration = 0.5f;
        [SerializeField, Tooltip("ロングタップドラッグ成立までの許容移動量")]
        private float _longTapDragMaxMovement = 12.0f;

        /// <summary>ドラッグ対象 RectTransform</summary>
        public RectTransform TargetRectTransform => _targetRectTransform;
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
            if (_targetRectTransform == null) {
                return false;
            }

            return RectTransformUtility.RectangleContainsScreenPoint(
                _targetRectTransform,
                screenPosition,
                ResolveCanvasCamera());
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

        /// <summary>
        /// 画面座標判定に使用する Canvas カメラを取得
        /// </summary>
        /// <returns>判定に使用する Canvas カメラ</returns>
        private Camera ResolveCanvasCamera() {
            var canvas = _targetRectTransform.GetComponentInParent<Canvas>();

            if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                return null;
            }

            return canvas.worldCamera;
        }
    }
}
