using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace UnityEnhancedGesture.Tests {
    internal sealed class GestureRecognizerTimeTests {
        private const int PointerId = 1;
        private const float StartTime = 10.0f;
        private const float FixedInputTime = 10.1f;

        [Test]
        public void Tap_LongTapUsesCurrentTimeWhenInputTimeIsStationary() {
            var handler = new TapHandler {
                EnableLongTap = true,
                LongTapDuration = 0.8f,
            };
            var recognizer = new TapGestureRecognizer();
            var input = CreateInput(GestureInputPhase.Stationary, Vector2.zero);
            var track = recognizer.CreateTrack(handler, input, null);
            var inputs = CreateInputs(input);

            recognizer.ProcessTrack(track, inputs, 10.2f);
            recognizer.ProcessTrack(track, inputs, 10.5f);

            Assert.That(handler.LongTapProgressEvents, Has.Count.EqualTo(2));
            Assert.That(handler.LongTapProgressEvents[0].Progress, Is.EqualTo(0.25f).Within(0.0001f));
            Assert.That(handler.LongTapProgressEvents[1].Progress, Is.EqualTo(0.625f).Within(0.0001f));
            Assert.That(handler.LongTapEvents, Is.Empty);

            recognizer.ProcessTrack(track, inputs, 10.8f);
            recognizer.ProcessTrack(track, inputs, 11.0f);

            Assert.That(handler.LongTapProgressEvents, Has.Count.EqualTo(3));
            Assert.That(handler.LongTapProgressEvents[2].Phase, Is.EqualTo(GestureEventPhase.Completed));
            Assert.That(handler.LongTapProgressEvents[2].Duration, Is.EqualTo(0.8f).Within(0.0001f));
            Assert.That(handler.LongTapEvents, Has.Count.EqualTo(1));
            Assert.That(handler.LongTapEvents[0].Duration, Is.EqualTo(0.8f).Within(0.0001f));
        }

        [Test]
        public void Tap_EndedBeforeLongTapDurationCancelsProgress() {
            var handler = new TapHandler {
                EnableLongTap = true,
                LongTapDuration = 0.8f,
            };
            var recognizer = new TapGestureRecognizer();
            var beganInput = CreateInput(GestureInputPhase.Stationary, Vector2.zero);
            var track = recognizer.CreateTrack(handler, beganInput, null);

            recognizer.ProcessTrack(track, CreateInputs(beganInput), 10.2f);
            var endedInput = CreateInput(GestureInputPhase.Ended, Vector2.zero);
            recognizer.ProcessTrack(track, CreateInputs(endedInput), 10.5f);

            Assert.That(handler.LongTapProgressEvents, Has.Count.EqualTo(2));
            Assert.That(handler.LongTapProgressEvents[1].Phase, Is.EqualTo(GestureEventPhase.Canceled));
            Assert.That(handler.LongTapEvents, Is.Empty);
            Assert.That(handler.TapEvents, Has.Count.EqualTo(1));
        }

        [Test]
        public void Tap_MovementBeyondLongTapLimitCancelsProgress() {
            var handler = new TapHandler {
                EnableLongTap = true,
                LongTapDuration = 0.8f,
                LongTapMaxMovement = 5.0f,
            };
            var recognizer = new TapGestureRecognizer();
            var beganInput = CreateInput(GestureInputPhase.Stationary, Vector2.zero);
            var track = recognizer.CreateTrack(handler, beganInput, null);

            recognizer.ProcessTrack(track, CreateInputs(beganInput), 10.2f);
            var movedInput = CreateInput(GestureInputPhase.Moved, new Vector2(6.0f, 0.0f));
            recognizer.ProcessTrack(track, CreateInputs(movedInput), 10.3f);

            Assert.That(handler.LongTapProgressEvents, Has.Count.EqualTo(2));
            Assert.That(handler.LongTapProgressEvents[1].Phase, Is.EqualTo(GestureEventPhase.Canceled));
            Assert.That(handler.LongTapEvents, Is.Empty);
        }

        [Test]
        public void Tap_SingleAndDoubleTapStillComplete() {
            var singleHandler = new TapHandler();
            var recognizer = new TapGestureRecognizer();
            var singleInput = CreateInput(GestureInputPhase.Ended, Vector2.zero);
            var singleTrack = recognizer.CreateTrack(singleHandler, singleInput, null);

            recognizer.ProcessTrack(singleTrack, CreateInputs(singleInput), 10.1f);

            Assert.That(singleHandler.TapEvents, Has.Count.EqualTo(1));
            Assert.That(singleHandler.TapEvents[0].Type, Is.EqualTo(TapGestureType.SingleTap));

            var doubleHandler = new TapHandler { EnableDoubleTap = true };
            var firstInput = CreateInput(GestureInputPhase.Ended, Vector2.zero);
            var doubleTrack = recognizer.CreateTrack(doubleHandler, firstInput, null);
            recognizer.ProcessTrack(doubleTrack, CreateInputs(firstInput), 10.1f);
            var secondBeganInput = CreateInput(GestureInputPhase.Began, Vector2.zero, 10.2f, 10.2f);
            Assert.That(recognizer.TryAddPointer(doubleTrack, secondBeganInput), Is.True);
            var secondEndedInput = CreateInput(GestureInputPhase.Ended, Vector2.zero, 10.2f, 10.3f);
            recognizer.ProcessTrack(doubleTrack, CreateInputs(secondEndedInput), 10.3f);

            Assert.That(doubleHandler.DoubleTapEvents, Has.Count.EqualTo(1));
            Assert.That(doubleHandler.DoubleTapEvents[0].Type, Is.EqualTo(TapGestureType.DoubleTap));
        }

        [Test]
        public void Drag_LongTapDragUsesCurrentTimeWhenInputTimeIsStationary() {
            var handler = new DragHandler {
                EnableLongTapDrag = true,
                LongTapDragDuration = 0.8f,
            };
            var recognizer = new DragGestureRecognizer();
            var input = CreateInput(GestureInputPhase.Stationary, Vector2.zero);
            var track = recognizer.CreateTrack(handler, input, null);
            var inputs = CreateInputs(input);

            recognizer.ProcessTrack(track, inputs, 10.2f);
            recognizer.ProcessTrack(track, inputs, 10.5f);
            recognizer.ProcessTrack(track, inputs, 10.8f);
            recognizer.ProcessTrack(track, inputs, 11.0f);

            Assert.That(handler.LongTapDragProgressEvents, Has.Count.EqualTo(3));
            Assert.That(handler.LongTapDragProgressEvents[0].Progress, Is.EqualTo(0.25f).Within(0.0001f));
            Assert.That(handler.LongTapDragProgressEvents[1].Progress, Is.EqualTo(0.625f).Within(0.0001f));
            Assert.That(handler.LongTapDragProgressEvents[2].Phase, Is.EqualTo(GestureEventPhase.Completed));
            Assert.That(handler.BeginDragEvents, Has.Count.EqualTo(1));
            Assert.That(handler.BeginDragEvents[0].StartMode, Is.EqualTo(DragGestureStartMode.LongTap));
            Assert.That(handler.BeginDragEvents[0].Duration, Is.EqualTo(0.8f).Within(0.0001f));
        }

        [Test]
        public void Drag_MovementBeyondLongTapLimitCancelsProgress() {
            var handler = new DragHandler {
                EnableLongTapDrag = true,
                LongTapDragDuration = 0.8f,
                LongTapDragMaxMovement = 5.0f,
                DragStartThreshold = 20.0f,
            };
            var recognizer = new DragGestureRecognizer();
            var input = CreateInput(GestureInputPhase.Stationary, Vector2.zero);
            var track = recognizer.CreateTrack(handler, input, null);

            recognizer.ProcessTrack(track, CreateInputs(input), 10.2f);
            var movedInput = CreateInput(GestureInputPhase.Moved, new Vector2(6.0f, 0.0f));
            recognizer.ProcessTrack(track, CreateInputs(movedInput), 10.3f);

            Assert.That(handler.LongTapDragProgressEvents, Has.Count.EqualTo(2));
            Assert.That(handler.LongTapDragProgressEvents[1].Phase, Is.EqualTo(GestureEventPhase.Canceled));
            Assert.That(handler.BeginDragEvents, Is.Empty);
        }

        [Test]
        public void Drag_EndedBeforeLongTapDurationCancelsProgress() {
            var handler = new DragHandler {
                EnableLongTapDrag = true,
                LongTapDragDuration = 0.8f,
            };
            var recognizer = new DragGestureRecognizer();
            var input = CreateInput(GestureInputPhase.Stationary, Vector2.zero);
            var track = recognizer.CreateTrack(handler, input, null);

            recognizer.ProcessTrack(track, CreateInputs(input), 10.2f);
            var endedInput = CreateInput(GestureInputPhase.Ended, Vector2.zero);
            recognizer.ProcessTrack(track, CreateInputs(endedInput), 10.5f);

            Assert.That(handler.LongTapDragProgressEvents, Has.Count.EqualTo(2));
            Assert.That(handler.LongTapDragProgressEvents[1].Phase, Is.EqualTo(GestureEventPhase.Canceled));
            Assert.That(handler.BeginDragEvents, Is.Empty);
        }

        [Test]
        public void Drag_ImmediateDragStillCompletes() {
            var handler = new DragHandler { DragStartThreshold = 5.0f };
            var recognizer = new DragGestureRecognizer();
            var input = CreateInput(GestureInputPhase.Moved, new Vector2(6.0f, 0.0f));
            var track = recognizer.CreateTrack(handler, input, null);

            recognizer.ProcessTrack(track, CreateInputs(input), 10.2f);
            var endedInput = CreateInput(GestureInputPhase.Ended, new Vector2(6.0f, 0.0f));
            recognizer.ProcessTrack(track, CreateInputs(endedInput), 10.3f);

            Assert.That(handler.BeginDragEvents, Has.Count.EqualTo(1));
            Assert.That(handler.BeginDragEvents[0].StartMode, Is.EqualTo(DragGestureStartMode.Immediate));
            Assert.That(handler.EndDragEvents, Has.Count.EqualTo(1));
        }

        [Test]
        public void Recognizers_UseProvidedUnscaledCurrentTimeWhenTimeScaleIsZero() {
            var previousTimeScale = Time.timeScale;
            Time.timeScale = 0.0f;

            try {
                var handler = new TapHandler {
                    EnableLongTap = true,
                    LongTapDuration = 0.8f,
                };
                var recognizer = new TapGestureRecognizer();
                var input = CreateInput(GestureInputPhase.Stationary, Vector2.zero);
                var track = recognizer.CreateTrack(handler, input, null);

                recognizer.ProcessTrack(track, CreateInputs(input), 10.8f);

                Assert.That(handler.LongTapEvents, Has.Count.EqualTo(1));
            } finally {
                Time.timeScale = previousTimeScale;
            }
        }

        private static GesturePointerInput CreateInput(
            GestureInputPhase phase,
            Vector2 position,
            float startTime = StartTime,
            float inputTime = FixedInputTime) {
            return new GesturePointerInput(
                PointerId,
                phase,
                Vector2.zero,
                position,
                Vector2.zero,
                null,
                startTime,
                inputTime);
        }

        private static IReadOnlyDictionary<int, GesturePointerInput> CreateInputs(GesturePointerInput input) {
            return new Dictionary<int, GesturePointerInput> { [PointerId] = input };
        }

        private sealed class TapHandler : ITapGestureHandler {
            public int Priority => 0;
            public bool IsActiveAndEnabled => true;
            public float MaxTapDuration { get; set; } = 0.5f;
            public float MaxTapMovement { get; set; } = 10.0f;
            public bool EnableDoubleTap { get; set; }
            public float DoubleTapMaxDelay { get; set; } = 0.3f;
            public float DoubleTapMaxMovement { get; set; } = 10.0f;
            public bool EnableLongTap { get; set; }
            public float LongTapDuration { get; set; } = 0.8f;
            public float LongTapMaxMovement { get; set; } = 10.0f;
            public List<TapGestureEvent> TapEvents { get; } = new();
            public List<TapGestureEvent> DoubleTapEvents { get; } = new();
            public List<TapGestureEvent> LongTapEvents { get; } = new();
            public List<LongTapProgressGestureEvent> LongTapProgressEvents { get; } = new();

            public bool CanHandle(Vector2 screenPosition, Camera eventCamera) => true;
            public void HandleTap(TapGestureEvent gestureEvent) => TapEvents.Add(gestureEvent);
            public void HandleDoubleTap(TapGestureEvent gestureEvent) => DoubleTapEvents.Add(gestureEvent);
            public void HandleLongTap(TapGestureEvent gestureEvent) => LongTapEvents.Add(gestureEvent);
            public void HandleLongTapProgress(LongTapProgressGestureEvent gestureEvent) => LongTapProgressEvents.Add(gestureEvent);
        }

        private sealed class DragHandler : IDragGestureHandler {
            public int Priority => 0;
            public bool IsActiveAndEnabled => true;
            public float DragStartThreshold { get; set; } = 10.0f;
            public bool EnableLongTapDrag { get; set; }
            public float LongTapDragDuration { get; set; } = 0.8f;
            public float LongTapDragMaxMovement { get; set; } = 10.0f;
            public List<DragGestureEvent> BeginDragEvents { get; } = new();
            public List<DragGestureEvent> DragEvents { get; } = new();
            public List<DragGestureEvent> EndDragEvents { get; } = new();
            public List<DragGestureEvent> CancelDragEvents { get; } = new();
            public List<LongTapDragProgressGestureEvent> LongTapDragProgressEvents { get; } = new();

            public bool CanHandle(Vector2 screenPosition, Camera eventCamera) => true;
            public void HandleBeginDrag(DragGestureEvent gestureEvent) => BeginDragEvents.Add(gestureEvent);
            public void HandleDrag(DragGestureEvent gestureEvent) => DragEvents.Add(gestureEvent);
            public void HandleEndDrag(DragGestureEvent gestureEvent) => EndDragEvents.Add(gestureEvent);
            public void HandleCancelDrag(DragGestureEvent gestureEvent) => CancelDragEvents.Add(gestureEvent);
            public void HandleLongTapDragProgress(LongTapDragProgressGestureEvent gestureEvent) => LongTapDragProgressEvents.Add(gestureEvent);
        }
    }
}
