using UnityEngine;
using UnityEngine.Serialization;
using UnityEnhancedGesture;

/// <summary>
/// サンプルカメラ操作
/// </summary>
public sealed class SampleCamera : MonoBehaviour {
    [FormerlySerializedAs("_dragHandler"), FormerlySerializedAs("_rectTransformDragHandler"), FormerlySerializedAs("_dragGestureHandlerUGui"), SerializeField, Tooltip("ドラッグ判定用 UI ハンドラー")]
    private DragGestureHandlerUI _dragGestureHandlerUI;

    private bool _isDragging;
    private float _dragPlaneHeight;
    private Vector3 _dragStartPivotPosition;
    private Vector3 _dragStartOffsetFromCenter;

    private void OnEnable() {
        _dragGestureHandlerUI.BeginDragEvent += OnBeginDrag;
        _dragGestureHandlerUI.DragEvent += OnDrag;
        _dragGestureHandlerUI.EndDragEvent += OnEndDrag;
        _dragGestureHandlerUI.CancelDragEvent += OnCancelDrag;
    }

    private void OnDisable() {
        _dragGestureHandlerUI.BeginDragEvent -= OnBeginDrag;
        _dragGestureHandlerUI.DragEvent -= OnDrag;
        _dragGestureHandlerUI.EndDragEvent -= OnEndDrag;
        _dragGestureHandlerUI.CancelDragEvent -= OnCancelDrag;
    }

    private void OnBeginDrag(DragGestureEvent gestureEvent) {
        if (gestureEvent.EventCamera == null) {
            return;
        }

        _dragPlaneHeight = transform.position.y;

        if (!TryGetDragPoint(gestureEvent.EventCamera, gestureEvent.StartPosition, out var dragPoint)) {
            return;
        }

        if (!TryGetDragPoint(gestureEvent.EventCamera, GetScreenCenter(), out var centerPoint)) {
            return;
        }

        _dragStartPivotPosition = transform.position;
        _dragStartOffsetFromCenter = dragPoint - centerPoint;
        _isDragging = true;
    }

    private void OnDrag(DragGestureEvent gestureEvent) {
        if (!_isDragging || gestureEvent.EventCamera == null) {
            return;
        }

        if (!TryGetDragPoint(gestureEvent.EventCamera, gestureEvent.Position, out var dragPoint)) {
            return;
        }

        if (!TryGetDragPoint(gestureEvent.EventCamera, GetScreenCenter(), out var centerPoint)) {
            return;
        }

        var currentOffsetFromCenter = dragPoint - centerPoint;
        transform.position = _dragStartPivotPosition + _dragStartOffsetFromCenter - currentOffsetFromCenter;
    }

    private void OnEndDrag(DragGestureEvent gestureEvent) {
        OnDrag(gestureEvent);
        _isDragging = false;
    }

    private void OnCancelDrag(DragGestureEvent gestureEvent) {
        _isDragging = false;
    }

    private bool TryGetDragPoint(Camera eventCamera, Vector2 screenPosition, out Vector3 dragPoint) {
        dragPoint = default;

        var dragPlane = new Plane(Vector3.up, new Vector3(0.0f, _dragPlaneHeight, 0.0f));
        var ray = eventCamera.ScreenPointToRay(screenPosition);

        if (!dragPlane.Raycast(ray, out var distance)) {
            return false;
        }

        dragPoint = ray.GetPoint(distance);
        return true;
    }

    private Vector2 GetScreenCenter() {
        return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }
}
