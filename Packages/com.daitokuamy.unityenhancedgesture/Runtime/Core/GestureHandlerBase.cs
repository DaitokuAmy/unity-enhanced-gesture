using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ジェスチャーハンドラーの共通基底クラス
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class GestureHandlerBase : MonoBehaviour {
        [SerializeField]
        private RectTransform _targetRectTransform;

        /// <summary>判定対象となる RectTransform</summary>
        public RectTransform TargetRectTransform {
            get {
                if (_targetRectTransform == null) {
                    CacheRectTransform();
                }

                return _targetRectTransform;
            }
        }

        /// <summary>
        /// エディタ追加時の参照を補完
        /// </summary>
        protected virtual void Reset() {
            CacheRectTransform();
        }

        /// <summary>
        /// 依存コンポーネントを初期化
        /// </summary>
        protected virtual void Awake() {
            CacheRectTransform();
        }

        /// <summary>
        /// 有効化時に中央管理へ登録
        /// </summary>
        protected virtual void OnEnable() {
            if (!Application.isPlaying) {
                return;
            }

            GestureCoordinator.Instance.RegisterHandler(this);
        }

        /// <summary>
        /// 無効化時に中央管理から解除
        /// </summary>
        protected virtual void OnDisable() {
            if (!Application.isPlaying || !GestureCoordinator.HasInstance) {
                return;
            }

            GestureCoordinator.Instance.UnregisterHandler(this);
        }

        /// <summary>
        /// インスペクタ更新時の参照を補完
        /// </summary>
        protected virtual void OnValidate() {
            CacheRectTransform();
        }

        /// <summary>
        /// RectTransform 参照を補完
        /// </summary>
        private void CacheRectTransform() {
            if (_targetRectTransform == null) {
                _targetRectTransform = GetComponent<RectTransform>();
            }
        }
    }
}
