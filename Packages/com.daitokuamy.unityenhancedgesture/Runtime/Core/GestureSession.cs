using System.Collections.Generic;
using UnityEngine;

namespace UnityEnhancedGesture {
    /// <summary>
    /// 1 つの入力系列を保持する内部セッション
    /// </summary>
    internal sealed class GestureSession {
        /// <summary>
        /// セッションを初期化
        /// </summary>
        /// <param name="targetEntry">所有対象</param>
        /// <param name="primaryTouchId">主タッチ ID</param>
        /// <param name="startPosition">開始位置</param>
        /// <param name="startTime">開始時刻</param>
        public GestureSession(GestureTargetEntry targetEntry, int primaryTouchId, Vector2 startPosition, float startTime) {
            TargetEntry = targetEntry;
            PrimaryTouchId = primaryTouchId;
            StartPosition = startPosition;
            StartTime = startTime;
            LastPrimaryPosition = startPosition;
        }

        /// <summary>所有対象</summary>
        public GestureTargetEntry TargetEntry { get; }
        /// <summary>主タッチ ID</summary>
        public int PrimaryTouchId { get; }
        /// <summary>副タッチ ID</summary>
        public int? SecondaryTouchId { get; private set; }
        /// <summary>成立済みジェスチャー種別</summary>
        public GestureRecognitionType RecognizedType { get; set; }
        /// <summary>開始位置</summary>
        public Vector2 StartPosition { get; }
        /// <summary>開始時刻</summary>
        public float StartTime { get; }
        /// <summary>主タッチの前回位置</summary>
        public Vector2 LastPrimaryPosition { get; private set; }
        /// <summary>副タッチの前回位置</summary>
        public Vector2? LastSecondaryPosition { get; private set; }
        /// <summary>ピンチ開始時の距離</summary>
        public float InitialPinchDistance { get; private set; }
        /// <summary>前回評価時の距離</summary>
        public float LastPinchDistance { get; set; }
        /// <summary>ピンチ開始時の角度</summary>
        public float InitialPinchAngle { get; private set; }
        /// <summary>前回評価時の角度</summary>
        public float LastPinchAngle { get; set; }

        /// <summary>副タッチを保持しているかどうか</summary>
        public bool HasSecondaryTouch => SecondaryTouchId.HasValue;
        /// <summary>副タッチを追加できるかどうか</summary>
        public bool CanMergeSecondTouch => !HasSecondaryTouch && RecognizedType == GestureRecognitionType.None;

        /// <summary>
        /// 副タッチを関連付ける
        /// </summary>
        /// <param name="touchId">副タッチ ID</param>
        /// <param name="primaryPosition">主タッチ位置</param>
        /// <param name="secondaryPosition">副タッチ位置</param>
        public void AttachSecondaryTouch(int touchId, Vector2 primaryPosition, Vector2 secondaryPosition) {
            SecondaryTouchId = touchId;
            LastPrimaryPosition = primaryPosition;
            LastSecondaryPosition = secondaryPosition;
            InitialPinchDistance = Vector2.Distance(primaryPosition, secondaryPosition);
            LastPinchDistance = InitialPinchDistance;
            InitialPinchAngle = GetAngle(primaryPosition, secondaryPosition);
            LastPinchAngle = InitialPinchAngle;
        }

        /// <summary>
        /// 最新の入力位置を保存
        /// </summary>
        /// <param name="inputSnapshot">入力スナップショット</param>
        public void UpdatePositions(GestureInputSnapshot inputSnapshot) {
            LastPrimaryPosition = inputSnapshot.PrimaryPosition;

            if (inputSnapshot.HasSecondaryTouch) {
                LastSecondaryPosition = inputSnapshot.SecondaryPosition;
            }
        }

        /// <summary>
        /// セッションに属するタッチ ID を列挙
        /// </summary>
        /// <returns>タッチ ID の列挙</returns>
        public IEnumerable<int> EnumerateTouchIds() {
            yield return PrimaryTouchId;

            if (SecondaryTouchId.HasValue) {
                yield return SecondaryTouchId.Value;
            }
        }

        /// <summary>
        /// 2 点間ベクトルの角度を取得
        /// </summary>
        /// <param name="primaryPosition">主タッチ位置</param>
        /// <param name="secondaryPosition">副タッチ位置</param>
        /// <returns>角度</returns>
        private static float GetAngle(Vector2 primaryPosition, Vector2 secondaryPosition) {
            Vector2 direction = secondaryPosition - primaryPosition;
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }
    }
}
