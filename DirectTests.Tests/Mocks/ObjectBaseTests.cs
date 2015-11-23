using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectTests.Mocks;
using NUnit.Framework;

namespace DirectTests.Tests.Mocks
{
    [TestFixture]
    public class ObjectBaseTests
    {
        #region GetSet

        [Test]
        public void GetSetProperties()
        {
            // arrange
            var prop1 = new object();
            var prop2 = new object();
            var prop3 = new object();
            var subject = new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object> { { "abc", prop1 } }));

            // act
            // assert
            Assert.AreEqual(prop1, subject.GetProperty<object>("abc"));
            subject.SetProperty("abc", prop2);
            Assert.AreEqual(prop2, subject.GetProperty<object>("abc"));
            subject.SetProperty("cde", prop3);
            Assert.AreEqual(prop3, subject.GetProperty<object>("cde"));
        }

        [Test]
        public void GetPropertyDoesntExistNonStrict()
        {
            // arrange
            var subject = new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()), false);

            // act
            // assert
            Assert.AreEqual(null, subject.GetProperty<object>("abc"));
            Assert.AreEqual(0, subject.GetProperty<int>("abc"));
        }

        [Test]
        public void GetPropertyDoesntExistStrict()
        {
            // arrange
            var subject = new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()), true);

            // act
            // assert
            Assert.Throws(typeof(InvalidOperationException), () => subject.GetProperty<object>("abc"));
        }

        [Test]
        public void GetInvalidPropertyType()
        {
            // arrange
            var subject = new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object> { { "abc", new object() } }));

            // act
            // assert
            Assert.Throws(typeof(InvalidOperationException), () => subject.GetProperty<string>("abc"));
        }

        #endregion

        #region Invoke void

        [Test]
        public void Invoke_Void_HasMock()
        {
            //TODO: this method needs mocking

            // arrange
            var ok = false;
            var arg = new object();
            var method = new MethodMockBuilder(null, new[] 
            { 
                Framework.Method<object>(a =>
                {
                    ok = true;
                    return a == arg;
                }) 
            });

            var subject = new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }));

            // act
            // assert
            subject.Invoke("abc", new[] { new KeyValuePair<Type, object>(arg.GetType(), arg) });
            Assert.IsTrue(ok);
        }

        [Test]
        public void Invoke_Void_NoMock_NonStrict()
        {
            //TODO: this method needs mocking

            // arrange
            var arg = new object();

            var subject = new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup() }
            }));

            // act
            // assert
            subject.Invoke("abc", new[] { new KeyValuePair<Type, object>(arg.GetType(), arg) });
        }

        [Test]
        public void Invoke_Void_NoMock_Strict()
        {
            //TODO: this method needs mocking

            // arrange
            var arg = new object();

            var subject = new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup() }
            }), true);

            // act
            // assert
            Assert.Throws(typeof(InvalidOperationException), () =>
                subject.Invoke("abc", new[] { new KeyValuePair<Type, object>(arg.GetType(), arg) }));
        }

        [Test]
        public void Invoke_Void_NoValidMock_NonStrict()
        {
            //TODO: this method needs mocking

            // arrange
            var ok = false;
            var arg = new object();
            var method = new MethodMockBuilder(null, new[] 
            { 
                Framework.Method<object>(a =>
                {
                    ok = true;
                    Assert.AreEqual(a, arg);
                    return false;
                }) 
            });

            var subject = new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }));

            // act
            // assert
            subject.Invoke("abc", new[] { new KeyValuePair<Type, object>(arg.GetType(), arg) });
            Assert.IsTrue(ok);
        }

        [Test]
        public void Invoke_Void_NoValidMock_Strict()
        {
            //TODO: this method needs mocking

            // arrange
            var arg = new object();
            var method = new MethodMockBuilder(null, new[] 
            { 
                Framework.Method<object>(a =>
                {
                    Assert.AreEqual(a, arg);
                    return false;
                }) 
            });

            var subject = new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }), true);

            // act
            // assert
            Assert.Throws(typeof(InvalidOperationException), () =>
                subject.Invoke("abc", new[] { new KeyValuePair<Type, object>(arg.GetType(), arg) }));
        }

        #endregion

        #region invoke non void

        [Test]
        public void Invoke_NonVoid_HasMock()
        {
            //TODO: this method needs mocking

            // arrange
            var ok = false;
            var arg = new object();
            var returnVal = new object();
            var method = new MethodMockBuilder(null, new[] 
            { 
                Framework.Method<object>(a =>
                {
                    ok = true;
                    return a == arg;
                }) 
            });
            (method as dynamic).Return(returnVal);

            var subject = new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }));

            // act
            // assert
            var output = subject.Invoke<object>("abc", new[] { new KeyValuePair<Type, object>(arg.GetType(), arg) });
            Assert.AreEqual(returnVal, output);
            Assert.IsTrue(ok);
        }

        [Test]
        public void Invoke_NonVoid_Null()
        {
            //TODO: this method needs mocking

            // arrange
            var ok = false;
            var arg = new object();
            object returnVal = null;
            var method = new MethodMockBuilder(null, new[] 
            { 
                Framework.Method<object>(a =>
                {
                    ok = true;
                    return a == arg;
                }) 
            });
            (method as dynamic).Return(returnVal);

            var subject = new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }));

            // act
            // assert
            var output = subject.Invoke<object>("abc", new[] { new KeyValuePair<Type, object>(arg.GetType(), arg) });
            Assert.AreEqual(returnVal, output);
            Assert.IsTrue(ok);
        }

        [Test]
        public void Invoke_NonVoid_ValueType()
        {
            //TODO: this method needs mocking

            // arrange
            var ok = false;
            var arg = new object();
            var returnVal = 555;
            var method = new MethodMockBuilder(null, new[] 
            { 
                Framework.Method<object>(a =>
                {
                    ok = true;
                    return a == arg;
                }) 
            });
            (method as dynamic).Return(returnVal);

            var subject = new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }));

            // act
            // assert
            var output = subject.Invoke<int>("abc", new[] { new KeyValuePair<Type, object>(arg.GetType(), arg) });
            Assert.AreEqual(returnVal, output);
            Assert.IsTrue(ok);
        }

        [Test]
        public void Invoke_NonVoid_Cast()
        {
            //TODO: this method needs mocking

            // arrange
            var ok = false;
            var arg = new object();
            var returnVal = 555;
            var method = new MethodMockBuilder(null, new[] 
            { 
                Framework.Method<object>(a =>
                {
                    ok = true;
                    return a == arg;
                }) 
            });
            (method as dynamic).Return(returnVal);

            var subject = new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }));

            // act
            // assert
            var output = subject.Invoke<object>("abc", new[] { new KeyValuePair<Type, object>(arg.GetType(), arg) });
            Assert.AreEqual(returnVal, output);
            Assert.IsTrue(ok);
        }

        [Test]
        public void Invoke_NonVoid_VoidMethod()
        {
            //TODO: this method needs mocking

            // arrange
            var ok = false;
            var arg = new object();
            var method = new MethodMockBuilder(null, new[] 
            { 
                Framework.Method<object>(a =>
                {
                    ok = true;
                    return true;
                }) 
            });

            var subject = new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }));

            // act
            // assert
            var output = subject.Invoke<object>("abc", new[] { new KeyValuePair<Type, object>(arg.GetType(), arg) });
            Assert.IsNull(output);
            Assert.IsTrue(ok);
        }

        [Test]
        public void Invoke_NonVoid_VoidMethod_RefType()
        {
            //TODO: this method needs mocking

            // arrange
            var ok = false;
            var arg = new object();
            var method = new MethodMockBuilder(null, new[] 
            { 
                Framework.Method<object>(a =>
                {
                    ok = true;
                    return true;
                }) 
            });

            var subject = new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }));

            // act
            // assert
            var output = subject.Invoke<int>("abc", new[] { new KeyValuePair<Type, object>(arg.GetType(), arg) });
            Assert.AreEqual(output, 0);
            Assert.IsTrue(ok);
        }

        [Test]
        public void Invoke_NonVoid_NoMock_NonStrict()
        {
            //TODO: this method needs mocking

            // arrange
            var arg = new object();

            var subject = new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup() }
            }));

            // act
            // assert
            var output = subject.Invoke<object>("abc", new[] { new KeyValuePair<Type, object>(arg.GetType(), arg) });
            Assert.IsNull(output);
        }

        [Test]
        public void Invoke_NonVoid_NoMock_Strict()
        {
            //TODO: this method needs mocking

            // arrange
            var arg = new object();

            var subject = new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup() }
            }), true);

            // act
            // assert
            Assert.Throws(typeof(InvalidOperationException), () =>
                subject.Invoke("abc", new[] { new KeyValuePair<Type, object>(arg.GetType(), arg) }));
        }

        [Test]
        public void Invoke_NonVoid_NoValidMock_NonStrict()
        {
            //TODO: this method needs mocking

            // arrange
            var ok = false;
            var arg = new object();
            var method = new MethodMockBuilder(null, new[] 
            { 
                Framework.Method<object>(a =>
                {
                    ok = true;
                    Assert.AreEqual(a, arg);
                    return false;
                }) 
            });

            var subject = new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }));

            // act
            // assert
            var output = subject.Invoke<object>("abc", new[] { new KeyValuePair<Type, object>(arg.GetType(), arg) });
            Assert.IsTrue(ok);
            Assert.IsNull(output);
        }

        [Test]
        public void Invoke_NonVoid_NoValidMock_Strict()
        {
            //TODO: this method needs mocking

            // arrange
            var arg = new object();
            var method = new MethodMockBuilder(null, new[] 
            { 
                Framework.Method<object>(a =>
                {
                    Assert.AreEqual(a, arg);
                    return false;
                }) 
            });

            var subject = new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }), true);

            // act
            // assert
            Assert.Throws(typeof(InvalidOperationException), () =>
                subject.Invoke("abc", new[] { new KeyValuePair<Type, object>(arg.GetType(), arg) }));
        }

        #endregion
    }
}
