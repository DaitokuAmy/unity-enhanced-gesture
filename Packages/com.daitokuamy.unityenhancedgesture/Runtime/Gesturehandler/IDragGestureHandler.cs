namespace UnityEnhancedGesture {
    /// <summary>
    /// ドラッグ通知を扱うハンドラー契約
    /// </summary>
    internal interface IDragGestureHandler : IGestureHandler {
        /// <summary>ドラッグ開始しきい値</summary>
        float DragStartThreshold { get; }

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
