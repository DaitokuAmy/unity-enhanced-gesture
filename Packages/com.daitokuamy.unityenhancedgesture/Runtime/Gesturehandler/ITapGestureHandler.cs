namespace UnityEnhancedGesture {
    /// <summary>
    /// タップイベントを扱うハンドラー契約
    /// </summary>
    internal interface ITapGestureHandler : IGestureHandler {
        /// <summary>単一タップとして認める最大継続時間</summary>
        float MaxTapDuration { get; }
        /// <summary>単一タップとして認める最大移動量</summary>
        float MaxTapMovement { get; }
        /// <summary>ダブルタップを有効化するかどうか</summary>
        bool EnableDoubleTap { get; }
        /// <summary>ダブルタップ成立までの最大待機時間</summary>
        float DoubleTapMaxDelay { get; }
        /// <summary>ダブルタップ成立までの最大位置差</summary>
        float DoubleTapMaxMovement { get; }
        /// <summary>ロングタップを有効化するかどうか</summary>
        bool EnableLongTap { get; }
        /// <summary>ロングタップ成立までの待機時間</summary>
        float LongTapDuration { get; }
        /// <summary>ロングタップ成立までの許容移動量</summary>
        float LongTapMaxMovement { get; }

        /// <summary>
        /// 単一タップイベントを受け取る
        /// </summary>
        /// <param name="gestureEvent">イベント引数</param>
        void HandleTap(TapGestureEvent gestureEvent);

        /// <summary>
        /// ダブルタップイベントを受け取る
        /// </summary>
        /// <param name="gestureEvent">イベント引数</param>
        void HandleDoubleTap(TapGestureEvent gestureEvent);

        /// <summary>
        /// ロングタップイベントを受け取る
        /// </summary>
        /// <param name="gestureEvent">イベント引数</param>
        void HandleLongTap(TapGestureEvent gestureEvent);

        /// <summary>
        /// ロングタップ進捗イベントを受け取る
        /// </summary>
        /// <param name="gestureEvent">イベント引数</param>
        void HandleLongTapProgress(LongTapProgressGestureEvent gestureEvent);
    }
}
