using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityEnhancedGesture {
    /// <summary>
    /// Pointer 系のマウス入力を解釈する入力解析器
    /// </summary>
    internal sealed class PointerMouseGestureInputProvider : IGestureInputProvider {
        private const int MousePrimaryPointerId = -1;
        private const int MouseSecondaryPointerId = -2;

        private readonly List<GesturePointerSample> _primarySamples = new();
        private readonly List<GesturePointerSample> _secondarySamples = new();

        private bool _isPressed;
        private bool _isSimulatingPinch;
        private bool _hasVisualizationCenter;
        private Vector2 _pinchCenter;
        private Vector2 _visualizationCenter;
        private Vector2 _startPosition;
        private Vector2 _secondaryStartPosition;
        private float _startTime;
        private float _secondaryStartTime;
        private Vector2 _previousPrimaryPosition;
        private Vector2 _previousSecondaryPosition;

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
            var isAltPressed = IsAltPressed();

            UpdateVisualizationCenter(position, isAltPressed);

            if (mouse.leftButton.wasPressedThisFrame) {
                BeginPrimaryPointer(position, time);
                results.Add(CreatePrimaryInput(GestureInputPhase.Began, position, delta, time));

                if (isAltPressed) {
                    BeginSecondaryPointer(position, time);
                    results.Add(CreateSecondaryInput(GestureInputPhase.Began, _secondaryStartPosition, Vector2.zero, time));
                }

                return;
            }

            if (!_isPressed) {
                return;
            }

            AppendSampleIfNeeded(
                _primarySamples,
                position,
                time - _startTime,
                mouse.leftButton.wasReleasedThisFrame);

            if (_isSimulatingPinch) {
                ProcessPinchSimulation(results, mouse, position, delta, time, isAltPressed);
                _previousPrimaryPosition = position;
                return;
            }

            if (isAltPressed) {
                BeginSecondaryPointer(position, time);
                results.Add(CreatePrimaryInput(GetContinuousPhase(delta), position, delta, time));
                results.Add(CreateSecondaryInput(GestureInputPhase.Began, _secondaryStartPosition, Vector2.zero, time));
                _previousPrimaryPosition = position;
                return;
            }

            if (mouse.leftButton.wasReleasedThisFrame) {
                results.Add(CreatePrimaryInput(GestureInputPhase.Ended, position, delta, time));
                ResetState();
                return;
            }

            if (mouse.leftButton.isPressed) {
                results.Add(CreatePrimaryInput(GetContinuousPhase(delta), position, delta, time));
                _previousPrimaryPosition = position;
                return;
            }

            results.Add(CreatePrimaryInput(GestureInputPhase.Canceled, position, delta, time));
            ResetState();
        }

        /// <summary>
        /// プライマリーポインターを開始
        /// </summary>
        /// <param name="position">開始位置</param>
        /// <param name="time">開始時刻</param>
        private void BeginPrimaryPointer(Vector2 position, float time) {
            _isPressed = true;
            _startPosition = position;
            _startTime = time;
            _previousPrimaryPosition = position;
            _primarySamples.Clear();
            AppendSampleIfNeeded(_primarySamples, position, 0.0f);
        }

        /// <summary>
        /// セカンダリーポインターを開始
        /// </summary>
        /// <param name="centerPosition">ピンチ中心位置</param>
        /// <param name="time">開始時刻</param>
        private void BeginSecondaryPointer(Vector2 centerPosition, float time) {
            _isSimulatingPinch = true;
            _pinchCenter = _hasVisualizationCenter ? _visualizationCenter : centerPosition;
            _secondaryStartPosition = centerPosition;
            _secondaryStartTime = time;
            _previousSecondaryPosition = _pinchCenter;
            _secondarySamples.Clear();
            AppendSampleIfNeeded(_secondarySamples, _pinchCenter, 0.0f);
        }

        /// <summary>
        /// ピンチシミュレーション入力を処理
        /// </summary>
        /// <param name="results">出力先</param>
        /// <param name="mouse">マウスデバイス</param>
        /// <param name="position">現在位置</param>
        /// <param name="delta">プライマリー差分量</param>
        /// <param name="time">現在時刻</param>
        /// <param name="isAltPressed">Alt 押下状態</param>
        private void ProcessPinchSimulation(List<GesturePointerInput> results, Mouse mouse, Vector2 position, Vector2 delta, float time, bool isAltPressed) {
            var secondaryPosition = GetSecondaryPosition(position);
            var secondaryDelta = secondaryPosition - _previousSecondaryPosition;
            AppendSampleIfNeeded(_secondarySamples, secondaryPosition, time - _secondaryStartTime);

            if (mouse.leftButton.wasReleasedThisFrame) {
                results.Add(CreatePrimaryInput(GestureInputPhase.Ended, position, delta, time));
                results.Add(CreateSecondaryInput(GestureInputPhase.Ended, secondaryPosition, secondaryDelta, time));
                ResetState();
                return;
            }

            if (!isAltPressed) {
                results.Add(CreatePrimaryInput(GetContinuousPhase(delta), position, delta, time));
                results.Add(CreateSecondaryInput(GestureInputPhase.Ended, secondaryPosition, secondaryDelta, time));
                _isSimulatingPinch = false;
                _secondarySamples.Clear();
                _previousSecondaryPosition = Vector2.zero;
                return;
            }

            if (mouse.leftButton.isPressed) {
                results.Add(CreatePrimaryInput(GetContinuousPhase(delta), position, delta, time));
                results.Add(CreateSecondaryInput(GetContinuousPhase(secondaryDelta), secondaryPosition, secondaryDelta, time));
                _previousSecondaryPosition = secondaryPosition;
                return;
            }

            results.Add(CreatePrimaryInput(GestureInputPhase.Canceled, position, delta, time));
            results.Add(CreateSecondaryInput(GestureInputPhase.Canceled, secondaryPosition, secondaryDelta, time));
            ResetState();
        }

        /// <summary>
        /// プライマリー入力を生成
        /// </summary>
        /// <param name="phase">入力フェーズ</param>
        /// <param name="position">現在位置</param>
        /// <param name="delta">差分量</param>
        /// <param name="time">現在時刻</param>
        /// <returns>生成した入力</returns>
        private GesturePointerInput CreatePrimaryInput(GestureInputPhase phase, Vector2 position, Vector2 delta, float time) {
            return new GesturePointerInput(
                MousePrimaryPointerId,
                phase,
                _startPosition,
                position,
                delta,
                _primarySamples.ToArray(),
                _startTime,
                time);
        }

        /// <summary>
        /// セカンダリー入力を生成
        /// </summary>
        /// <param name="phase">入力フェーズ</param>
        /// <param name="position">現在位置</param>
        /// <param name="delta">差分量</param>
        /// <param name="time">現在時刻</param>
        /// <returns>生成した入力</returns>
        private GesturePointerInput CreateSecondaryInput(GestureInputPhase phase, Vector2 position, Vector2 delta, float time) {
            return new GesturePointerInput(
                MouseSecondaryPointerId,
                phase,
                _secondaryStartPosition,
                position,
                delta,
                _secondarySamples.ToArray(),
                _secondaryStartTime,
                time);
        }

        /// <summary>
        /// 継続入力フェーズを取得
        /// </summary>
        /// <param name="delta">差分量</param>
        /// <returns>継続入力フェーズ</returns>
        private GestureInputPhase GetContinuousPhase(Vector2 delta) {
            return delta == Vector2.zero ? GestureInputPhase.Stationary : GestureInputPhase.Moved;
        }

        /// <summary>
        /// ピンチ用セカンダリー座標を取得
        /// </summary>
        /// <param name="primaryPosition">プライマリー座標</param>
        /// <returns>セカンダリー座標</returns>
        private Vector2 GetSecondaryPosition(Vector2 primaryPosition) {
            return (_pinchCenter * 2.0f) - primaryPosition;
        }

        /// <summary>
        /// 可視化用のシミュレーション座標を取得
        /// </summary>
        /// <param name="hasCenter">中央点を表示する場合は true</param>
        /// <param name="center">中央点座標</param>
        /// <param name="hasPointerPair">1点目と2点目を表示する場合は true</param>
        /// <param name="primaryPosition">1点目座標</param>
        /// <param name="secondaryPosition">2点目座標</param>
        /// <returns>取得できた場合は true</returns>
        internal bool TryGetSimulationGuiData(
            out bool hasCenter,
            out Vector2 center,
            out bool hasPointerPair,
            out Vector2 primaryPosition,
            out Vector2 secondaryPosition) {
#if UNITY_EDITOR
            hasCenter = _hasVisualizationCenter;
            center = default;
            hasPointerPair = false;
            primaryPosition = default;
            secondaryPosition = default;

            if (_hasVisualizationCenter) {
                center = _visualizationCenter;
            }

            if (!_hasVisualizationCenter) {
                return false;
            }

            if (_isSimulatingPinch && Mouse.current != null) {
                hasPointerPair = true;
                primaryPosition = Mouse.current.position.ReadValue();
                secondaryPosition = GetSecondaryPosition(primaryPosition);
            }

            return true;
#else
            hasCenter = false;
            center = default;
            hasPointerPair = false;
            primaryPosition = default;
            secondaryPosition = default;
            return false;
#endif
        }

        /// <summary>
        /// 可視化用の中央点状態を更新
        /// </summary>
        /// <param name="position">現在位置</param>
        /// <param name="isAltPressed">Alt 押下状態</param>
        private void UpdateVisualizationCenter(Vector2 position, bool isAltPressed) {
            var isAltReleasedThisFrame = Keyboard.current != null
                && (Keyboard.current.leftAltKey.wasReleasedThisFrame || Keyboard.current.rightAltKey.wasReleasedThisFrame);
            var isAltPressedThisFrame = Keyboard.current != null
                && (Keyboard.current.leftAltKey.wasPressedThisFrame || Keyboard.current.rightAltKey.wasPressedThisFrame);

            if (!isAltPressed || isAltReleasedThisFrame) {
                _hasVisualizationCenter = false;
                _visualizationCenter = Vector2.zero;
                return;
            }

            if (isAltPressedThisFrame) {
                _visualizationCenter = position;
                _hasVisualizationCenter = true;
            }
        }

        /// <summary>
        /// Alt キー押下状態を判定
        /// </summary>
        /// <returns>Alt キー押下中の場合は true</returns>
        private bool IsAltPressed() {
            return Keyboard.current != null
                && (Keyboard.current.leftAltKey.isPressed || Keyboard.current.rightAltKey.isPressed);
        }

        /// <summary>
        /// 履歴へサンプルを追加
        /// </summary>
        /// <param name="samples">追加先サンプル一覧</param>
        /// <param name="position">サンプル位置</param>
        /// <param name="elapsedTime">開始からの経過時間</param>
        /// <param name="allowDuplicatePosition">同一位置の追加を許可するかどうか</param>
        private void AppendSampleIfNeeded(
            List<GesturePointerSample> samples,
            Vector2 position,
            float elapsedTime,
            bool allowDuplicatePosition = false) {
            if (!allowDuplicatePosition
                && samples.Count > 0
                && samples[samples.Count - 1].Position == position) {
                return;
            }

            samples.Add(new GesturePointerSample(position, elapsedTime));
        }

        /// <summary>
        /// 内部状態を初期化
        /// </summary>
        private void ResetState() {
            _isPressed = false;
            _isSimulatingPinch = false;
            _pinchCenter = Vector2.zero;
            _startPosition = Vector2.zero;
            _secondaryStartPosition = Vector2.zero;
            _startTime = 0.0f;
            _secondaryStartTime = 0.0f;
            _previousPrimaryPosition = Vector2.zero;
            _previousSecondaryPosition = Vector2.zero;
            _primarySamples.Clear();
            _secondarySamples.Clear();
        }
    }
}
