using UnityEditor;
using UnityEngine;
using UnityEnhancedGesture;

namespace UnityEnhancedGesture.Editor {
    /// <summary>
    /// ジェスチャーハンドラー共通インスペクター
    /// </summary>
    [CustomEditor(typeof(GestureHandlerBase), true)]
    [CanEditMultipleObjects]
    public sealed class GestureHandlerBaseEditor : UnityEditor.Editor {
        /// <inheritdoc/>
        public override void OnInspectorGUI() {
            serializedObject.Update();
            DrawPriority();
            DrawUIBlocking();

            if (target is DragGestureHandlerUI) {
                DrawDragInspector("_targetRectTransform");
            }
            else if (target is DragGestureHandler3D) {
                DrawDragInspector("_targetColliders");
            }
            else if (target is TapGestureHandlerUI) {
                DrawTapInspector("_targetRectTransform");
            }
            else if (target is TapGestureHandler3D) {
                DrawTapInspector("_targetColliders");
            }
            else if (target is PinchGestureHandlerUI) {
                DrawPinchInspector("_targetRectTransform");
            }
            else if (target is PinchGestureHandler3D) {
                DrawPinchInspector("_targetColliders");
            }
            else {
                DrawDefaultInspector();
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 優先度を描画
        /// </summary>
        private void DrawPriority() {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_priority"));
        }

        /// <summary>
        /// uGUI ブロック設定を描画
        /// </summary>
        private void DrawUIBlocking() {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_isBlockedByUI"));
        }

        /// <summary>
        /// ドラッグハンドラー用インスペクターを描画
        /// </summary>
        /// <param name="targetPropertyName">対象参照プロパティ名</param>
        private void DrawDragInspector(string targetPropertyName) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(targetPropertyName));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_dragStartThreshold"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_enableLongTapDrag"));

            if (serializedObject.FindProperty("_enableLongTapDrag").boolValue) {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_longTapDragDuration"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_longTapDragMaxMovement"));
            }
        }

        /// <summary>
        /// タップハンドラー用インスペクターを描画
        /// </summary>
        /// <param name="targetPropertyName">対象参照プロパティ名</param>
        private void DrawTapInspector(string targetPropertyName) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(targetPropertyName));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_maxTapDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_maxTapMovement"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_enableDoubleTap"));

            if (serializedObject.FindProperty("_enableDoubleTap").boolValue) {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_doubleTapMaxDelay"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_doubleTapMaxMovement"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_enableLongTap"));

            if (serializedObject.FindProperty("_enableLongTap").boolValue) {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_longTapDuration"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_longTapMaxMovement"));
            }
        }

        /// <summary>
        /// ピンチハンドラー用インスペクターを描画
        /// </summary>
        /// <param name="targetPropertyName">対象参照プロパティ名</param>
        private void DrawPinchInspector(string targetPropertyName) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(targetPropertyName));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_pinchStartThreshold"));
        }
    }
}
