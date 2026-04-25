using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ピンチ系イベント通知に使用する引数
    /// </summary>
    public readonly struct PinchGestureEvent {
        /// <summary>
        /// ピンチイベント引数を初期化
        /// </summary>
        /// <param name="phase">通知フェーズ</param>
        /// <param name="center">2 点の中心座標</param>
        /// <param name="startDistance">開始時距離</param>
        /// <param name="distance">現在距離</param>
        /// <param name="deltaDistance">前回通知からの距離差分</param>
        /// <param name="scale">開始時距離に対する拡大率</param>
        /// <param name="angle">2 点間ベクトルの現在角度</param>
        /// <param name="deltaAngle">前回通知からの角度差分</param>
        public PinchGestureEvent(
            GestureEventPhase phase,
            Vector2 center,
            float startDistance,
            float distance,
            float deltaDistance,
            float scale,
            float angle,
            float deltaAngle) {
            Phase = phase;
            Center = center;
            StartDistance = startDistance;
            Distance = distance;
            DeltaDistance = deltaDistance;
            Scale = scale;
            Angle = angle;
            DeltaAngle = deltaAngle;
        }

        /// <summary>通知フェーズ</summary>
        public GestureEventPhase Phase { get; }
        /// <summary>2 点の中心座標</summary>
        public Vector2 Center { get; }
        /// <summary>開始時距離</summary>
        public float StartDistance { get; }
        /// <summary>現在距離</summary>
        public float Distance { get; }
        /// <summary>前回通知からの距離差分</summary>
        public float DeltaDistance { get; }
        /// <summary>開始時距離に対する拡大率</summary>
        public float Scale { get; }
        /// <summary>2 点間ベクトルの現在角度</summary>
        public float Angle { get; }
        /// <summary>前回通知からの角度差分</summary>
        public float DeltaAngle { get; }
    }
}
