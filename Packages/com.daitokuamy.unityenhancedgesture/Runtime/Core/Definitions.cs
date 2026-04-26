namespace UnityEnhancedGesture {
    /// <summary>
    /// ジェスチャー通知の進行状態
    /// </summary>
    public enum DragGestureStartMode {
        Immediate,
        LongTap,
    }

    public enum GestureEventPhase {
        Began,
        Updated,
        Completed,
        Canceled,
    }

    /// <summary>
    /// 入力更新の駆動方式
    /// </summary>
    public enum GestureCoordinatorUpdateMode {
        Update,
        ManualUpdate,
    }

    /// <summary>
    /// 入力システム有効化の管理方式
    /// </summary>
    public enum GestureInputManagementMode {
        Automatic,
        External,
    }

    /// <summary>
    /// 入力解析後の進行状態
    /// </summary>
    public enum GestureInputPhase {
        Began,
        Moved,
        Stationary,
        Ended,
        Canceled,
    }

    public enum TapGestureType {
        SingleTap,
        DoubleTap,
        LongTap,
    }
}
