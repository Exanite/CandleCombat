using System;
using Exanite.Core.Properties;
using NUnit.Framework;

namespace Exanite.Core.Tests.Editor.Properties
{
    [TestFixture]
    public class PropertyCollectionTests
    {
        private const string DefaultPropertyName = "Default";

        private static readonly PropertyDefinition<string> StringADefinition = new PropertyDefinition<string>("StringA");
        private static readonly PropertyDefinition<string> StringBDefinition = new PropertyDefinition<string>("StringB");

        private PropertyCollection collection;

        [SetUp]
        public void SetUp()
        {
            collection = new PropertyCollection();
        }

        [Test]
        public void GetProperty_WhenPropertyDoesNotExist_ReturnsNull()
        {
            var property = collection.GetProperty(StringADefinition);

            Assert.IsNull(property);
        }

        [Test]
        public void GetPropertyValue_WhenPropertyDoesNotExist_ThrowsError()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                collection.GetPropertyValue(StringADefinition);
            });
        }

        [Test]
        public void SetPropertyValue_WhenPropertyDoesNotExist_ThrowsError()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                collection.SetPropertyValue(StringADefinition, "");
            });
        }

        [Test]
        public void GetOrAddProperty_WhenCalledTwice_ReturnsSameProperty()
        {
            var propertyA = collection.GetOrAddProperty(StringADefinition);
            var propertyB = collection.GetOrAddProperty(StringADefinition);

            Assert.AreEqual(propertyA, propertyB);
        }

        [Test]
        public void GetOrAddProperty_WhenProvidedDifferentStringDefinitions_ReturnsDifferentProperties()
        {
            var propertyA = collection.GetOrAddProperty(StringADefinition);
            var propertyB = collection.GetOrAddProperty(StringBDefinition);

            Assert.AreNotEqual(propertyA, propertyB);
        }

        [Test]
        public void GetOrAddProperty_WhenProvidedDefaultAndPropertyDoesNotExist_ReturnsPropertyWithProvidedDefault()
        {
            const string defaultValue = "DefaultValue";
            var property = collection.GetOrAddProperty(new PropertyDefinition<string>(DefaultPropertyName, defaultValue));

            Assert.AreEqual(defaultValue, property.Value);
        }

        [Test]
        public void GetOrAddProperty_WhenProvidedDefaultAndPropertyDoesExist_ReturnsPropertyWithExistingValue()
        {
            const string existingDefaultValue = "Existing";
            const string newDefaultValue = "New";

            var propertyA = collection.GetOrAddProperty(new PropertyDefinition<string>(DefaultPropertyName, existingDefaultValue));
            var propertyB = collection.GetOrAddProperty(new PropertyDefinition<string>(DefaultPropertyName, newDefaultValue));

            Assert.AreEqual(propertyA.Value, existingDefaultValue);
            Assert.AreEqual(propertyB.Value, existingDefaultValue);
        }

        [Test]
        public void GetOrAddProperty_WhenCalledTwiceWithDifferentTypes_ThrowsException()
        {
            Assert.Throws<PropertyTypeMismatchException>(() =>
            {
                collection.GetOrAddProperty(new PropertyDefinition<string>(DefaultPropertyName));
                collection.GetOrAddProperty(new PropertyDefinition<int>(DefaultPropertyName));
            });
        }
    }
}