using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using InputTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using InputTouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace UnityEnhancedGesture {
    /// <summary>
    /// EnhancedTouch から入力を解析する実装
    /// </summary>
    internal sealed class EnhancedTouchGestureInputProvider : IGestureInputProvider {
        /// <inheritdoc/>
        public string NotReadyMessage => "GestureCoordinator is running in External input management mode, but EnhancedTouchSupport is not enabled.";

        /// <inheritdoc/>
        public bool IsReady(GestureInputManagementMode inputManagementMode) {
            return inputManagementMode == GestureInputManagementMode.Automatic || EnhancedTouchSupport.enabled;
        }

        /// <inheritdoc/>
        public void Enable(GestureInputManagementMode inputManagementMode) {
            if (inputManagementMode == GestureInputManagementMode.Automatic) {
                EnhancedTouchSupport.Enable();
            }
        }

        /// <inheritdoc/>
        public void Disable(GestureInputManagementMode inputManagementMode) {
            if (inputManagementMode == GestureInputManagementMode.Automatic) {
                EnhancedTouchSupport.Disable();
            }
        }

        /// <inheritdoc/>
        public void CollectInputs(List<GesturePointerInput> results) {
            foreach (var touch in InputTouch.activeTouches) {
                results.Add(CreateInput(touch));
            }
        }

        /// <summary>
        /// EnhancedTouch から共通入力データを生成
        /// </summary>
        /// <param name="touch">対象タッチ</param>
        /// <returns>解析結果</returns>
        private GesturePointerInput CreateInput(InputTouch touch) {
            return new GesturePointerInput(
                touch.touchId,
                ConvertPhase(touch.phase),
                touch.startScreenPosition,
                touch.screenPosition,
                touch.delta,
                CreatePositions(touch),
                (float)touch.startTime,
                (float)touch.time);
        }

        /// <summary>
        /// タッチフェーズを共通フェーズへ変換
        /// </summary>
        /// <param name="phase">変換元フェーズ</param>
        /// <returns>変換結果</returns>
        private GestureInputPhase ConvertPhase(InputTouchPhase phase) {
            return phase switch {
                InputTouchPhase.Began => GestureInputPhase.Began,
                InputTouchPhase.Moved => GestureInputPhase.Moved,
                InputTouchPhase.Stationary => GestureInputPhase.Stationary,
                InputTouchPhase.Ended => GestureInputPhase.Ended,
                InputTouchPhase.Canceled => GestureInputPhase.Canceled,
                _ => GestureInputPhase.Stationary,
            };
        }

        /// <summary>
        /// 現在タッチの履歴座標列を生成
        /// </summary>
        /// <param name="touch">対象タッチ</param>
        /// <returns>開始から現在までの座標列</returns>
        private Vector2[] CreatePositions(InputTouch touch) {
            var positions = new List<Vector2>(touch.history.Count + 1);

            for (var i = touch.history.Count - 1; i >= 0; i--) {
                AppendPositionIfNeeded(positions, touch.history[i].screenPosition);
            }

            AppendPositionIfNeeded(positions, touch.screenPosition);
            return positions.ToArray();
        }

        /// <summary>
        /// 重複しない場合のみ座標を追加
        /// </summary>
        /// <param name="positions">追加先</param>
        /// <param name="position">追加候補</param>
        private void AppendPositionIfNeeded(List<Vector2> positions, Vector2 position) {
            if (positions.Count > 0 && positions[positions.Count - 1] == position) {
                return;
            }

            positions.Add(position);
        }
    }
}
