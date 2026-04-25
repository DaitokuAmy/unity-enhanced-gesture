using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using InputTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using InputTouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ジェスチャー検出と排他制御を統括する中央管理クラス
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class GestureCoordinator : MonoBehaviour {
        private static GestureCoordinator s_instance;

        private readonly Dictionary<RectTransform, GestureTargetEntry> _entriesByRectTransform = new();
        private readonly Dictionary<int, GestureSession> _sessionsByTouchId = new();
        private readonly Dictionary<int, InputTouch> _touchesById = new();
        private readonly List<GestureSession> _sessionBuffer = new();
        private readonly DragGestureRecognizer _dragGestureRecognizer = new();
        private readonly PinchGestureRecognizer _pinchGestureRecognizer = new();
        private readonly TapGestureRecognizer _tapGestureRecognizer = new();
        private readonly LongPressGestureRecognizer _longPressGestureRecognizer = new();

        /// <summary>現在の中央管理インスタンス</summary>
        public static GestureCoordinator Instance {
            get {
                if (s_instance == null) {
                    CreateInstance();
                }

                return s_instance;
            }
        }

        /// <summary>インスタンスが生成済みかどうか</summary>
        internal static bool HasInstance => s_instance != null;

        /// <summary>
        /// シーン読み込み前に管理インスタンスを準備
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() {
            if (s_instance == null) {
                CreateInstance();
            }
        }

        /// <summary>
        /// ハンドラーを中央管理へ登録
        /// </summary>
        /// <param name="handler">登録対象ハンドラー</param>
        public void RegisterHandler(GestureHandlerBase handler) {
            if (handler == null) {
                return;
            }

            RectTransform rectTransform = handler.TargetRectTransform;

            if (!_entriesByRectTransform.TryGetValue(rectTransform, out GestureTargetEntry targetEntry)) {
                targetEntry = new GestureTargetEntry(rectTransform);
                _entriesByRectTransform.Add(rectTransform, targetEntry);
            }

            targetEntry.AddHandler(handler);
        }

        /// <summary>
        /// ハンドラーを中央管理から解除
        /// </summary>
        /// <param name="handler">解除対象ハンドラー</param>
        public void UnregisterHandler(GestureHandlerBase handler) {
            if (handler == null) {
                return;
            }

            RectTransform rectTransform = handler.TargetRectTransform;

            if (!_entriesByRectTransform.TryGetValue(rectTransform, out GestureTargetEntry targetEntry)) {
                return;
            }

            targetEntry.RemoveHandler(handler);

            if (targetEntry.IsEmpty) {
                _entriesByRectTransform.Remove(rectTransform);
            }
        }

        /// <summary>
        /// 管理インスタンスを生成
        /// </summary>
        private static void CreateInstance() {
            GameObject coordinatorObject = new("[Unity Enhanced Gesture Coordinator]") {
                hideFlags = HideFlags.HideInHierarchy,
            };

            DontDestroyOnLoad(coordinatorObject);
            s_instance = coordinatorObject.AddComponent<GestureCoordinator>();
        }

        /// <summary>
        /// インスタンスを初期化
        /// </summary>
        private void Awake() {
            if (s_instance != null && s_instance != this) {
                Destroy(gameObject);
                return;
            }

            s_instance = this;
            EnhancedTouchSupport.Enable();
        }

        /// <summary>
        /// インスタンス破棄時に入力監視を終了
        /// </summary>
        private void OnDestroy() {
            if (s_instance != this) {
                return;
            }

            EnhancedTouchSupport.Disable();
            s_instance = null;
        }

        /// <summary>
        /// 毎フレームの入力更新を処理
        /// </summary>
        private void Update() {
            _tapGestureRecognizer.FlushPending(Time.realtimeSinceStartup);
            CacheTouches();
            HandleTouchBegins();
            CollectSessions();

            for (int i = 0; i < _sessionBuffer.Count; i++) {
                GestureSession session = _sessionBuffer[i];

                if (!TryCreateSnapshot(session, out GestureInputSnapshot inputSnapshot)) {
                    RemoveSession(session);
                    continue;
                }

                ProcessSession(session, inputSnapshot);
            }
        }

        /// <summary>
        /// 現在のタッチ一覧をキャッシュ
        /// </summary>
        private void CacheTouches() {
            _touchesById.Clear();

            foreach (InputTouch touch in InputTouch.activeTouches) {
                _touchesById[touch.touchId] = touch;
            }
        }

        /// <summary>
        /// 開始したタッチからセッションを生成または拡張
        /// </summary>
        private void HandleTouchBegins() {
            foreach (InputTouch touch in _touchesById.Values) {
                if (touch.phase != InputTouchPhase.Began) {
                    continue;
                }

                if (_sessionsByTouchId.ContainsKey(touch.touchId)) {
                    continue;
                }

                if (!GestureRaycastResolver.TryResolveTarget(_entriesByRectTransform, touch.screenPosition, out GestureTargetEntry targetEntry)) {
                    continue;
                }

                if (TryMergeTouchIntoPinchSession(targetEntry, touch)) {
                    continue;
                }

                GestureSession session = new(targetEntry, touch.touchId, touch.startScreenPosition, (float)touch.startTime);
                _sessionsByTouchId.Add(touch.touchId, session);
            }
        }

        /// <summary>
        /// 既存セッションへ副タッチを結合
        /// </summary>
        /// <param name="targetEntry">対象エントリ</param>
        /// <param name="touch">追加するタッチ</param>
        /// <returns>結合に成功した場合は true</returns>
        private bool TryMergeTouchIntoPinchSession(GestureTargetEntry targetEntry, InputTouch touch) {
            if (!targetEntry.TryGetHandler<PinchGestureHandler>(out _)) {
                return false;
            }

            GestureSession candidateSession = null;

            foreach (GestureSession session in _sessionsByTouchId.Values) {
                if (session.TargetEntry != targetEntry || !session.CanMergeSecondTouch) {
                    continue;
                }

                if (!_touchesById.TryGetValue(session.PrimaryTouchId, out InputTouch primaryTouch)) {
                    continue;
                }

                session.AttachSecondaryTouch(touch.touchId, primaryTouch.screenPosition, touch.screenPosition);
                candidateSession = session;
                break;
            }

            if (candidateSession == null) {
                return false;
            }

            _sessionsByTouchId.Add(touch.touchId, candidateSession);
            return true;
        }

        /// <summary>
        /// 重複なしのセッション一覧を構築
        /// </summary>
        private void CollectSessions() {
            _sessionBuffer.Clear();

            foreach (GestureSession session in _sessionsByTouchId.Values) {
                if (!_sessionBuffer.Contains(session)) {
                    _sessionBuffer.Add(session);
                }
            }
        }

        /// <summary>
        /// セッション用の入力スナップショットを生成
        /// </summary>
        /// <param name="session">対象セッション</param>
        /// <param name="inputSnapshot">生成結果</param>
        /// <returns>生成に成功した場合は true</returns>
        private bool TryCreateSnapshot(GestureSession session, out GestureInputSnapshot inputSnapshot) {
            inputSnapshot = default;

            if (!_touchesById.TryGetValue(session.PrimaryTouchId, out InputTouch primaryTouch)) {
                return false;
            }

            if (!session.HasSecondaryTouch) {
                inputSnapshot = new GestureInputSnapshot(primaryTouch, null);
                return true;
            }

            if (!session.SecondaryTouchId.HasValue || !_touchesById.TryGetValue(session.SecondaryTouchId.Value, out InputTouch secondaryTouch)) {
                return false;
            }

            inputSnapshot = new GestureInputSnapshot(primaryTouch, secondaryTouch);
            return true;
        }

        /// <summary>
        /// セッション状態に応じて認識器を評価
        /// </summary>
        /// <param name="session">対象セッション</param>
        /// <param name="inputSnapshot">入力スナップショット</param>
        private void ProcessSession(GestureSession session, GestureInputSnapshot inputSnapshot) {
            if (session.RecognizedType == GestureRecognitionType.Drag) {
                ProcessRecognizedDrag(session, inputSnapshot);
                return;
            }

            if (session.RecognizedType == GestureRecognitionType.Pinch) {
                ProcessRecognizedPinch(session, inputSnapshot);
                return;
            }

            if (session.RecognizedType == GestureRecognitionType.LongPress) {
                ProcessRecognizedLongPress(session, inputSnapshot);
                return;
            }

            if (inputSnapshot.TouchCount == 2
                && session.TargetEntry.TryGetHandler<PinchGestureHandler>(out PinchGestureHandler pinchHandler)
                && _pinchGestureRecognizer.TryBegin(pinchHandler, session, inputSnapshot)) {
                return;
            }

            if (inputSnapshot.TouchCount == 1
                && session.TargetEntry.TryGetHandler<DragGestureHandler>(out DragGestureHandler dragHandler)
                && _dragGestureRecognizer.TryBegin(dragHandler, session, inputSnapshot)) {
                return;
            }

            if (inputSnapshot.TouchCount == 1
                && session.TargetEntry.TryGetHandler<TapGestureHandler>(out TapGestureHandler tapHandler)
                && _longPressGestureRecognizer.TryBegin(tapHandler, session, inputSnapshot)) {
                return;
            }

            if (inputSnapshot.HasAnyCanceledTouch) {
                if (inputSnapshot.TouchCount == 1 && session.TargetEntry.TryGetHandler<TapGestureHandler>(out TapGestureHandler canceledTapHandler)) {
                    _tapGestureRecognizer.RaiseCanceled(canceledTapHandler, inputSnapshot);
                }

                RemoveSession(session);
                return;
            }

            if (inputSnapshot.TouchCount == 1 && inputSnapshot.IsPrimaryEnded) {
                if (session.TargetEntry.TryGetHandler<TapGestureHandler>(out TapGestureHandler completedTapHandler)
                    && !_tapGestureRecognizer.TryCompleteTap(completedTapHandler, inputSnapshot)) {
                    _tapGestureRecognizer.RaiseCanceled(completedTapHandler, inputSnapshot);
                }

                RemoveSession(session);
                return;
            }

            if (inputSnapshot.TouchCount == 2 && inputSnapshot.HasAnyEndedTouch) {
                RemoveSession(session);
            }
        }

        /// <summary>
        /// 成立済みドラッグを更新
        /// </summary>
        /// <param name="session">対象セッション</param>
        /// <param name="inputSnapshot">入力スナップショット</param>
        private void ProcessRecognizedDrag(GestureSession session, GestureInputSnapshot inputSnapshot) {
            if (!session.TargetEntry.TryGetHandler<DragGestureHandler>(out DragGestureHandler dragHandler)
                || _dragGestureRecognizer.Update(dragHandler, session, inputSnapshot)) {
                RemoveSession(session);
            }
        }

        /// <summary>
        /// 成立済みピンチを更新
        /// </summary>
        /// <param name="session">対象セッション</param>
        /// <param name="inputSnapshot">入力スナップショット</param>
        private void ProcessRecognizedPinch(GestureSession session, GestureInputSnapshot inputSnapshot) {
            if (!session.TargetEntry.TryGetHandler<PinchGestureHandler>(out PinchGestureHandler pinchHandler)
                || _pinchGestureRecognizer.Update(pinchHandler, session, inputSnapshot)) {
                RemoveSession(session);
            }
        }

        /// <summary>
        /// 成立済みロングプレスを更新
        /// </summary>
        /// <param name="session">対象セッション</param>
        /// <param name="inputSnapshot">入力スナップショット</param>
        private void ProcessRecognizedLongPress(GestureSession session, GestureInputSnapshot inputSnapshot) {
            if (!session.TargetEntry.TryGetHandler<TapGestureHandler>(out TapGestureHandler tapHandler)
                || _longPressGestureRecognizer.Update(tapHandler, session, inputSnapshot)) {
                RemoveSession(session);
            }
        }

        /// <summary>
        /// セッションを破棄
        /// </summary>
        /// <param name="session">破棄対象セッション</param>
        private void RemoveSession(GestureSession session) {
            foreach (int touchId in session.EnumerateTouchIds()) {
                _sessionsByTouchId.Remove(touchId);
            }
        }
    }
}
