namespace UnityEnhancedGesture {
    /// <summary>
    /// 内部認識器の共通基底クラス
    /// </summary>
    internal abstract class GestureRecognizerBase {
        /// <summary>認識器が担当するジェスチャー種別</summary>
        public abstract GestureRecognitionType RecognitionType { get; }
    }
}
