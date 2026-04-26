using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ジェスチャーハンドラーの共通基底クラス
    /// </summary>
    public abstract class GestureHandlerBase : MonoBehaviour, IGestureHandler {
        [SerializeField, Tooltip("候補が重複した際の優先度")]
        private int _priority = 0;

        /// <summary>
        /// 候補が重複した際の優先度
        /// </summary>
        public int Priority {
            get => _priority;
            set => _priority = value;
        }

        /// <inheritdoc/>
        public bool IsActiveAndEnabled => isActiveAndEnabled;

        /// <summary>
        /// 有効化時に中央管理へ登録
        /// </summary>
        protected virtual void OnEnable() {
            if (!Application.isPlaying || GestureCoordinator.Instance == null) {
                return;
            }

            GestureCoordinator.Instance.RegisterHandler(this);
        }

        /// <summary>
        /// 無効化時に中央管理から解除
        /// </summary>
        protected virtual void OnDisable() {
            if (!Application.isPlaying || GestureCoordinator.Instance == null) {
                return;
            }

            GestureCoordinator.Instance.UnregisterHandler(this);
        }

        /// <inheritdoc/>
        public abstract bool CanHandle(Vector2 screenPosition, Camera eventCamera);
    }
}
