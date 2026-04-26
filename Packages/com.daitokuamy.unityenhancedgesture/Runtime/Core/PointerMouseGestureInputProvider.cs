using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityEnhancedGesture {
    /// <summary>
    /// Pointer 系のマウス入力を解析する実装
    /// </summary>
    internal sealed class PointerMouseGestureInputProvider : IGestureInputProvider {
        private const int MousePointerId = -1;

        private readonly List<Vector2> _positions = new();

        private bool _isPressed;
        private Vector2 _startPosition;
        private float _startTime;

        /// <inheritdoc/>
        public string NotReadyMessage => "GestureCoordinator could not find a mouse device for pointer input.";

        /// <inheritdoc/>
        public bool IsReady(GestureInputManagementMode inputManagementMode) {
            return Mouse.current != null;
        }

        /// <inheritdoc/>
        public void Enable(GestureInputManagementMode inputManagementMode) {
            ResetState();
        }

        /// <inheritdoc/>
        public void Disable(GestureInputManagementMode inputManagementMode) {
            ResetState();
        }

        /// <inheritdoc/>
        public void CollectInputs(List<GesturePointerInput> results) {
            if (Mouse.current == null) {
                return;
            }

            var mouse = Mouse.current;
            var position = mouse.position.ReadValue();
            var delta = mouse.delta.ReadValue();
            var time = Time.unscaledTime;

            if (mouse.leftButton.wasPressedThisFrame) {
                _isPressed = true;
                _startPosition = position;
                _startTime = time;
                _positions.Clear();
                AppendPositionIfNeeded(position);
                results.Add(CreateInput(GestureInputPhase.Began, position, delta, time));
                return;
            }

            if (!_isPressed) {
                return;
            }

            AppendPositionIfNeeded(position);

            if (mouse.leftButton.wasReleasedThisFrame) {
                results.Add(CreateInput(GestureInputPhase.Ended, position, delta, time));
                ResetState();
                return;
            }

            if (mouse.leftButton.isPressed) {
                var phase = delta == Vector2.zero ? GestureInputPhase.Stationary : GestureInputPhase.Moved;
                results.Add(CreateInput(phase, position, delta, time));
                return;
            }

            results.Add(CreateInput(GestureInputPhase.Canceled, position, delta, time));
            ResetState();
        }

        /// <summary>
        /// 現在状態から共通入力データを生成
        /// </summary>
        /// <param name="phase">現在フェーズ</param>
        /// <param name="position">現在位置</param>
        /// <param name="delta">前回差分</param>
        /// <param name="time">現在時刻</param>
        /// <returns>生成結果</returns>
        private GesturePointerInput CreateInput(GestureInputPhase phase, Vector2 position, Vector2 delta, float time) {
            return new GesturePointerInput(
                MousePointerId,
                phase,
                _startPosition,
                position,
                delta,
                _positions.ToArray(),
                _startTime,
                time);
        }

        /// <summary>
        /// 直前座標と異なる場合のみ履歴へ追加
        /// </summary>
        /// <param name="position">追加候補</param>
        private void AppendPositionIfNeeded(Vector2 position) {
            if (_positions.Count > 0 && _positions[_positions.Count - 1] == position) {
                return;
            }

            _positions.Add(position);
        }

        /// <summary>
        /// 内部状態を初期化
        /// </summary>
        private void ResetState() {
            _isPressed = false;
            _startPosition = Vector2.zero;
            _startTime = 0.0f;
            _positions.Clear();
        }
    }
}
