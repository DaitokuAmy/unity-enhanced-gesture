using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEnhancedGesture;

public sealed class SampleLongTapDragProgressGauge : MonoBehaviour {
    [SerializeField, Tooltip("進捗表示対象の 3D ドラッグハンドラー群。空の場合はシーン上から自動収集します")]
    private DragGestureHandler3D[] _dragGestureHandlers = Array.Empty<DragGestureHandler3D>();
    [SerializeField, Tooltip("ハンドラー未指定時にシーン上の 3D ドラッグハンドラーを自動収集するかどうか")]
    private bool _findHandlersOnAwake = true;
    [SerializeField, Tooltip("ゲージを配置する Canvas")]
    private Canvas _canvas;
    [SerializeField, Tooltip("移動対象の RectTransform")]
    private RectTransform _gaugeRectTransform;
    [SerializeField, Tooltip("表示/非表示を制御する CanvasGroup")]
    private CanvasGroup _canvasGroup;
    [SerializeField, Tooltip("fillAmount を更新する Image")]
    private Image _progressImage;
    [SerializeField, Tooltip("タップ位置からの表示オフセット")]
    private Vector2 _screenOffset = new Vector2(0.0f, 48.0f);
    [SerializeField, Tooltip("Canvas の範囲内に収めるかどうか")]
    private bool _clampToCanvas = true;

    private Vector2 _currentScreenPosition;
    private readonly Dictionary<DragGestureHandler3D, Action<LongTapDragProgressGestureEvent>> _subscriptions = new();

    private void Awake() {
        if (_canvas == null) {
            _canvas = GetComponentInParent<Canvas>();
        }

        if (_gaugeRectTransform == null) {
            _gaugeRectTransform = transform as RectTransform;
        }

        if (_canvasGroup == null) {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        Hide();
    }

    private void OnEnable() {
        RefreshSubscriptions();
        Hide();
    }

    private void OnDisable() {
        ClearSubscriptions();
    }

    private void RefreshSubscriptions() {
        ClearSubscriptions();

        var handlers = ResolveHandlers();
        for (var i = 0; i < handlers.Length; i++) {
            var dragGestureHandler = handlers[i];
            if (dragGestureHandler == null || _subscriptions.ContainsKey(dragGestureHandler)) {
                continue;
            }

            void OnProgress(LongTapDragProgressGestureEvent gestureEvent) {
                OnLongTapDragProgress(dragGestureHandler, gestureEvent);
            }

            _subscriptions.Add(dragGestureHandler, OnProgress);
            dragGestureHandler.LongTapDragProgressEvent += OnProgress;
        }
    }

    private DragGestureHandler3D[] ResolveHandlers() {
        if (_dragGestureHandlers != null && _dragGestureHandlers.Length > 0) {
            return _dragGestureHandlers;
        }

        if (!_findHandlersOnAwake) {
            return Array.Empty<DragGestureHandler3D>();
        }

        return FindObjectsByType<DragGestureHandler3D>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    }

    private void ClearSubscriptions() {
        foreach (var subscription in _subscriptions) {
            if (subscription.Key != null) {
                subscription.Key.LongTapDragProgressEvent -= subscription.Value;
            }
        }

        _subscriptions.Clear();
    }

    private void OnLongTapDragProgress(DragGestureHandler3D sourceHandler, LongTapDragProgressGestureEvent gestureEvent) {
        switch (gestureEvent.Phase) {
            case GestureEventPhase.Began:
                _currentScreenPosition = ResolveScreenPosition(sourceHandler, gestureEvent);
                ShowAt(_currentScreenPosition, gestureEvent.Progress);
                break;
            case GestureEventPhase.Updated:
                ShowAt(_currentScreenPosition, gestureEvent.Progress);
                break;
            case GestureEventPhase.Completed:
                ShowAt(_currentScreenPosition, 1.0f);
                Hide();
                break;
            case GestureEventPhase.Canceled:
                Hide();
                break;
        }
    }

    private Vector2 ResolveScreenPosition(DragGestureHandler3D sourceHandler, LongTapDragProgressGestureEvent gestureEvent) {
        if (TryGetHitWorldPosition(sourceHandler, gestureEvent, out var worldPosition)) {
            return gestureEvent.EventCamera.WorldToScreenPoint(worldPosition);
        }

        return gestureEvent.StartPosition;
    }

    private bool TryGetHitWorldPosition(DragGestureHandler3D sourceHandler, LongTapDragProgressGestureEvent gestureEvent, out Vector3 worldPosition) {
        worldPosition = default;

        if (sourceHandler == null || gestureEvent.EventCamera == null) {
            return false;
        }

        var colliders = sourceHandler.TargetColliders;
        if (colliders == null) {
            return false;
        }

        var ray = gestureEvent.EventCamera.ScreenPointToRay(gestureEvent.StartPosition);
        var hasHit = false;
        var closestDistance = float.PositiveInfinity;

        for (var i = 0; i < colliders.Count; i++) {
            var targetCollider = colliders[i];
            if (targetCollider == null || !targetCollider.Raycast(ray, out var hit, float.PositiveInfinity)) {
                continue;
            }

            if (hasHit && hit.distance >= closestDistance) {
                continue;
            }

            closestDistance = hit.distance;
            worldPosition = hit.point;
            hasHit = true;
        }

        return hasHit;
    }

    private void ShowAt(Vector2 screenPosition, float progress) {
        SetProgress(progress);
        MoveToScreenPosition(screenPosition + _screenOffset);
        SetVisible(true);
    }

    private void MoveToScreenPosition(Vector2 screenPosition) {
        if (_canvas == null || _gaugeRectTransform == null) {
            return;
        }

        var canvasTransform = (RectTransform)_canvas.transform;
        var canvasCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasTransform, screenPosition, canvasCamera, out var localPosition)) {
            return;
        }

        _gaugeRectTransform.anchoredPosition = _clampToCanvas ? ClampToCanvas(canvasTransform, localPosition) : localPosition;
    }

    private Vector2 ClampToCanvas(RectTransform canvasTransform, Vector2 localPosition) {
        var canvasRect = canvasTransform.rect;
        var gaugeRect = _gaugeRectTransform.rect;
        var halfWidth = gaugeRect.width * 0.5f;
        var halfHeight = gaugeRect.height * 0.5f;

        return new Vector2(
            Mathf.Clamp(localPosition.x, canvasRect.xMin + halfWidth, canvasRect.xMax - halfWidth),
            Mathf.Clamp(localPosition.y, canvasRect.yMin + halfHeight, canvasRect.yMax - halfHeight));
    }

    private void SetProgress(float progress) {
        if (_progressImage == null) {
            return;
        }

        _progressImage.fillAmount = Mathf.Clamp01(progress);
    }

    private void SetVisible(bool visible) {
        if (_canvasGroup != null) {
            _canvasGroup.alpha = visible ? 1.0f : 0.0f;
        }
    }

    private void Hide() {
        SetProgress(0.0f);
        SetVisible(false);
    }
}
