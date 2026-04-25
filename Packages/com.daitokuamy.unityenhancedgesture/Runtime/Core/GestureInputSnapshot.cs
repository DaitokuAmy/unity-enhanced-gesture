using System;
using UnityEngine;
using InputTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using InputTouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace UnityEnhancedGesture {
    /// <summary>
    /// 認識器へ渡す入力スナップショット
    /// </summary>
    internal readonly struct GestureInputSnapshot {
        private readonly InputTouch _primaryTouch;
        private readonly InputTouch? _secondaryTouch;

        /// <summary>
        /// 入力スナップショットを初期化
        /// </summary>
        /// <param name="primaryTouch">主タッチ</param>
        /// <param name="secondaryTouch">副タッチ</param>
        public GestureInputSnapshot(InputTouch primaryTouch, InputTouch? secondaryTouch) {
            _primaryTouch = primaryTouch;
            _secondaryTouch = secondaryTouch;
        }

        /// <summary>主タッチ</summary>
        public InputTouch PrimaryTouch => _primaryTouch;
        /// <summary>副タッチ</summary>
        public InputTouch SecondaryTouch {
            get {
                if (!_secondaryTouch.HasValue) {
                    throw new InvalidOperationException("Secondary touch is not available.");
                }

                return _secondaryTouch.Value;
            }
        }

        /// <summary>副タッチを保持しているかどうか</summary>
        public bool HasSecondaryTouch => _secondaryTouch.HasValue;
        /// <summary>保持しているタッチ数</summary>
        public int TouchCount => HasSecondaryTouch ? 2 : 1;
        /// <summary>主タッチの現在位置</summary>
        public Vector2 PrimaryPosition => _primaryTouch.screenPosition;
        /// <summary>副タッチの現在位置</summary>
        public Vector2 SecondaryPosition => SecondaryTouch.screenPosition;
        /// <summary>利用中タッチの中心座標</summary>
        public Vector2 Center => HasSecondaryTouch ? (PrimaryPosition + SecondaryPosition) * 0.5f : PrimaryPosition;
        /// <summary>主タッチが終了したかどうか</summary>
        public bool IsPrimaryEnded => _primaryTouch.ended;
        /// <summary>主タッチがキャンセルされたかどうか</summary>
        public bool IsPrimaryCanceled => _primaryTouch.phase == InputTouchPhase.Canceled;
        /// <summary>いずれかのタッチが終了したかどうか</summary>
        public bool HasAnyEndedTouch => _primaryTouch.ended || (HasSecondaryTouch && SecondaryTouch.ended);
        /// <summary>いずれかのタッチがキャンセルされたかどうか</summary>
        public bool HasAnyCanceledTouch => _primaryTouch.phase == InputTouchPhase.Canceled || (HasSecondaryTouch && SecondaryTouch.phase == InputTouchPhase.Canceled);
        /// <summary>いずれかのタッチが移動したかどうか</summary>
        public bool HasAnyMovement => _primaryTouch.phase == InputTouchPhase.Moved || (HasSecondaryTouch && SecondaryTouch.phase == InputTouchPhase.Moved);
        /// <summary>主タッチの経過時間</summary>
        public float PrimaryElapsedTime => (float)(_primaryTouch.time - _primaryTouch.startTime);
        /// <summary>主タッチの総移動距離</summary>
        public float PrimaryTravelDistance => Vector2.Distance(_primaryTouch.startScreenPosition, _primaryTouch.screenPosition);
    }
}
