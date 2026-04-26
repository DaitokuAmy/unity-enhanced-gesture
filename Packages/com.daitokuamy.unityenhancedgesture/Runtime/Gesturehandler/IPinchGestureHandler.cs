namespace UnityEnhancedGesture {
    /// <summary>
    /// ピンチイベントを扱うハンドラー契約
    /// </summary>
    internal interface IPinchGestureHandler : IGestureHandler {
        /// <summary>ピンチ開始しきい値</summary>
        float PinchStartThreshold { get; }

        /// <summary>
        /// ピンチ開始イベントを受け取る
        /// </summary>
        /// <param name="gestureEvent">イベント引数</param>
        void HandleBeginPinch(PinchGestureEvent gestureEvent);

        /// <summary>
        /// ピンチ更新イベントを受け取る
        /// </summary>
        /// <param name="gestureEvent">イベント引数</param>
        void HandlePinch(PinchGestureEvent gestureEvent);

        /// <summary>
        /// ピンチ終了イベントを受け取る
        /// </summary>
        /// <param name="gestureEvent">イベント引数</param>
        void HandleEndPinch(PinchGestureEvent gestureEvent);

        /// <summary>
        /// ピンチキャンセルイベントを受け取る
        /// </summary>
        /// <param name="gestureEvent">イベント引数</param>
        void HandleCancelPinch(PinchGestureEvent gestureEvent);
    }
}
