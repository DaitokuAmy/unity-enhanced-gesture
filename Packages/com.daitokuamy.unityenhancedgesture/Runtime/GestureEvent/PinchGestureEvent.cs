using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ピンチイベント通知に使用する引数
    /// </summary>
    public readonly struct PinchGestureEvent {
        /// <summary>
        /// ピンチイベント引数を生成
        /// </summary>
        /// <param name="phase">イベントフェーズ</param>
        /// <param name="startCenter">開始中心位置</param>
        /// <param name="center">現在中心位置</param>
        /// <param name="centerDelta">前回からの中心差分</param>
        /// <param name="startDistance">開始距離</param>
        /// <param name="distance">現在距離</param>
        /// <param name="deltaDistance">前回からの距離差分</param>
        /// <param name="scale">開始距離に対する倍率</param>
        /// <param name="startAngle">開始角度</param>
        /// <param name="angle">現在角度</param>
        /// <param name="deltaAngle">前回からの角度差分</param>
        /// <param name="totalAngleDelta">開始角度からの角度差分</param>
        /// <param name="firstPosition">1本目の現在位置</param>
        /// <param name="secondPosition">2本目の現在位置</param>
        /// <param name="duration">開始からの経過時間</param>
        /// <param name="eventCamera">イベントに紐づくカメラ</param>
        public PinchGestureEvent(
            GestureEventPhase phase,
            Vector2 startCenter,
            Vector2 center,
            Vector2 centerDelta,
            float startDistance,
            float distance,
            float deltaDistance,
            float scale,
            float startAngle,
            float angle,
            float deltaAngle,
            float totalAngleDelta,
            Vector2 firstPosition,
            Vector2 secondPosition,
            float duration,
            Camera eventCamera) {
            Phase = phase;
            StartCenter = startCenter;
            Center = center;
            CenterDelta = centerDelta;
            StartDistance = startDistance;
            Distance = distance;
            DeltaDistance = deltaDistance;
            Scale = scale;
            StartAngle = startAngle;
            Angle = angle;
            DeltaAngle = deltaAngle;
            TotalAngleDelta = totalAngleDelta;
            FirstPosition = firstPosition;
            SecondPosition = secondPosition;
            Duration = duration;
            EventCamera = eventCamera;
        }

        /// <summary>イベントフェーズ</summary>
        public GestureEventPhase Phase { get; }
        /// <summary>開始中心位置</summary>
        public Vector2 StartCenter { get; }
        /// <summary>現在中心位置</summary>
        public Vector2 Center { get; }
        /// <summary>前回からの中心差分</summary>
        public Vector2 CenterDelta { get; }
        /// <summary>開始距離</summary>
        public float StartDistance { get; }
        /// <summary>現在距離</summary>
        public float Distance { get; }
        /// <summary>前回からの距離差分</summary>
        public float DeltaDistance { get; }
        /// <summary>開始距離に対する倍率</summary>
        public float Scale { get; }
        /// <summary>開始角度</summary>
        public float StartAngle { get; }
        /// <summary>現在角度</summary>
        public float Angle { get; }
        /// <summary>前回からの角度差分</summary>
        public float DeltaAngle { get; }
        /// <summary>開始角度からの角度差分</summary>
        public float TotalAngleDelta { get; }
        /// <summary>1本目の現在位置</summary>
        public Vector2 FirstPosition { get; }
        /// <summary>2本目の現在位置</summary>
        public Vector2 SecondPosition { get; }
        /// <summary>開始からの経過時間</summary>
        public float Duration { get; }
        /// <summary>イベントに紐づくカメラ</summary>
        public Camera EventCamera { get; }
    }
}
