using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// Editor 上のシミュレーション入力位置を GameView に可視化する補助クラス
    /// </summary>
    internal sealed class GestureSimulationGui {
#if UNITY_EDITOR
        private const float MarkerSize = 32.0f;

        private static readonly Color CenterColor = new(1.0f, 1.0f, 1.0f, 0.35f);
        private static readonly Color PrimaryColor = new(0.2f, 0.9f, 1.0f, 0.35f);
        private static readonly Color SecondaryColor = new(1.0f, 0.45f, 0.2f, 0.35f);

        private static Texture2D s_markerTexture;

        private bool _hasCenter;
        private bool _hasPointerPair;
        private Vector2 _center;
        private Vector2 _primaryPosition;
        private Vector2 _secondaryPosition;

        /// <summary>
        /// 描画状態を更新
        /// </summary>
        /// <param name="hasCenter">中央点を描画する場合は true</param>
        /// <param name="center">中央点座標</param>
        /// <param name="hasPointerPair">1点目と2点目を描画する場合は true</param>
        /// <param name="primaryPosition">1点目座標</param>
        /// <param name="secondaryPosition">2点目座標</param>
        public void SetState(
            bool hasCenter,
            Vector2 center,
            bool hasPointerPair,
            Vector2 primaryPosition,
            Vector2 secondaryPosition) {
            _hasCenter = hasCenter;
            _center = center;
            _hasPointerPair = hasPointerPair;
            _primaryPosition = primaryPosition;
            _secondaryPosition = secondaryPosition;
        }

        /// <summary>
        /// GameView へ可視化を描画
        /// </summary>
        public void DrawGui() {
            if (!_hasCenter) {
                return;
            }

            DrawMarker(_center, CenterColor);

            if (!_hasPointerPair) {
                return;
            }

            DrawMarker(_primaryPosition, PrimaryColor);
            DrawMarker(_secondaryPosition, SecondaryColor);
        }

        /// <summary>
        /// 指定座標へマーカーを描画
        /// </summary>
        /// <param name="screenPosition">描画するスクリーン座標</param>
        /// <param name="color">描画色</param>
        private void DrawMarker(Vector2 screenPosition, Color color) {
            var markerTexture = GetMarkerTexture();
            var guiPosition = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
            var markerRect = new Rect(
                guiPosition.x - (MarkerSize * 0.5f),
                guiPosition.y - (MarkerSize * 0.5f),
                MarkerSize,
                MarkerSize);
            var previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(markerRect, markerTexture);
            GUI.color = previousColor;
        }

        /// <summary>
        /// マーカー描画に使用するテクスチャを取得
        /// </summary>
        /// <returns>マーカー描画用テクスチャ</returns>
        private Texture2D GetMarkerTexture() {
            if (s_markerTexture != null) {
                return s_markerTexture;
            }

            var textureSize = 64;
            var texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false) {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
            };
            var center = (textureSize - 1) * 0.5f;
            var radius = textureSize * 0.5f;
            var colors = new Color[textureSize * textureSize];

            for (var y = 0; y < textureSize; y++) {
                for (var x = 0; x < textureSize; x++) {
                    var index = (y * textureSize) + x;
                    var distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    colors[index] = distance <= radius ? Color.white : Color.clear;
                }
            }

            texture.SetPixels(colors);
            texture.Apply();
            s_markerTexture = texture;
            return s_markerTexture;
        }
#else
        /// <summary>
        /// 描画状態を更新
        /// </summary>
        /// <param name="hasCenter">中央点を描画する場合は true</param>
        /// <param name="center">中央点座標</param>
        /// <param name="hasPointerPair">1点目と2点目を描画する場合は true</param>
        /// <param name="primaryPosition">1点目座標</param>
        /// <param name="secondaryPosition">2点目座標</param>
        public void SetState(
            bool hasCenter,
            Vector2 center,
            bool hasPointerPair,
            Vector2 primaryPosition,
            Vector2 secondaryPosition) {
            _ = hasCenter;
            _ = center;
            _ = hasPointerPair;
            _ = primaryPosition;
            _ = secondaryPosition;
        }

        /// <summary>
        /// GameView へ可視化を描画
        /// </summary>
        public void DrawGui() {
        }
#endif
    }
}
