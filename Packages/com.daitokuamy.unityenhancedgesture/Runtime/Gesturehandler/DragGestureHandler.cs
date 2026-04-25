using System;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ドラッグ通知を公開するハンドラー
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DragGestureHandler : GestureHandlerBase {
        [SerializeField, Tooltip("ドラッグ開始とみなす移動量")]
        private float _dragStartThreshold = 12f;

        /// <summary>ドラッグ開始しきい値</summary>
        public float DragStartThreshold => _dragStartThreshold;

        /// <summary>ドラッグ開始時に発火されるイベント</summary>
        public event Action<DragGestureEvent> BeginDragEvent;
        /// <summary>ドラッグ更新時に発火されるイベント</summary>
        public event Action<DragGestureEvent> DragEvent;
        /// <summary>ドラッグ終了時に発火されるイベント</summary>
        public event Action<DragGestureEvent> EndDragEvent;
        /// <summary>ドラッグキャンセル時に発火されるイベント</summary>
        public event Action<DragGestureEvent> CancelDragEvent;

        /// <summary>
        /// ドラッグ開始イベントを発火
        /// </summary>
        /// <param name="gestureEvent">通知内容</param>
        internal void RaiseBeginDrag(DragGestureEvent gestureEvent) {
            BeginDragEvent?.Invoke(gestureEvent);
        }

        /// <summary>
        /// ドラッグ更新イベントを発火
        /// </summary>
        /// <param name="gestureEvent">通知内容</param>
        internal void RaiseDrag(DragGestureEvent gestureEvent) {
            DragEvent?.Invoke(gestureEvent);
        }

        /// <summary>
        /// ドラッグ終了イベントを発火
        /// </summary>
        /// <param name="gestureEvent">通知内容</param>
        internal void RaiseEndDrag(DragGestureEvent gestureEvent) {
            EndDragEvent?.Invoke(gestureEvent);
        }

        /// <summary>
        /// ドラッグキャンセルイベントを発火
        /// </summary>
        /// <param name="gestureEvent">通知内容</param>
        internal void RaiseCancelDrag(DragGestureEvent gestureEvent) {
            CancelDragEvent?.Invoke(gestureEvent);
        }
    }
}
