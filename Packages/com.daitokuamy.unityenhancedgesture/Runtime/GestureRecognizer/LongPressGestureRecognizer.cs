using UnityEngine;
using InputTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ロングプレス成立を判定する内部認識器
    /// </summary>
    internal sealed class LongPressGestureRecognizer : GestureRecognizerBase {
        /// <inheritdoc/>
        public override GestureRecognitionType RecognitionType => GestureRecognitionType.LongPress;

        /// <summary>
        /// ロングプレス開始を判定
        /// </summary>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="session">対象セッション</param>
        /// <param name="inputSnapshot">入力スナップショット</param>
        /// <returns>開始した場合は true</returns>
        public bool TryBegin(TapGestureHandler handler, GestureSession session, GestureInputSnapshot inputSnapshot) {
            if (!handler.EnableLongPress || inputSnapshot.TouchCount != 1) {
                return false;
            }

            if (inputSnapshot.PrimaryTravelDistance > handler.MaxTapMovement || inputSnapshot.PrimaryElapsedTime < handler.LongPressDuration) {
                return false;
            }

            session.RecognizedType = RecognitionType;
            session.UpdatePositions(inputSnapshot);
            handler.RaiseLongPress(CreateEvent(inputSnapshot.PrimaryTouch, GestureEventPhase.Began));
            return true;
        }

        /// <summary>
        /// 成立済みロングプレスを更新
        /// </summary>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="session">対象セッション</param>
        /// <param name="inputSnapshot">入力スナップショット</param>
        /// <returns>終了またはキャンセルしてセッション破棄が必要な場合は true</returns>
        public bool Update(TapGestureHandler handler, GestureSession session, GestureInputSnapshot inputSnapshot) {
            session.UpdatePositions(inputSnapshot);

            if (inputSnapshot.IsPrimaryCanceled) {
                handler.RaiseCancelTap(CreateEvent(inputSnapshot.PrimaryTouch, GestureEventPhase.Canceled));
                return true;
            }

            if (inputSnapshot.IsPrimaryEnded) {
                handler.RaiseLongPress(CreateEvent(inputSnapshot.PrimaryTouch, GestureEventPhase.Completed));
                return true;
            }

            return false;
        }

        /// <summary>
        /// ロングプレス通知用イベント引数を生成
        /// </summary>
        /// <param name="touch">対象タッチ</param>
        /// <param name="phase">通知フェーズ</param>
        /// <returns>イベント引数</returns>
        private static TapGestureEvent CreateEvent(InputTouch touch, GestureEventPhase phase) {
            return new TapGestureEvent(
                TapGestureKind.LongPress,
                phase,
                touch.startScreenPosition,
                touch.screenPosition,
                (float)(touch.time - touch.startTime),
                1);
        }
    }
}
