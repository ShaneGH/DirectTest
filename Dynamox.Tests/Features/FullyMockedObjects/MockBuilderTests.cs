using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;
using Dynamox.StronglyTyped;
using NUnit.Framework;

namespace Dynamox.Tests.Features.FullyMockedObjects
{
    [TestFixture]
    public class FullyMockedObjectTestss
    {
        public interface ITestClass
        {
            //TODO
            string this[string key] { get; set; }

            string Property1 { get; set; }
            ITestClass Property2 { get; set; }

            string Method1(int val1);
        }

        [Test]
        public void SmokeTests()
        {
            // Arrange
            // Act
            var mock = Dx.Mock<ITestClass>(new
            {
                Property1 = "Hello",
                Property2 = new
                {
                    Property1 = "Goodbye",
                    Method1 = Dx.Method<int, string>(a =>
                    {
                        return "Well";
                    })
                },
                Method1 = Dx.Method<int, string>(a =>
                {
                    return "Indeed";
                })
            });

            // Assert
            Assert.AreEqual(mock.Property1, "Hello");
            Assert.AreEqual(mock.Property2.Property1, "Goodbye");
            Assert.AreEqual(mock.Method1(1), "Indeed");
            Assert.AreEqual(mock.Property2.Method1(1), "Well");
        }
    }
}