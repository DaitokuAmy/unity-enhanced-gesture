using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityEnhancedGesture {
    /// <summary>
    /// 入力取得とジェスチャー配送を統括する中核クラス
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class GestureCoordinator : MonoBehaviour {
        private static readonly List<IGestureRecognizer> Recognizers = new() {
            new DragGestureRecognizer(),
            new TapGestureRecognizer(),
            new PinchGestureRecognizer(),
        };

        private static GestureCoordinator s_instance;

        [SerializeField, Tooltip("入力システムの管理方法")]
        private GestureInputManagementMode _inputManagementMode = GestureInputManagementMode.Automatic;
        [SerializeField, Tooltip("入力更新の実行方法")]
        private GestureCoordinatorUpdateMode _updateMode = GestureCoordinatorUpdateMode.Update;
        [SerializeField, Tooltip("3D 判定やレイ変換に使用する共有カメラ")]
        private Camera _eventCamera = null;

        private readonly List<IGestureHandler> _handlers = new();
        private readonly Dictionary<int, GesturePointerInput> _inputsByPointerId = new();
        private readonly List<GesturePointerInput> _inputBuffer = new();
        private readonly List<IGestureTrack> _tracks = new();
        private readonly List<IGestureTrack> _trackBuffer = new();
        private readonly List<IGestureRecognizer> _attachedRecognizerBuffer = new();
        private readonly GestureSimulationGui _simulationGui = new();

        private PointerMouseGestureInputProvider _mouseInputProvider;
        private EnhancedTouchGestureInputProvider _enhancedTouchInputProvider;
        private IGestureInputProvider _inputProvider;
        private bool _hasWarnedInputProviderState;
        private int _lastManualUpdateFrame = -1;

        /// <summary>
        /// 現在の中核インスタンス
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
        /// 有効化時に入力管理を開始
        /// </summary>
        private void OnEnable() {
            if (!Application.isPlaying || s_instance != this) {
                return;
            }

            ResetProcessingState();
            _enhancedTouchInputProvider.Enable(_inputManagementMode);
            _mouseInputProvider.Enable(_inputManagementMode);
            _inputProvider = ResolveInputProvider();
            RefreshRegisteredHandlers();
        }

        /// <summary>
        /// 無効化時に入力管理を停止
        /// </summary>
        private void OnDisable() {
            if (!Application.isPlaying || s_instance != this || _inputProvider == null) {
                return;
            }

            _mouseInputProvider.Disable(_inputManagementMode);
            _enhancedTouchInputProvider.Disable(_inputManagementMode);
            ResetProcessingState();
        }

        /// <summary>
        /// 破棄時にインスタンス参照を解除
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
                UpdateSimulationGui();
                return;
            }

            ProcessInput();
            UpdateSimulationGui();
        }

        /// <summary>
        /// GameView 描画を実行
        /// </summary>
        private void OnGUI() {
            _simulationGui.DrawGui();
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
            UpdateSimulationGui();
        }

        /// <summary>
        /// 現在使用する入力プロバイダーを解決
        /// </summary>
        /// <returns>使用する入力プロバイダー</returns>
        private IGestureInputProvider ResolveInputProvider() {
            if (_enhancedTouchInputProvider != null
                && Touchscreen.current != null
                && _enhancedTouchInputProvider.IsReady(_inputManagementMode)) {
                return _enhancedTouchInputProvider;
            }

            if (_mouseInputProvider != null && _mouseInputProvider.IsReady(_inputManagementMode)) {
                return _mouseInputProvider;
            }

            return _enhancedTouchInputProvider;
        }

        /// <summary>
        /// シーン上の有効ハンドラーを再収集
        /// </summary>
        private void RefreshRegisteredHandlers() {
            _handlers.Clear();

            foreach (var handler in FindObjectsByType<GestureHandlerBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
                RegisterHandler(handler);
            }
        }

        /// <summary>
        /// 入力処理全体を実行
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
                ProcessTrack(_trackBuffer[i]);
            }

            CleanupCompletedTracks();
        }

        /// <summary>
        /// シミュレーション可視化状態を更新
        /// </summary>
        private void UpdateSimulationGui() {
#if UNITY_EDITOR
            if (_inputProvider is PointerMouseGestureInputProvider mouseInputProvider
                && mouseInputProvider.TryGetSimulationGuiData(
                    out var hasMouseCenter,
                    out var mouseCenter,
                    out var hasMousePointerPair,
                    out var mousePrimary,
                    out var mouseSecondary)) {
                _simulationGui.SetState(hasMouseCenter, mouseCenter, hasMousePointerPair, mousePrimary, mouseSecondary);
                return;
            }

            if (_inputProvider is EnhancedTouchGestureInputProvider enhancedTouchInputProvider
                && enhancedTouchInputProvider.TryGetSimulationGuiData(
                    out var hasTouchCenter,
                    out var touchCenter,
                    out var hasTouchPointerPair,
                    out var touchPrimary,
                    out var touchSecondary)) {
                _simulationGui.SetState(hasTouchCenter, touchCenter, hasTouchPointerPair, touchPrimary, touchSecondary);
                return;
            }
#endif

            _simulationGui.SetState(false, default, false, default, default);
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
        /// 現在フレームの入力一覧を構築
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
        /// 新規開始入力からトラックを生成または既存トラックへ追加
        /// </summary>
        private void HandleInputBegins() {
            foreach (var input in _inputsByPointerId.Values) {
                if (input.Phase != GestureInputPhase.Began) {
                    continue;
                }

                TryAddPointerToTracks(input, _attachedRecognizerBuffer);
                CreateTracksForInput(input, _attachedRecognizerBuffer);
            }
        }

        /// <summary>
        /// 現在処理対象のトラック一覧を構築
        /// </summary>
        private void CollectTracks() {
            _trackBuffer.Clear();

            for (var i = 0; i < _tracks.Count; i++) {
                _trackBuffer.Add(_tracks[i]);
            }
        }

        /// <summary>
        /// 入力に対して recognizer 系統ごとのトラックを生成
        /// </summary>
        /// <param name="input">開始入力</param>
        /// <param name="attachedRecognizers">追加済み recognizer 一覧</param>
        private void CreateTracksForInput(GesturePointerInput input, List<IGestureRecognizer> attachedRecognizers) {
            for (var i = 0; i < Recognizers.Count; i++) {
                var recognizer = Recognizers[i];

                if (attachedRecognizers.Contains(recognizer) || HasTrackForPointer(recognizer, input.PointerId)) {
                    continue;
                }

                if (!TrySelectHandler(input.Position, recognizer, out var handler)) {
                    continue;
                }

                _tracks.Add(recognizer.CreateTrack(handler, input, _eventCamera));
            }
        }

        /// <summary>
        /// 指定 recognizer 系統で配送先ハンドラーを選択
        /// </summary>
        /// <param name="screenPosition">開始位置</param>
        /// <param name="recognizer">対象 recognizer</param>
        /// <param name="handler">選択結果ハンドラー</param>
        /// <returns>選択できた場合は true</returns>
        private bool TrySelectHandler(Vector2 screenPosition, IGestureRecognizer recognizer, out IGestureHandler handler) {
            handler = null;
            var selectedPriority = int.MinValue;

            for (var i = 0; i < _handlers.Count; i++) {
                var currentHandler = _handlers[i];

                if (currentHandler == null
                    || !currentHandler.IsActiveAndEnabled
                    || !currentHandler.CanHandle(screenPosition, _eventCamera)
                    || !recognizer.CanCreateTrack(currentHandler)) {
                    continue;
                }

                if (handler == null || currentHandler.Priority > selectedPriority) {
                    handler = currentHandler;
                    selectedPriority = currentHandler.Priority;
                }
            }

            return handler != null;
        }

        /// <summary>
        /// トラックを更新
        /// </summary>
        /// <param name="track">対象トラック</param>
        private void ProcessTrack(IGestureTrack track) {
            if (track == null || track.Recognizer == null || track.IsCompleted) {
                return;
            }

            track.Recognizer.ProcessTrack(track, _inputsByPointerId, Time.unscaledTime);
        }

        /// <summary>
        /// 既存トラックへ新規ポインターを追加
        /// </summary>
        /// <param name="input">開始入力</param>
        /// <param name="attachedRecognizers">追加できた recognizer 一覧</param>
        private void TryAddPointerToTracks(GesturePointerInput input, List<IGestureRecognizer> attachedRecognizers) {
            attachedRecognizers.Clear();

            for (var i = 0; i < _tracks.Count; i++) {
                var track = _tracks[i];

                if (track == null || track.IsCompleted || track.Recognizer == null || HasPointer(track, input.PointerId)) {
                    continue;
                }

                if (!track.Recognizer.TryAddPointer(track, input)) {
                    continue;
                }

                attachedRecognizers.Add(track.Recognizer);
            }
        }

        /// <summary>
        /// 指定 recognizer 系統で既にポインターを所有するトラックがあるか判定
        /// </summary>
        /// <param name="recognizer">対象 recognizer</param>
        /// <param name="pointerId">ポインター ID</param>
        /// <returns>既に存在する場合は true</returns>
        private bool HasTrackForPointer(IGestureRecognizer recognizer, int pointerId) {
            for (var i = 0; i < _tracks.Count; i++) {
                var track = _tracks[i];

                if (track == null || track.IsCompleted || !ReferenceEquals(track.Recognizer, recognizer)) {
                    continue;
                }

                if (HasPointer(track, pointerId)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// トラックが指定ポインターを所有しているか判定
        /// </summary>
        /// <param name="track">対象トラック</param>
        /// <param name="pointerId">ポインター ID</param>
        /// <returns>所有している場合は true</returns>
        private bool HasPointer(IGestureTrack track, int pointerId) {
            for (var i = 0; i < track.PointerIds.Count; i++) {
                if (track.PointerIds[i] == pointerId) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 完了済みトラックを除去
        /// </summary>
        private void CleanupCompletedTracks() {
            for (var i = _tracks.Count - 1; i >= 0; i--) {
                var track = _tracks[i];

                if (track == null || track.IsCompleted) {
                    _tracks.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 現在のデバイス状態に応じて入力プロバイダーを更新
        /// </summary>
        private void RefreshActiveInputProvider() {
            var resolvedInputProvider = ResolveInputProvider();

            if (ReferenceEquals(_inputProvider, resolvedInputProvider)) {
                return;
            }

            ResetProcessingState();
            _inputProvider = resolvedInputProvider;
        }

        /// <summary>
        /// 入力処理中の内部状態を初期化
        /// </summary>
        private void ResetProcessingState() {
            _inputsByPointerId.Clear();
            _inputBuffer.Clear();
            _tracks.Clear();
            _trackBuffer.Clear();
            _attachedRecognizerBuffer.Clear();
            _hasWarnedInputProviderState = false;
            _lastManualUpdateFrame = -1;
        }
    }
}
