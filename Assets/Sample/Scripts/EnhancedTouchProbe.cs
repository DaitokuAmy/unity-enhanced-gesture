using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using InputTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using InputTouchPhase = UnityEngine.InputSystem.TouchPhase;

/// <summary>
/// Editor 上の Mouse と TouchSimulation / EnhancedTouch の関係を検証するためのスクリプト
/// </summary>
public sealed class EnhancedTouchProbe : MonoBehaviour {
    [SerializeField, Tooltip("ログ出力を有効化するか")]
    private bool _enableLogging = true;
    [SerializeField, Tooltip("起動時に EnhancedTouchSupport を有効化するか")]
    private bool _enableEnhancedTouchSupport = true;
    [SerializeField, Tooltip("起動時に TouchSimulation.Enable をコードから呼ぶか")]
    private bool _enableTouchSimulationFromCode = false;

    private string _lastSummary = string.Empty;
    private string _lastPrimaryTouchState = string.Empty;
    private string _lastFirstTouchState = string.Empty;

    /// <summary>
    /// 有効化時に入力検証の準備を行う
    /// </summary>
    private void OnEnable() {
        if (_enableEnhancedTouchSupport) {
            EnhancedTouchSupport.Enable();
        }

        if (_enableTouchSimulationFromCode) {
            TouchSimulation.Enable();
        }

        Log("OnEnable");
        LogSummary(force: true);
    }

    /// <summary>
    /// 無効化時に入力検証を終了する
    /// </summary>
    private void OnDisable() {
        if (_enableTouchSimulationFromCode && TouchSimulation.instance != null) {
            TouchSimulation.Disable();
        }

        if (_enableEnhancedTouchSupport && EnhancedTouchSupport.enabled) {
            EnhancedTouchSupport.Disable();
        }

        Log("OnDisable");
    }

    /// <summary>
    /// 毎フレーム状態変化と入力イベントを観測する
    /// </summary>
    private void Update() {
        if (!_enableLogging) {
            return;
        }

        LogSummary(force: false);
        LogMouseEvents();
        LogEnhancedTouchEvents();
        LogTouchscreenState();
    }

    /// <summary>
    /// 現在の入力システム状態を要約して出力する
    /// </summary>
    /// <param name="force">強制出力する場合は true</param>
    private void LogSummary(bool force) {
        var summaryBuilder = new StringBuilder();
        summaryBuilder.Append("Summary ");
        summaryBuilder.Append($"EnhancedTouchEnabled:{EnhancedTouchSupport.enabled} ");
        summaryBuilder.Append($"TouchSimulationInstance:{(TouchSimulation.instance != null)} ");
        summaryBuilder.Append($"MouseCurrent:{(Mouse.current != null)} ");
        summaryBuilder.Append($"TouchscreenCurrent:{(Touchscreen.current != null)} ");
        summaryBuilder.Append($"ActiveTouches:{InputTouch.activeTouches.Count}");

        var summary = summaryBuilder.ToString();

        if (!force && summary == _lastSummary) {
            return;
        }

        _lastSummary = summary;
        Log(summary);
    }

    /// <summary>
    /// Mouse デバイスから取得できるイベントを出力する
    /// </summary>
    private void LogMouseEvents() {
        if (Mouse.current == null) {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame) {
            Log($"Mouse Pressed Position:{Mouse.current.position.ReadValue()} Delta:{Mouse.current.delta.ReadValue()}");
        }

        if (Mouse.current.leftButton.isPressed && Mouse.current.delta.ReadValue() != Vector2.zero) {
            Log($"Mouse Moved Position:{Mouse.current.position.ReadValue()} Delta:{Mouse.current.delta.ReadValue()}");
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame) {
            Log($"Mouse Released Position:{Mouse.current.position.ReadValue()} Delta:{Mouse.current.delta.ReadValue()}");
        }
    }

    /// <summary>
    /// EnhancedTouch のアクティブタッチ情報を出力する
    /// </summary>
    private void LogEnhancedTouchEvents() {
        for (var i = 0; i < InputTouch.activeTouches.Count; i++) {
            var touch = InputTouch.activeTouches[i];

            if (touch.phase == InputTouchPhase.Stationary) {
                continue;
            }

            Log(
                $"EnhancedTouch Id:{touch.touchId} Phase:{touch.phase} Position:{touch.screenPosition} Delta:{touch.delta} Start:{touch.startScreenPosition}");
        }
    }

    /// <summary>
    /// Touchscreen の primaryTouch と touches[0] 状態を出力する
    /// </summary>
    private void LogTouchscreenState() {
        if (Touchscreen.current == null) {
            _lastPrimaryTouchState = string.Empty;
            _lastFirstTouchState = string.Empty;
            return;
        }

        var primaryTouch = Touchscreen.current.primaryTouch;
        var primaryTouchState =
            $"Touchscreen PrimaryTouch Press:{primaryTouch.press.isPressed} Phase:{primaryTouch.phase.ReadValue()} Position:{primaryTouch.position.ReadValue()} Delta:{primaryTouch.delta.ReadValue()}";

        if (primaryTouchState != _lastPrimaryTouchState) {
            _lastPrimaryTouchState = primaryTouchState;
            Log(primaryTouchState);
        }

        if (Touchscreen.current.touches.Count <= 0) {
            _lastFirstTouchState = string.Empty;
            return;
        }

        var firstTouch = Touchscreen.current.touches[0];
        var firstTouchState =
            $"Touchscreen Touch0 Press:{firstTouch.press.isPressed} Phase:{firstTouch.phase.ReadValue()} Position:{firstTouch.position.ReadValue()} Delta:{firstTouch.delta.ReadValue()}";

        if (firstTouchState == _lastFirstTouchState) {
            return;
        }

        _lastFirstTouchState = firstTouchState;
        Log(firstTouchState);
    }

    /// <summary>
    /// 検証ログを出力する
    /// </summary>
    /// <param name="message">出力メッセージ</param>
    private void Log(string message) {
        if (!_enableLogging) {
            return;
        }

        Debug.Log($"[EnhancedTouchProbe] {message}", this);
    }
}
