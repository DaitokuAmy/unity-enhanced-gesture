namespace UnityEnhancedGesture {
    /// <summary>
    /// ドラッグ通知を扱うハンドラー契約
    /// </summary>
    internal interface IDragGestureHandler : IGestureHandler {
        /// <summary>ドラッグ開始しきい値</summary>
        float DragStartThreshold { get; }
        /// <summary>ロングタップドラッグを有効化するかどうか</summary>
        bool EnableLongTapDrag { get; }
        /// <summary>ロングタップドラッグ開始までの待機時間</summary>
        float LongTapDragDuration { get; }
        /// <summary>ロングタップドラッグ成立までの許容移動量</summary>
        float LongTapDragMaxMovement { get; }

        /// <summary>
        /// ドラッグ開始通知を受け取る
        /// </summary>
        /// <param name="gestureEvent">通知内容</param>
        void HandleBeginDrag(DragGestureEvent gestureEvent);

        /// <summary>
        /// ドラッグ更新通知を受け取る
        /// </summary>
        /// <param name="gestureEvent">通知内容</param>
        void HandleDrag(DragGestureEvent gestureEvent);

        /// <summary>
        /// ドラッグ終了通知を受け取る
        /// </summary>
        /// <param name="gestureEvent">通知内容</param>
        void HandleEndDrag(DragGestureEvent gestureEvent);

        /// <summary>
        /// ドラッグキャンセル通知を受け取る
        /// </summary>
        /// <param name="gestureEvent">通知内容</param>
        void HandleCancelDrag(DragGestureEvent gestureEvent);
    }
}
