using UnityEngine;
using UnityEnhancedGesture;

/// <summary>
/// サンプルカメラ操作
/// </summary>
public sealed class SampleCamera : MonoBehaviour {
    private const float PinchZoomDeltaSensitivity = 0.03f;

    [SerializeField, Tooltip("ドラッグ判定用 UI ハンドラー")]
    private DragGestureHandlerUI _dragGestureHandlerUI;
    [SerializeField, Tooltip("ピンチ判定用 UI ハンドラー")]
    private PinchGestureHandlerUI _pinchGestureHandlerUI;

    [SerializeField, Tooltip("注視点")]
    private Transform _lookAt;
    [SerializeField, Tooltip("距離")]
    private Transform _distance;

    private bool _isDragging;
    private bool _isPinching;
    private float _dragPlaneHeight;
    private Vector3 _dragStartPivotPosition;
    private Vector3 _dragStartOffsetFromCenter;

    private void OnEnable() {
        _dragGestureHandlerUI.BeginDragEvent += OnBeginDrag;
        _dragGestureHandlerUI.DragEvent += OnDrag;
        _dragGestureHandlerUI.EndDragEvent += OnEndDrag;
        _dragGestureHandlerUI.CancelDragEvent += OnCancelDrag;
        _pinchGestureHandlerUI.BeginPinchEvent += OnBeginPinch;
        _pinchGestureHandlerUI.PinchEvent += OnPinch;
        _pinchGestureHandlerUI.EndPinchEvent += OnEndPinch;
        _pinchGestureHandlerUI.CancelPinchEvent += OnCancelPinch;
    }

    private void OnDisable() {
        _dragGestureHandlerUI.BeginDragEvent -= OnBeginDrag;
        _dragGestureHandlerUI.DragEvent -= OnDrag;
        _dragGestureHandlerUI.EndDragEvent -= OnEndDrag;
        _dragGestureHandlerUI.CancelDragEvent -= OnCancelDrag;
        _pinchGestureHandlerUI.BeginPinchEvent -= OnBeginPinch;
        _pinchGestureHandlerUI.PinchEvent -= OnPinch;
        _pinchGestureHandlerUI.EndPinchEvent -= OnEndPinch;
        _pinchGestureHandlerUI.CancelPinchEvent -= OnCancelPinch;
    }

    private void OnBeginDrag(DragGestureEvent gestureEvent) {
        if (gestureEvent.EventCamera == null || gestureEvent.ActivePointerCount >= 2) {
            return;
        }

        _dragPlaneHeight = _lookAt.position.y;

        if (!TryGetDragPoint(gestureEvent.EventCamera, gestureEvent.StartPosition, out var dragPoint)) {
            return;
        }

        if (!TryGetDragPoint(gestureEvent.EventCamera, GetScreenCenter(), out var centerPoint)) {
            return;
        }

        _dragStartPivotPosition = _lookAt.position;
        _dragStartOffsetFromCenter = dragPoint - centerPoint;
        _isDragging = true;
    }

    private void OnDrag(DragGestureEvent gestureEvent) {
        if (gestureEvent.ActivePointerCount >= 2) {
            _isDragging = false;
            return;
        }

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
        _lookAt.position = _dragStartPivotPosition + _dragStartOffsetFromCenter - currentOffsetFromCenter;
    }

    private void OnEndDrag(DragGestureEvent gestureEvent) {
        OnDrag(gestureEvent);
        _isDragging = false;
    }

    private void OnCancelDrag(DragGestureEvent gestureEvent) {
        _isDragging = false;
    }

    private void OnBeginPinch(PinchGestureEvent gestureEvent) {
        _isDragging = false;
        _isPinching = true;
        ApplyPinch(gestureEvent);
    }

    private void OnPinch(PinchGestureEvent gestureEvent) {
        if (!_isPinching) {
            return;
        }

        ApplyPinch(gestureEvent);
    }

    private void OnEndPinch(PinchGestureEvent gestureEvent) {
        if (_isPinching) {
            ApplyPinch(gestureEvent);
        }

        _isPinching = false;
    }

    private void OnCancelPinch(PinchGestureEvent gestureEvent) {
        _isPinching = false;
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

    private void ApplyPinch(PinchGestureEvent gestureEvent) {
        if (Mathf.Approximately(gestureEvent.DeltaDistance, 0.0f)) {
            return;
        }

        var localPosition = _distance.localPosition;
        localPosition.z += gestureEvent.DeltaDistance * PinchZoomDeltaSensitivity;
        _distance.localPosition = localPosition;
    }
}
