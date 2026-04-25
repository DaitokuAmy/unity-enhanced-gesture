using System.Collections.Generic;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// 1 つの判定対象に紐づくハンドラー群
    /// </summary>
    internal sealed class GestureTargetEntry {
        private readonly List<GestureHandlerBase> _handlers = new();

        /// <summary>
        /// 判定対象エントリを初期化
        /// </summary>
        /// <param name="rectTransform">対象 RectTransform</param>
        public GestureTargetEntry(RectTransform rectTransform) {
            RectTransform = rectTransform;
        }

        /// <summary>対象 RectTransform</summary>
        public RectTransform RectTransform { get; }

        /// <summary>登録ハンドラーが空かどうか</summary>
        public bool IsEmpty => _handlers.Count == 0;

        /// <summary>
        /// ハンドラーを追加
        /// </summary>
        /// <param name="handler">追加対象ハンドラー</param>
        public void AddHandler(GestureHandlerBase handler) {
            if (!_handlers.Contains(handler)) {
                _handlers.Add(handler);
            }
        }

        /// <summary>
        /// ハンドラーを削除
        /// </summary>
        /// <param name="handler">削除対象ハンドラー</param>
        public void RemoveHandler(GestureHandlerBase handler) {
            _handlers.Remove(handler);
        }

        /// <summary>
        /// 指定型のハンドラーを取得
        /// </summary>
        /// <typeparam name="T">取得対象型</typeparam>
        /// <param name="handler">取得結果</param>
        /// <returns>取得に成功した場合は true</returns>
        public bool TryGetHandler<T>(out T handler)
            where T : GestureHandlerBase {
            foreach (GestureHandlerBase candidate in _handlers) {
                if (candidate is T typedHandler && typedHandler.isActiveAndEnabled) {
                    handler = typedHandler;
                    return true;
                }
            }

            handler = null;
            return false;
        }
    }
}
