using UnityEngine;
using InputTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using InputTouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace UnityEnhancedGesture {
    /// <summary>
    /// ドラッグ成立を判定する内部認識器
    /// </summary>
    internal sealed class DragGestureRecognizer : GestureRecognizerBase {
        /// <inheritdoc/>
        public override GestureRecognitionType RecognitionType => GestureRecognitionType.Drag;

        /// <summary>
        /// ドラッグ開始を判定
        /// </summary>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="session">対象セッション</param>
        /// <param name="inputSnapshot">入力スナップショット</param>
        /// <returns>開始した場合は true</returns>
        public bool TryBegin(DragGestureHandler handler, GestureSession session, GestureInputSnapshot inputSnapshot) {
            if (inputSnapshot.TouchCount != 1 || inputSnapshot.PrimaryTravelDistance < handler.DragStartThreshold) {
                return false;
            }

            session.RecognizedType = RecognitionType;
            session.UpdatePositions(inputSnapshot);
            handler.RaiseBeginDrag(CreateEvent(inputSnapshot.PrimaryTouch, GestureEventPhase.Began));

            if (inputSnapshot.PrimaryTouch.phase == InputTouchPhase.Moved && inputSnapshot.PrimaryTouch.delta != Vector2.zero) {
                handler.RaiseDrag(CreateEvent(inputSnapshot.PrimaryTouch, GestureEventPhase.Updated));
            }

            return true;
        }

        /// <summary>
        /// 成立済みドラッグを更新
        /// </summary>
        /// <param name="handler">対象ハンドラー</param>
        /// <param name="session">対象セッション</param>
        /// <param name="inputSnapshot">入力スナップショット</param>
        /// <returns>終了またはキャンセルしてセッション破棄が必要な場合は true</returns>
        public bool Update(DragGestureHandler handler, GestureSession session, GestureInputSnapshot inputSnapshot) {
            session.UpdatePositions(inputSnapshot);

            if (inputSnapshot.IsPrimaryCanceled) {
                handler.RaiseCancelDrag(CreateEvent(inputSnapshot.PrimaryTouch, GestureEventPhase.Canceled));
                return true;
            }

            if (inputSnapshot.IsPrimaryEnded) {
                handler.RaiseEndDrag(CreateEvent(inputSnapshot.PrimaryTouch, GestureEventPhase.Completed));
                return true;
            }

            if (inputSnapshot.PrimaryTouch.phase == InputTouchPhase.Moved && inputSnapshot.PrimaryTouch.delta != Vector2.zero) {
                handler.RaiseDrag(CreateEvent(inputSnapshot.PrimaryTouch, GestureEventPhase.Updated));
            }

            return false;
        }

        /// <summary>
        /// ドラッグ通知用イベント引数を生成
        /// </summary>
        /// <param name="touch">対象タッチ</param>
        /// <param name="phase">通知フェーズ</param>
        /// <returns>イベント引数</returns>
        private static DragGestureEvent CreateEvent(InputTouch touch, GestureEventPhase phase) {
            return new DragGestureEvent(
                phase,
                touch.startScreenPosition,
                touch.screenPosition,
                touch.delta,
                touch.screenPosition - touch.startScreenPosition,
                (float)(touch.time - touch.startTime));
        }
    }
}
