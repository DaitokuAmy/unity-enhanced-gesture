using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityEnhancedGesture {
    /// <summary>
    /// 入力の収集とジェスチャー配送を統括する中央管理クラス
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class GestureCoordinator : MonoBehaviour {
        private static readonly List<IGestureRecognizer> Recognizers = new() {
            new DragGestureRecognizer(),
        };

        private static GestureCoordinator s_instance;

        [SerializeField, Tooltip("入力システムの有効化管理方式")]
        private GestureInputManagementMode _inputManagementMode = GestureInputManagementMode.Automatic;
        [SerializeField, Tooltip("入力更新の駆動方式")]
        private GestureCoordinatorUpdateMode _updateMode = GestureCoordinatorUpdateMode.Update;

        private readonly List<IGestureHandler> _handlers = new();
        private readonly Dictionary<int, GesturePointerInput> _inputsByPointerId = new();
        private readonly Dictionary<int, IGestureTrack> _tracksByPointerId = new();
        private readonly List<GesturePointerInput> _inputBuffer = new();
        private readonly List<IGestureTrack> _trackBuffer = new();

        private PointerMouseGestureInputProvider _mouseInputProvider;
        private EnhancedTouchGestureInputProvider _enhancedTouchInputProvider;
        private IGestureInputProvider _inputProvider;
        private bool _hasWarnedInputProviderState;
        private int _lastManualUpdateFrame = -1;

        /// <summary>
        /// 現在の管理インスタンス
        /// </summary>
        public static GestureCoordinator Instance => s_instance;

        /// <summary>
        /// インスタンスを初期化
        /// </summary>
        private void Awake() {
            if (s_instance != null && s_instance != this) {
                Debug.LogError("Multiple GestureCoordinator components are not supported.", this);
                enabled = false;
                return;
            }

            s_instance = this;
            _mouseInputProvider = new PointerMouseGestureInputProvider();
            _enhancedTouchInputProvider = new EnhancedTouchGestureInputProvider();
            _inputProvider = ResolveInputProvider();
        }

        /// <summary>
        /// 有効化時に入力管理を初期化
        /// </summary>
        private void OnEnable() {
            if (!Application.isPlaying || s_instance != this) {
                return;
            }

            _enhancedTouchInputProvider.Enable(_inputManagementMode);
            _mouseInputProvider.Enable(_inputManagementMode);
            _inputProvider = ResolveInputProvider();
            RefreshRegisteredHandlers();
        }

        /// <summary>
        /// 無効化時に入力管理を解放
        /// </summary>
        private void OnDisable() {
            if (!Application.isPlaying || s_instance != this || _inputProvider == null) {
                return;
            }

            _mouseInputProvider.Disable(_inputManagementMode);
            _enhancedTouchInputProvider.Disable(_inputManagementMode);
        }

        /// <summary>
        /// 破棄時にインスタンス参照を解放
        /// </summary>
        private void OnDestroy() {
            if (s_instance == this) {
                s_instance = null;
            }
        }

        /// <summary>
        /// 毎フレーム入力を更新
        /// </summary>
        private void Update() {
            if (_updateMode != GestureCoordinatorUpdateMode.Update) {
                return;
            }

            ProcessInput();
        }

        /// <summary>
        /// ハンドラーを登録
        /// </summary>
        /// <param name="handler">登録対象ハンドラー</param>
        public void RegisterHandler(IGestureHandler handler) {
            if (handler == null || _handlers.Contains(handler)) {
                return;
            }

            _handlers.Add(handler);
        }

        /// <summary>
        /// ハンドラー登録を解除
        /// </summary>
        /// <param name="handler">解除対象ハンドラー</param>
        public void UnregisterHandler(IGestureHandler handler) {
            _handlers.Remove(handler);
        }

        /// <summary>
        /// 手動更新を実行
        /// </summary>
        public void ManualUpdate() {
            if (_updateMode != GestureCoordinatorUpdateMode.ManualUpdate) {
                return;
            }

            if (_lastManualUpdateFrame == Time.frameCount) {
                throw new InvalidOperationException("GestureCoordinator.ManualUpdate must not be called more than once per frame.");
            }

            _lastManualUpdateFrame = Time.frameCount;
            ProcessInput();
        }

        /// <summary>
        /// 現在有効な入力解析実装を解決
        /// </summary>
        /// <returns>入力解析実装</returns>
        private IGestureInputProvider ResolveInputProvider() {
            if (_enhancedTouchInputProvider != null && Touchscreen.current != null && _enhancedTouchInputProvider.IsReady(_inputManagementMode)) {
                return _enhancedTouchInputProvider;
            }

            if (_mouseInputProvider != null && _mouseInputProvider.IsReady(_inputManagementMode)) {
                return _mouseInputProvider;
            }

            return _enhancedTouchInputProvider;
        }

        /// <summary>
        /// シーン上の有効ハンドラー登録を同期
        /// </summary>
        private void RefreshRegisteredHandlers() {
            _handlers.Clear();

            foreach (var handler in FindObjectsByType<GestureHandlerBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
                RegisterHandler(handler);
            }
        }

        /// <summary>
        /// 入力処理本体
        /// </summary>
        private void ProcessInput() {
            RefreshActiveInputProvider();

            if (!CanProcessInput()) {
                return;
            }

            CacheInputs();
            HandleInputBegins();
            CollectTracks();

            for (var i = 0; i < _trackBuffer.Count; i++) {
                var track = _trackBuffer[i];

                if (!_inputsByPointerId.TryGetValue(track.PointerId, out var input)) {
                    _tracksByPointerId.Remove(track.PointerId);
                    continue;
                }

                ProcessTrack(track, input);
            }
        }

        /// <summary>
        /// 入力処理可能かどうかを判定
        /// </summary>
        /// <returns>処理可能な場合は true</returns>
        private bool CanProcessInput() {
            if (_inputProvider == null) {
                return false;
            }

            if (_inputProvider.IsReady(_inputManagementMode)) {
                _hasWarnedInputProviderState = false;
                return true;
            }

            if (!_hasWarnedInputProviderState) {
                Debug.LogWarning(_inputProvider.NotReadyMessage, this);
                _hasWarnedInputProviderState = true;
            }

            return false;
        }

        /// <summary>
        /// 現在フレームの入力一覧をキャッシュ
        /// </summary>
        private void CacheInputs() {
            _inputBuffer.Clear();
            _inputsByPointerId.Clear();

            _inputProvider.CollectInputs(_inputBuffer);

            for (var i = 0; i < _inputBuffer.Count; i++) {
                var input = _inputBuffer[i];
                _inputsByPointerId[input.PointerId] = input;
            }
        }

        /// <summary>
        /// 新規入力開始から候補ハンドラーを選択
        /// </summary>
        private void HandleInputBegins() {
            foreach (var input in _inputsByPointerId.Values) {
                if (input.Phase != GestureInputPhase.Began || _tracksByPointerId.ContainsKey(input.PointerId)) {
                    continue;
                }

                if (!TrySelectHandler(input.Position, out var handler, out var recognizer)) {
                    continue;
                }

                var track = recognizer.CreateTrack(handler, input.PointerId, input.StartPosition, input.StartTime);
                _tracksByPointerId.Add(input.PointerId, track);
            }
        }

        /// <summary>
        /// 進行中トラック一覧を構築
        /// </summary>
        private void CollectTracks() {
            _trackBuffer.Clear();

            foreach (var track in _tracksByPointerId.Values) {
                _trackBuffer.Add(track);
            }
        }

        /// <summary>
        /// 優先度に基づいて配送先ハンドラーを選択
        /// </summary>
        /// <param name="screenPosition">開始位置</param>
        /// <param name="handler">選択されたハンドラー</param>
        /// <param name="recognizer">選択された認識器</param>
        /// <returns>選択できた場合は true</returns>
        private bool TrySelectHandler(Vector2 screenPosition, out IGestureHandler handler, out IGestureRecognizer recognizer) {
            handler = null;
            recognizer = null;
            var selectedPriority = int.MinValue;

            for (var i = 0; i < _handlers.Count; i++) {
                var currentHandler = _handlers[i];

                if (currentHandler == null || !currentHandler.IsActiveAndEnabled || !currentHandler.CanHandle(screenPosition)) {
                    continue;
                }

                if (!TryGetRecognizer(currentHandler, out var currentRecognizer)) {
                    continue;
                }

                if (handler == null || currentHandler.Priority > selectedPriority) {
                    handler = currentHandler;
                    recognizer = currentRecognizer;
                    selectedPriority = currentHandler.Priority;
                }
            }

            return handler != null;
        }

        /// <summary>
        /// トラックを更新
        /// </summary>
        /// <param name="track">対象トラック</param>
        /// <param name="input">現在入力</param>
        private void ProcessTrack(IGestureTrack track, GesturePointerInput input) {
            if (track == null || track.Recognizer == null) {
                _tracksByPointerId.Remove(track.PointerId);
                return;
            }

            track.Recognizer.ProcessTrack(track, input);

            if (track.IsCompleted) {
                _tracksByPointerId.Remove(track.PointerId);
            }
        }

        /// <summary>
        /// 指定ハンドラーに対応する認識器を取得
        /// </summary>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="recognizer">取得結果</param>
        /// <returns>取得できた場合は true</returns>
        private bool TryGetRecognizer(IGestureHandler handler, out IGestureRecognizer recognizer) {
            for (var i = 0; i < Recognizers.Count; i++) {
                var currentRecognizer = Recognizers[i];

                if (!currentRecognizer.CanCreateTrack(handler)) {
                    continue;
                }

                recognizer = currentRecognizer;
                return true;
            }

            recognizer = null;
            return false;
        }

        /// <summary>
        /// 現在のデバイス状態に応じて入力解析実装を更新
        /// </summary>
        private void RefreshActiveInputProvider() {
            var resolvedInputProvider = ResolveInputProvider();

            if (ReferenceEquals(_inputProvider, resolvedInputProvider)) {
                return;
            }

            _inputProvider = resolvedInputProvider;
        }
    }
}
