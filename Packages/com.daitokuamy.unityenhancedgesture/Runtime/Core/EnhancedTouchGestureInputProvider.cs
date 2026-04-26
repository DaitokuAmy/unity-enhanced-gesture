using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using InputTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using InputTouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace UnityEnhancedGesture {
    /// <summary>
    /// EnhancedTouch から入力を解釈する入力解析器
    /// </summary>
    internal sealed class EnhancedTouchGestureInputProvider : IGestureInputProvider {
        private const int SimulatedSecondaryPointerId = -100;

        private readonly List<GesturePointerSample> _simulatedSecondarySamples = new();

        private bool _hasVisualizationCenter;
        private bool _isSimulatingPinch;
        private int _simulatedPrimaryPointerId;
        private Vector2 _simulatedStartCenter;
        private Vector2 _visualizationCenter;
        private float _simulatedStartTime;
        private Vector2 _previousSecondaryPosition;

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

            ResetSimulation();
        }

        /// <inheritdoc/>
        public void Disable(GestureInputManagementMode inputManagementMode) {
            if (inputManagementMode == GestureInputManagementMode.Automatic) {
                EnhancedTouchSupport.Disable();
            }

            ResetSimulation();
        }

        /// <inheritdoc/>
        public void CollectInputs(List<GesturePointerInput> results) {
            foreach (var touch in InputTouch.activeTouches) {
                results.Add(CreateInput(touch));
            }

            UpdateVisualizationCenter(results);
            CollectEditorSimulatedPinch(results);
        }

        /// <summary>
        /// EnhancedTouch を共通入力へ変換
        /// </summary>
        /// <param name="touch">対象タッチ</param>
        /// <returns>変換後入力</returns>
        private GesturePointerInput CreateInput(InputTouch touch) {
            return new GesturePointerInput(
                touch.touchId,
                ConvertPhase(touch.phase),
                touch.startScreenPosition,
                touch.screenPosition,
                touch.delta,
                CreateSamples(touch),
                (float)touch.startTime,
                (float)touch.time);
        }

        /// <summary>
        /// Editor でピンチ用の 2 本目入力を合成
        /// </summary>
        /// <param name="results">出力先</param>
        private void CollectEditorSimulatedPinch(List<GesturePointerInput> results) {
#if UNITY_EDITOR
            if (!Application.isEditor) {
                return;
            }

            var primaryTouchCount = 0;
            var primaryInputIndex = -1;

            for (var i = 0; i < results.Count; i++) {
                if (results[i].PointerId == SimulatedSecondaryPointerId) {
                    continue;
                }

                primaryTouchCount++;

                if (primaryInputIndex < 0) {
                    primaryInputIndex = i;
                }
            }

            if (primaryTouchCount != 1 || primaryInputIndex < 0) {
                ResetSimulation();
                return;
            }

            if (!IsAltPressed()) {
                TryEmitSimulatedEnd(results, results[primaryInputIndex]);
                return;
            }

            var primaryInput = results[primaryInputIndex];

            if (!_isSimulatingPinch || _simulatedPrimaryPointerId != primaryInput.PointerId) {
                BeginSimulation(primaryInput);
                var simulatedSecondaryPosition = GetSecondaryPosition(primaryInput.Position, _simulatedStartCenter);
                results.Add(CreateSimulatedInput(GestureInputPhase.Began, simulatedSecondaryPosition, Vector2.zero, primaryInput.Time));
                return;
            }

            var secondaryPosition = GetSecondaryPosition(primaryInput.Position, _simulatedStartCenter);
            var secondaryDelta = secondaryPosition - _previousSecondaryPosition;
            AppendSampleIfNeeded(_simulatedSecondarySamples, secondaryPosition, primaryInput.Time - _simulatedStartTime);
            _previousSecondaryPosition = secondaryPosition;
            results.Add(CreateSimulatedInput(GetContinuousPhase(secondaryDelta), secondaryPosition, secondaryDelta, primaryInput.Time));
#else
            _ = results;
#endif
        }

        /// <summary>
        /// Alt キー解除時に合成入力の終了を追加
        /// </summary>
        /// <param name="results">出力先</param>
        /// <param name="primaryInput">現在の主入力</param>
        private void TryEmitSimulatedEnd(List<GesturePointerInput> results, GesturePointerInput primaryInput) {
            if (!_isSimulatingPinch || _simulatedPrimaryPointerId != primaryInput.PointerId) {
                return;
            }

            var secondaryPosition = GetSecondaryPosition(primaryInput.Position, _simulatedStartCenter);
            var secondaryDelta = secondaryPosition - _previousSecondaryPosition;
            AppendSampleIfNeeded(_simulatedSecondarySamples, secondaryPosition, primaryInput.Time - _simulatedStartTime);
            results.Add(CreateSimulatedInput(GestureInputPhase.Ended, secondaryPosition, secondaryDelta, primaryInput.Time));
            ResetSimulation();
        }

        /// <summary>
        /// 合成ピンチ状態を開始
        /// </summary>
        /// <param name="primaryInput">現在の主入力</param>
        private void BeginSimulation(GesturePointerInput primaryInput) {
            _isSimulatingPinch = true;
            _simulatedPrimaryPointerId = primaryInput.PointerId;
            _simulatedStartCenter = _hasVisualizationCenter ? _visualizationCenter : primaryInput.Position;
            _simulatedStartTime = primaryInput.StartTime;
            _previousSecondaryPosition = _simulatedStartCenter;
            _simulatedSecondarySamples.Clear();
            AppendSampleIfNeeded(_simulatedSecondarySamples, _simulatedStartCenter, primaryInput.Time - _simulatedStartTime);
        }

        /// <summary>
        /// 合成 2 本目入力を生成
        /// </summary>
        /// <param name="phase">入力フェーズ</param>
        /// <param name="position">現在位置</param>
        /// <param name="delta">差分量</param>
        /// <param name="time">現在時刻</param>
        /// <returns>生成した入力</returns>
        private GesturePointerInput CreateSimulatedInput(GestureInputPhase phase, Vector2 position, Vector2 delta, float time) {
            return new GesturePointerInput(
                SimulatedSecondaryPointerId,
                phase,
                _simulatedStartCenter,
                position,
                delta,
                _simulatedSecondarySamples.ToArray(),
                _simulatedStartTime,
                time);
        }

        /// <summary>
        /// タッチフェーズを共通フェーズへ変換
        /// </summary>
        /// <param name="phase">変換元フェーズ</param>
        /// <returns>変換後フェーズ</returns>
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
        /// タッチ履歴からサンプル列を生成
        /// </summary>
        /// <param name="touch">対象タッチ</param>
        /// <returns>サンプル列</returns>
        private GesturePointerSample[] CreateSamples(InputTouch touch) {
            var samples = new List<GesturePointerSample>(touch.history.Count + 1);

            for (var i = touch.history.Count - 1; i >= 0; i--) {
                var historyTouch = touch.history[i];
                AppendSampleIfNeeded(
                    samples,
                    historyTouch.screenPosition,
                    (float)(historyTouch.time - touch.startTime));
            }

            AppendSampleIfNeeded(samples, touch.screenPosition, (float)(touch.time - touch.startTime));
            return samples.ToArray();
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
        /// 対称位置の 2 本目座標を取得
        /// </summary>
        /// <param name="primaryPosition">1 本目の現在位置</param>
        /// <param name="startCenter">開始中心位置</param>
        /// <returns>2 本目の現在位置</returns>
        private Vector2 GetSecondaryPosition(Vector2 primaryPosition, Vector2 startCenter) {
            return (startCenter * 2.0f) - primaryPosition;
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

            if (_isSimulatingPinch && TryGetSimulatedPrimaryPosition(out primaryPosition)) {
                hasPointerPair = true;
                secondaryPosition = GetSecondaryPosition(primaryPosition, _simulatedStartCenter);
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
        /// <param name="results">現在フレームの入力一覧</param>
        private void UpdateVisualizationCenter(List<GesturePointerInput> results) {
#if UNITY_EDITOR
            var isAltPressed = IsAltPressed();
            var isAltReleasedThisFrame = Keyboard.current != null
                && (Keyboard.current.leftAltKey.wasReleasedThisFrame || Keyboard.current.rightAltKey.wasReleasedThisFrame);
            var isAltPressedThisFrame = Keyboard.current != null
                && (Keyboard.current.leftAltKey.wasPressedThisFrame || Keyboard.current.rightAltKey.wasPressedThisFrame);

            if (!isAltPressed || isAltReleasedThisFrame) {
                _hasVisualizationCenter = false;
                _visualizationCenter = Vector2.zero;
                return;
            }

            if (!isAltPressedThisFrame) {
                return;
            }

            if (!TryGetVisualizationCenterPosition(results, out var centerPosition)) {
                return;
            }

            _visualizationCenter = centerPosition;
            _hasVisualizationCenter = true;
#else
            _ = results;
#endif
        }

        /// <summary>
        /// 可視化用中央点の取得元座標を解決
        /// </summary>
        /// <param name="results">現在フレームの入力一覧</param>
        /// <param name="centerPosition">取得した中央点座標</param>
        /// <returns>取得できた場合は true</returns>
        private bool TryGetVisualizationCenterPosition(List<GesturePointerInput> results, out Vector2 centerPosition) {
            centerPosition = default;

            if (results.Count > 0) {
                centerPosition = results[0].Position;
                return true;
            }

            if (Pointer.current != null) {
                centerPosition = Pointer.current.position.ReadValue();
                return true;
            }

            if (Mouse.current != null) {
                centerPosition = Mouse.current.position.ReadValue();
                return true;
            }

            centerPosition = Input.mousePosition;
            return centerPosition != Vector2.zero;
        }

        /// <summary>
        /// 合成ピンチ状態を初期化
        /// </summary>
        private void ResetSimulation() {
            _isSimulatingPinch = false;
            _simulatedPrimaryPointerId = 0;
            _simulatedStartCenter = Vector2.zero;
            _simulatedStartTime = 0.0f;
            _previousSecondaryPosition = Vector2.zero;
            _simulatedSecondarySamples.Clear();
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
        /// シミュレーション対象の 1 本目位置を取得
        /// </summary>
        /// <param name="primaryPosition">取得した 1 本目位置</param>
        /// <returns>取得できた場合は true</returns>
        private bool TryGetSimulatedPrimaryPosition(out Vector2 primaryPosition) {
            primaryPosition = default;

            for (var i = 0; i < InputTouch.activeTouches.Count; i++) {
                var touch = InputTouch.activeTouches[i];

                if (touch.touchId != _simulatedPrimaryPointerId) {
                    continue;
                }

                primaryPosition = touch.screenPosition;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 重複しない場合のみサンプルを追加
        /// </summary>
        /// <param name="samples">追加先サンプル一覧</param>
        /// <param name="position">サンプル位置</param>
        /// <param name="elapsedTime">開始からの経過時間</param>
        private void AppendSampleIfNeeded(List<GesturePointerSample> samples, Vector2 position, float elapsedTime) {
            if (samples.Count > 0 && samples[samples.Count - 1].Position == position) {
                return;
            }

            samples.Add(new GesturePointerSample(position, elapsedTime));
        }

    }
}
