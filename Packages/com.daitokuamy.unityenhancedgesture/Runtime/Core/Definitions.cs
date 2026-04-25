namespace UnityEnhancedGesture {
    /// <summary>
    /// ジェスチャー通知の進行状態
    /// </summary>
    public enum GestureEventPhase {
        /// <summary>開始時</summary>
        Began,
        /// <summary>継続更新時</summary>
        Updated,
        /// <summary>完了時</summary>
        Completed,
        /// <summary>キャンセル時</summary>
        Canceled,
    }

    /// <summary>
    /// タップ系ジェスチャーの種別
    /// </summary>
    public enum TapGestureKind {
        /// <summary>シングルタップ</summary>
        SingleTap,
        /// <summary>ダブルタップ</summary>
        DoubleTap,
        /// <summary>ロングプレス</summary>
        LongPress,
    }

    /// <summary>
    /// 内部で管理する成立済みジェスチャー種別
    /// </summary>
    internal enum GestureRecognitionType {
        /// <summary>未成立</summary>
        None,
        /// <summary>ドラッグ</summary>
        Drag,
        /// <summary>ピンチ</summary>
        Pinch,
        /// <summary>タップ</summary>
        Tap,
        /// <summary>ダブルタップ</summary>
        DoubleTap,
        /// <summary>ロングプレス</summary>
        LongPress,
    }
}
