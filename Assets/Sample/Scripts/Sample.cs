using System;
using UnityEngine;
using UnityEnhancedGesture;

/// <summary>
/// サンプルコード
/// </summary>
public sealed class Sample : MonoBehaviour {
    [SerializeField, Tooltip("ドラッグ判定用ハンドラー")]
    private DragGestureHandler _dragHandler;

    private void OnEnable() {
        _dragHandler.BeginDragEvent += OnBeginDrag;
        _dragHandler.DragEvent += OnDrag;
        _dragHandler.EndDragEvent += OnEndDrag;
        _dragHandler.CancelDragEvent += OnCancelDrag;
    }

    private void OnDisable() {
        _dragHandler.BeginDragEvent -= OnBeginDrag;
        _dragHandler.DragEvent -= OnDrag;
        _dragHandler.EndDragEvent -= OnEndDrag;
        _dragHandler.CancelDragEvent -= OnCancelDrag;
    }

    private void OnBeginDrag(DragGestureEvent gestureEvent) {
        Debug.Log(FormatEventLog("BeginDrag", gestureEvent), this);
    }

    private void OnDrag(DragGestureEvent gestureEvent) {
        Debug.Log(FormatEventLog("Drag", gestureEvent), this);
    }

    private void OnEndDrag(DragGestureEvent gestureEvent) {
        Debug.Log(FormatEventLog("EndDrag", gestureEvent), this);
    }

    private void OnCancelDrag(DragGestureEvent gestureEvent) {
        Debug.Log(FormatEventLog("CancelDrag", gestureEvent), this);
    }

    private string FormatEventLog(string eventName, DragGestureEvent gestureEvent) {
        return
            $"{eventName} Phase:{gestureEvent.Phase} " +
            $"Start:{gestureEvent.StartPosition} " +
            $"Position:{gestureEvent.Position} " +
            $"Delta:{gestureEvent.Delta} " +
            $"TotalDelta:{gestureEvent.TotalDelta} " +
            $"Duration:{gestureEvent.Duration:0.000} " +
            $"Positions:{FormatPositions(gestureEvent.Positions)}";
    }

    private string FormatPositions(Vector2[] positions) {
        if (positions == null || positions.Length <= 0) {
            return "[]";
        }

        return $"[{string.Join(", ", Array.ConvertAll(positions, position => position.ToString()))}]";
    }
}
