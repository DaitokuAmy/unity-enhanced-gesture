using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ピンチ成立を判定する内部認識器
    /// </summary>
    internal sealed class PinchGestureRecognizer : GestureRecognizerBase {
        /// <inheritdoc/>
        public override GestureRecognitionType RecognitionType => GestureRecognitionType.Pinch;

        /// <summary>
        /// ピンチ開始を判定
        /// </summary>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="session">対象セッション</param>
        /// <param name="inputSnapshot">入力スナップショット</param>
        /// <returns>開始した場合は true</returns>
        public bool TryBegin(PinchGestureHandler handler, GestureSession session, GestureInputSnapshot inputSnapshot) {
            if (inputSnapshot.TouchCount != 2) {
                return false;
            }

            float distance = GetDistance(inputSnapshot);

            if (Mathf.Abs(distance - session.InitialPinchDistance) < handler.PinchStartThreshold) {
                return false;
            }

            float angle = GetAngle(inputSnapshot);
            session.RecognizedType = RecognitionType;
            session.UpdatePositions(inputSnapshot);
            session.LastPinchDistance = distance;
            session.LastPinchAngle = angle;
            handler.RaiseBeginPinch(CreateEvent(session, inputSnapshot, GestureEventPhase.Began, distance, distance, angle, angle));
            return true;
        }

        /// <summary>
        /// 成立済みピンチを更新
        /// </summary>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="session">対象セッション</param>
        /// <param name="inputSnapshot">入力スナップショット</param>
        /// <returns>終了またはキャンセルしてセッション破棄が必要な場合は true</returns>
        public bool Update(PinchGestureHandler handler, GestureSession session, GestureInputSnapshot inputSnapshot) {
            if (inputSnapshot.TouchCount != 2) {
                return true;
            }

            float previousDistance = session.LastPinchDistance;
            float previousAngle = session.LastPinchAngle;
            float distance = GetDistance(inputSnapshot);
            float angle = GetAngle(inputSnapshot);

            session.UpdatePositions(inputSnapshot);
            session.LastPinchDistance = distance;
            session.LastPinchAngle = angle;

            if (inputSnapshot.HasAnyCanceledTouch) {
                handler.RaiseCancelPinch(CreateEvent(session, inputSnapshot, GestureEventPhase.Canceled, previousDistance, distance, previousAngle, angle));
                return true;
            }

            if (inputSnapshot.HasAnyEndedTouch) {
                handler.RaiseEndPinch(CreateEvent(session, inputSnapshot, GestureEventPhase.Completed, previousDistance, distance, previousAngle, angle));
                return true;
            }

            if (inputSnapshot.HasAnyMovement) {
                handler.RaisePinch(CreateEvent(session, inputSnapshot, GestureEventPhase.Updated, previousDistance, distance, previousAngle, angle));
            }

            return false;
        }

        /// <summary>
        /// 現在の 2 点間距離を取得
        /// </summary>
        /// <param name="inputSnapshot">入力スナップショット</param>
        /// <returns>距離</returns>
        private static float GetDistance(GestureInputSnapshot inputSnapshot) {
            return Vector2.Distance(inputSnapshot.PrimaryPosition, inputSnapshot.SecondaryPosition);
        }

        /// <summary>
        /// 現在の 2 点間角度を取得
        /// </summary>
        /// <param name="inputSnapshot">入力スナップショット</param>
        /// <returns>角度</returns>
        private static float GetAngle(GestureInputSnapshot inputSnapshot) {
            Vector2 direction = inputSnapshot.SecondaryPosition - inputSnapshot.PrimaryPosition;
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// ピンチ通知用イベント引数を生成
        /// </summary>
        /// <param name="session">対象セッション</param>
        /// <param name="inputSnapshot">入力スナップショット</param>
        /// <param name="phase">通知フェーズ</param>
        /// <param name="previousDistance">前回距離</param>
        /// <param name="currentDistance">現在距離</param>
        /// <param name="previousAngle">前回角度</param>
        /// <param name="currentAngle">現在角度</param>
        /// <returns>イベント引数</returns>
        private static PinchGestureEvent CreateEvent(
            GestureSession session,
            GestureInputSnapshot inputSnapshot,
            GestureEventPhase phase,
            float previousDistance,
            float currentDistance,
            float previousAngle,
            float currentAngle) {
            float startDistance = Mathf.Approximately(session.InitialPinchDistance, 0f) ? currentDistance : session.InitialPinchDistance;
            return new PinchGestureEvent(
                phase,
                inputSnapshot.Center,
                startDistance,
                currentDistance,
                currentDistance - previousDistance,
                Mathf.Approximately(startDistance, 0f) ? 1f : currentDistance / startDistance,
                currentAngle,
                Mathf.DeltaAngle(previousAngle, currentAngle));
        }
    }
}
