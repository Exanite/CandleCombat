using Exanite.Core.Events;
using Exanite.Core.Properties;
using NUnit.Framework;

namespace Exanite.Core.Tests.Editor.Properties
{
    [TestFixture]
    public class PropertyTests
    {
        private const string DefaultPropertyName = "Default";

        [Test]
        public void UntypedValue_IsEqualToValue()
        {
            var property = new Property<string>(DefaultPropertyName, "A");

            Assert.AreEqual(property.UntypedValue, property.Value);

            property.Value = "B";

            Assert.AreEqual(property.UntypedValue, property.Value);
        }

        [Test]
        public void ValueChanged_OnValueChanged_IsRaisedWithCorrectValues()
        {
            const string previousValue = "Previous";
            const string newValue = "New";

            var property = new Property<string>(DefaultPropertyName, previousValue);
            var valueChangedRecorder = new EventRaisedRecorder();
            property.ValueChanged += (sender, args) =>
            {
                valueChangedRecorder.OnEventRaised();

                Assert.AreEqual(args.Property, property);
                Assert.AreEqual(args.PreviousValue, previousValue);
                Assert.AreEqual(args.NewValue, newValue);
            };

            property.Value = newValue;

            Assert.IsTrue(valueChangedRecorder.IsRaised);
        }

        [Test]
        public void UntypedValueChanged_OnValueChanged_IsRaisedWithCorrectValues()
        {
            const string previousValue = "Previous";
            const string newValue = "New";

            var property = new Property<string>(DefaultPropertyName, previousValue);
            var valueChangedRecorder = new EventRaisedRecorder();
            property.UntypedValueChanged += (sender, args) =>
            {
                valueChangedRecorder.OnEventRaised();

                Assert.AreEqual(args.Property, property);
                Assert.AreEqual(args.PreviousValue, previousValue);
                Assert.AreEqual(args.NewValue, newValue);
            };

            property.Value = newValue;

            Assert.IsTrue(valueChangedRecorder.IsRaised);
        }
    }
}