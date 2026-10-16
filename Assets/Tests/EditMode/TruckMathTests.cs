using NUnit.Framework;
using TruckGame.Driving;

namespace TruckGame.Tests
{
    public sealed class TruckMathTests
    {
        [Test]
        public void SteeringLimit_UsesLowSpeedAngleBelowReferenceSpeed()
        {
            var result = TruckMath.SteeringLimit(2f, 5f, 20f, 28f, 8f);
            Assert.That(result, Is.EqualTo(28f).Within(0.001f));
        }

        [Test]
        public void SteeringLimit_UsesHighSpeedAngleAboveReferenceSpeed()
        {
            var result = TruckMath.SteeringLimit(25f, 5f, 20f, 28f, 8f);
            Assert.That(result, Is.EqualTo(8f).Within(0.001f));
        }

        [Test]
        public void SteeringLimit_InterpolatesBetweenSpeedReferences()
        {
            var result = TruckMath.SteeringLimit(12.5f, 5f, 20f, 28f, 8f);
            Assert.That(result, Is.EqualTo(18f).Within(0.001f));
        }
    }
}

