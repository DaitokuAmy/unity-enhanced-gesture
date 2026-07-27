using NUnit.Framework;
using UnityEngine;

namespace UnityEnhancedGesture.Tests {
    internal sealed class DragGestureEventVelocityTests {
        [Test]
        public void TryGetRecentVelocity_DuplicateEndPositionUsesRecentMovement() {
            var gestureEvent = CreateEvent(
                new GesturePointerSample(new Vector2(80.0f, 200.0f), 0.164f),
                new GesturePointerSample(new Vector2(100.0f, 200.0f), 0.180f),
                new GesturePointerSample(new Vector2(120.0f, 200.0f), 0.196f),
                new GesturePointerSample(new Vector2(120.0f, 200.0f), 0.230f));

            var succeeded = gestureEvent.TryGetRecentVelocity(0.1f, 0.05f, 1.0f, out var velocity);

            Assert.That(succeeded, Is.True);
            Assert.That(velocity.x, Is.EqualTo(1250.0f).Within(0.01f));
            Assert.That(velocity.y, Is.EqualTo(0.0f).Within(0.01f));
        }

        [Test]
        public void TryGetRecentVelocity_DefaultSettingsUseRecentMovement() {
            var gestureEvent = CreateEvent(
                new GesturePointerSample(new Vector2(100.0f, 200.0f), 0.180f),
                new GesturePointerSample(new Vector2(120.0f, 200.0f), 0.196f),
                new GesturePointerSample(new Vector2(120.0f, 200.0f), 0.230f));

            var succeeded = gestureEvent.TryGetRecentVelocity(0.1f, out var velocity);

            Assert.That(succeeded, Is.True);
            Assert.That(velocity.x, Is.EqualTo(1250.0f).Within(0.01f));
            Assert.That(velocity.y, Is.EqualTo(0.0f).Within(0.01f));
        }

        [Test]
        public void TryGetRecentVelocity_StationaryDurationBeyondLimitReturnsZero() {
            var gestureEvent = CreateEvent(
                new GesturePointerSample(new Vector2(100.0f, 200.0f), 0.180f),
                new GesturePointerSample(new Vector2(120.0f, 200.0f), 0.196f),
                new GesturePointerSample(new Vector2(120.0f, 200.0f), 0.260f));

            var succeeded = gestureEvent.TryGetRecentVelocity(0.1f, 0.05f, 1.0f, out var velocity);

            Assert.That(succeeded, Is.True);
            Assert.That(velocity, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void TryGetRecentVelocity_MovementOutsideWindowIsIgnored() {
            var gestureEvent = CreateEvent(
                new GesturePointerSample(new Vector2(100.0f, 200.0f), 0.100f),
                new GesturePointerSample(new Vector2(120.0f, 200.0f), 0.210f),
                new GesturePointerSample(new Vector2(120.0f, 200.0f), 0.230f));

            var succeeded = gestureEvent.TryGetRecentVelocity(0.1f, 0.05f, 1.0f, out var velocity);

            Assert.That(succeeded, Is.True);
            Assert.That(velocity, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void TryGetRecentVelocity_MultipleSamplesUsesLongestRecentInterval() {
            var gestureEvent = CreateEvent(
                new GesturePointerSample(new Vector2(60.0f, 200.0f), 0.150f),
                new GesturePointerSample(new Vector2(80.0f, 200.0f), 0.170f),
                new GesturePointerSample(new Vector2(100.0f, 200.0f), 0.190f),
                new GesturePointerSample(new Vector2(120.0f, 200.0f), 0.210f));

            var succeeded = gestureEvent.TryGetRecentVelocity(0.1f, 0.05f, 1.0f, out var velocity);

            Assert.That(succeeded, Is.True);
            Assert.That(velocity.x, Is.EqualTo(1000.0f).Within(0.01f));
            Assert.That(velocity.y, Is.EqualTo(0.0f).Within(0.01f));
        }

        [Test]
        public void TryGetRecentVelocity_InvalidArgumentsReturnFalse() {
            var gestureEvent = CreateEvent(
                new GesturePointerSample(Vector2.zero, 0.0f),
                new GesturePointerSample(Vector2.one, 0.1f));

            Assert.That(gestureEvent.TryGetRecentVelocity(0.0f, 0.05f, 1.0f, out _), Is.False);
            Assert.That(gestureEvent.TryGetRecentVelocity(0.1f, -0.01f, 1.0f, out _), Is.False);
            Assert.That(gestureEvent.TryGetRecentVelocity(0.1f, 0.05f, -1.0f, out _), Is.False);
            Assert.That(default(DragGestureEvent).TryGetRecentVelocity(0.1f, out _), Is.False);
        }

        private static DragGestureEvent CreateEvent(params GesturePointerSample[] samples) {
            return new DragGestureEvent(
                GestureEventPhase.Completed,
                DragGestureStartMode.Immediate,
                samples[0].Position,
                samples[samples.Length - 1].Position,
                Vector2.zero,
                samples[samples.Length - 1].Position - samples[0].Position,
                samples,
                samples[samples.Length - 1].ElapsedTime,
                1,
                null);
        }
    }
}
