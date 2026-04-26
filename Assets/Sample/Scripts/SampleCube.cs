using UnityEngine;
using UnityEnhancedGesture;

/// <summary>
/// 3D ドラッグサンプル用の挙動
/// </summary>
public sealed class SampleCube : MonoBehaviour {
    [SerializeField, Tooltip("監視対象の 3D ドラッグハンドラー")]
    private DragGestureHandler3D _dragGestureHandler3D;

    private bool _isDragging;

    private void OnEnable() {
        if (_dragGestureHandler3D == null) {
            return;
        }

        _dragGestureHandler3D.BeginDragEvent += OnBeginDrag;
        _dragGestureHandler3D.DragEvent += OnDrag;
        _dragGestureHandler3D.EndDragEvent += OnEndDrag;
        _dragGestureHandler3D.CancelDragEvent += OnCancelDrag;
    }

    private void OnDisable() {
        if (_dragGestureHandler3D == null) {
            return;
        }

        _dragGestureHandler3D.BeginDragEvent -= OnBeginDrag;
        _dragGestureHandler3D.DragEvent -= OnDrag;
        _dragGestureHandler3D.EndDragEvent -= OnEndDrag;
        _dragGestureHandler3D.CancelDragEvent -= OnCancelDrag;
    }

    private void OnBeginDrag(DragGestureEvent gestureEvent) {
        if (!TryGetGroundPoint(gestureEvent, out var groundPoint)) {
            return;
        }

        _isDragging = true;
        ApplyGroundPoint(groundPoint);
    }

    private void OnDrag(DragGestureEvent gestureEvent) {
        if (!_isDragging || !TryGetGroundPoint(gestureEvent, out var groundPoint)) {
            return;
        }

        ApplyGroundPoint(groundPoint);
    }

    private void OnEndDrag(DragGestureEvent gestureEvent) {
        OnDrag(gestureEvent);
        _isDragging = false;
    }

    private void OnCancelDrag(DragGestureEvent gestureEvent) {
        _isDragging = false;
    }

    private void ApplyGroundPoint(Vector3 groundPoint) {
        var currentPosition = transform.position;
        transform.position = new Vector3(groundPoint.x, currentPosition.y, groundPoint.z);
    }

    private bool TryGetGroundPoint(DragGestureEvent gestureEvent, out Vector3 groundPoint) {
        groundPoint = default;

        if (gestureEvent.EventCamera == null) {
            return false;
        }

        var currentPosition = transform.position;
        var dragPlane = new Plane(Vector3.up, new Vector3(0.0f, currentPosition.y, 0.0f));
        var ray = gestureEvent.EventCamera.ScreenPointToRay(gestureEvent.Position);

        if (!dragPlane.Raycast(ray, out var distance)) {
            return false;
        }

        groundPoint = ray.GetPoint(distance);
        return true;
    }
}
