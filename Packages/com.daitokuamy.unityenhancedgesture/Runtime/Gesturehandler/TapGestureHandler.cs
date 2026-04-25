using System;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// タップ系通知を公開するハンドラー
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TapGestureHandler : GestureHandlerBase {
        [SerializeField, Tooltip("タップ中に許容する移動量")]
        private float _maxTapMovement = 12f;
        [SerializeField, Tooltip("シングルタップとして許容する最大時間")]
        private float _maxTapDuration = 0.25f;
        [SerializeField, Tooltip("ダブルタップ判定時間")]
        private float _doubleTapTime = 0.25f;
        [SerializeField, Tooltip("ロングプレス判定時間")]
        private float _longPressDuration = 0.5f;
        [SerializeField, Tooltip("ダブルタップを有効にする場合は true")]
        private bool _enableDoubleTap = true;
        [SerializeField, Tooltip("ロングプレスを有効にする場合は true")]
        private bool _enableLongPress = true;

        /// <summary>タップ中に許容する移動量</summary>
        public float MaxTapMovement => _maxTapMovement;
        /// <summary>シングルタップとして許容する最大時間</summary>
        public float MaxTapDuration => _maxTapDuration;
        /// <summary>ダブルタップ判定時間</summary>
        public float DoubleTapTime => _doubleTapTime;
        /// <summary>ロングプレス判定時間</summary>
        public float LongPressDuration => _longPressDuration;
        /// <summary>ダブルタップを有効にするかどうか</summary>
        public bool EnableDoubleTap => _enableDoubleTap;
        /// <summary>ロングプレスを有効にするかどうか</summary>
        public bool EnableLongPress => _enableLongPress;

        /// <summary>シングルタップ確定時に発火されるイベント</summary>
        public event Action<TapGestureEvent> TapEvent;
        /// <summary>ダブルタップ確定時に発火されるイベント</summary>
        public event Action<TapGestureEvent> DoubleTapEvent;
        /// <summary>ロングプレス通知時に発火されるイベント</summary>
        public event Action<TapGestureEvent> LongPressEvent;
        /// <summary>タップ系ジェスチャーのキャンセル時に発火されるイベント</summary>
        public event Action<TapGestureEvent> CancelTapEvent;

        /// <summary>
        /// シングルタップイベントを発火
        /// </summary>
        /// <param name="gestureEvent">通知内容</param>
        internal void RaiseTap(TapGestureEvent gestureEvent) {
            TapEvent?.Invoke(gestureEvent);
        }

        /// <summary>
        /// ダブルタップイベントを発火
        /// </summary>
        /// <param name="gestureEvent">通知内容</param>
        internal void RaiseDoubleTap(TapGestureEvent gestureEvent) {
            DoubleTapEvent?.Invoke(gestureEvent);
        }

        /// <summary>
        /// ロングプレスイベントを発火
        /// </summary>
        /// <param name="gestureEvent">通知内容</param>
        internal void RaiseLongPress(TapGestureEvent gestureEvent) {
            LongPressEvent?.Invoke(gestureEvent);
        }

        /// <summary>
        /// タップキャンセルイベントを発火
        /// </summary>
        /// <param name="gestureEvent">通知内容</param>
        internal void RaiseCancelTap(TapGestureEvent gestureEvent) {
            CancelTapEvent?.Invoke(gestureEvent);
        }
    }
}
