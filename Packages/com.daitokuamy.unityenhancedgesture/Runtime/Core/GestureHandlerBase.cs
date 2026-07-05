using System.Collections.Generic;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ジェスチャーハンドラーの共通基底クラス
    /// </summary>
    public abstract class GestureHandlerBase : MonoBehaviour, IGestureHandler {
        [SerializeField, Tooltip("候補が重複した際の優先度")]
        private int _priority = 0;
        [SerializeField, Tooltip("uGUI の RaycastTarget によって開始をブロックするかどうか")]
        private bool _isBlockedByUI = true;

        /// <summary>
        /// 候補が重複した際の優先度
        /// </summary>
        public int Priority {
            get => _priority;
            set => _priority = value;
        }

        /// <summary>
        /// uGUI の RaycastTarget によって開始をブロックするかどうか
        /// </summary>
        public bool IsBlockedByUI {
            get => _isBlockedByUI;
            set => _isBlockedByUI = value;
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

        /// <summary>
        /// 指定 Collider 群の最も近いヒット距離を取得
        /// </summary>
        /// <param name="targetColliders">判定対象 Collider 群</param>
        /// <param name="ray">判定に使用する Ray</param>
        /// <param name="distance">最も近いヒット距離</param>
        /// <returns>いずれかの Collider にヒットした場合は true</returns>
        protected static bool TryGetClosestColliderHitDistance(IReadOnlyList<Collider> targetColliders, Ray ray, out float distance) {
            distance = float.PositiveInfinity;

            if (targetColliders == null) {
                return false;
            }

            var hasHit = false;

            for (var i = 0; i < targetColliders.Count; i++) {
                var targetCollider = targetColliders[i];

                if (targetCollider == null || !targetCollider.Raycast(ray, out var hit, float.PositiveInfinity)) {
                    continue;
                }

                if (!hasHit || hit.distance < distance) {
                    distance = hit.distance;
                    hasHit = true;
                }
            }

            return hasHit;
        }

        /// <summary>
        /// 指定 uGUI RaycastTarget が自身の対象かどうかを判定
        /// </summary>
        /// <param name="raycastTarget">RaycastTarget の GameObject</param>
        /// <returns>自身の対象の場合は true</returns>
        internal virtual bool IsSelfUIRaycastTarget(GameObject raycastTarget) {
            return false;
        }

        /// <summary>
        /// RectTransform 判定に使用する Canvas カメラを取得
        /// </summary>
        /// <param name="targetRectTransform">対象 RectTransform</param>
        /// <returns>判定に使用する Canvas カメラ</returns>
        protected Camera ResolveCanvasCamera(RectTransform targetRectTransform) {
            if (targetRectTransform == null) {
                return null;
            }

            var canvas = targetRectTransform.GetComponentInParent<Canvas>();

            if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                return null;
            }

            return canvas.worldCamera;
        }
    }
}
