using System;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// RectTransform を対象にドラッグ通知を公開するハンドラー
    /// </summary>
    public sealed class DragGestureHandler : GestureHandlerBase, IDragGestureHandler {
        [SerializeField, Tooltip("ドラッグ対象 RectTransform")]
        private RectTransform _targetRectTransform = null;
        [SerializeField, Tooltip("ドラッグ開始とみなす移動量")]
        private float _dragStartThreshold = 12.0f;

        /// <summary>
        /// ドラッグ対象 RectTransform
        /// </summary>
        public RectTransform TargetRectTransform => _targetRectTransform;

        /// <inheritdoc/>
        public float DragStartThreshold => _dragStartThreshold;

        /// <summary>
        /// ドラッグ開始時に通知するイベント
        /// </summary>
        public event Action<DragGestureEvent> BeginDragEvent;

        /// <summary>
        /// ドラッグ更新時に通知するイベント
        /// </summary>
        public event Action<DragGestureEvent> DragEvent;

        /// <summary>
        /// ドラッグ終了時に通知するイベント
        /// </summary>
        public event Action<DragGestureEvent> EndDragEvent;

        /// <summary>
        /// ドラッグキャンセル時に通知するイベント
        /// </summary>
        public event Action<DragGestureEvent> CancelDragEvent;

        /// <inheritdoc/>
        public override bool CanHandle(Vector2 screenPosition) {
            if (_targetRectTransform == null) {
                return false;
            }

            return RectTransformUtility.RectangleContainsScreenPoint(
                _targetRectTransform,
                screenPosition,
                GetEventCamera());
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
        /// 画面座標判定に使用するイベントカメラを取得
        /// </summary>
        /// <returns>使用するイベントカメラ</returns>
        private Camera GetEventCamera() {
            var canvas = _targetRectTransform.GetComponentInParent<Canvas>();

            if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                return null;
            }

            return canvas.worldCamera;
        }
    }
}
