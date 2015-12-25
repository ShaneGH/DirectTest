using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;
using NUnit.Framework;

namespace Dynamox.Tests.Mocks
{
    [TestFixture]
    public class ObjectBaseTests
    {
        #region GetSet

        public class C1 { }
        public class C2 : C1 { }

        [Test]
        public void HasFieldOrProperty()
        {
            // arrange
            dynamic mock = new MockBuilder();
            mock.Prop1 = new C1();
            mock.Prop2 = Dx.Property(new C1());
            mock.Prop3.Prop = 7;

            var subject = new ObjectBase(DxSettings.GlobalSettings, mock.Values);

            // act
            //assert
            Assert.True(subject.HasMockedFieldOrProperty<C1>("Prop1"));
            Assert.True(subject.HasMockedFieldOrProperty<object>("Prop1"));
            Assert.False(subject.HasMockedFieldOrProperty<C2>("Prop1"));

            Assert.True(subject.HasMockedFieldOrProperty<C1>("Prop2"));
            Assert.True(subject.HasMockedFieldOrProperty<object>("Prop2"));
            Assert.False(subject.HasMockedFieldOrProperty<C2>("Prop2"));

            Assert.True(subject.HasMockedFieldOrProperty<C1>("Prop3"));
            Assert.False(subject.HasMockedFieldOrProperty<int>("Prop3"));  // int is value type
            Assert.False(subject.HasMockedFieldOrProperty<string>("Prop3")); // string is sealed
        }

        [Test]
        public void GetMockedIndexKeys_Inheritance()
        {
            var mod = Dx.Module();

            mod.Add("Happy Path")
                .Arrange(bag =>
                {
                    bag.key = new C1();
                    dynamic mock = new MockBuilder();
                    mock[4, bag.key] = new C1();

                    bag.subject = new ObjectBase(DxSettings.GlobalSettings, mock.IndexedValues);
                })
                .Act(bag =>
                    (IEnumerable<IEnumerable<MethodArg>>)bag.subject.GetMockedIndexKeys<C1>(new[] { typeof(int), typeof(C1) }))
                .Assert((bag, result) =>
                {
                    Assert.AreEqual(result.Count(), 1);
                    Assert.AreEqual(result.ElementAt(0).Count(), 2);
                    Assert.AreEqual(result.ElementAt(0).ElementAt(0).Arg, 4);
                    Assert.AreEqual(result.ElementAt(0).ElementAt(0).ArgType, typeof(int));
                    Assert.AreEqual(result.ElementAt(0).ElementAt(1).Arg, bag.key);
                    Assert.AreEqual(result.ElementAt(0).ElementAt(1).ArgType, typeof(C1));
                });

            mod.Add("inheritance, parent key type")
                .BasedOn("Happy Path")
                .UseParentArrange(false)
                .Arrange(bag =>
                {
                    bag.key = new C2();
                    dynamic mock = new MockBuilder();
                    mock[4, bag.key] = new C1();

                    bag.subject = new ObjectBase(DxSettings.GlobalSettings, mock.IndexedValues);
                })
                .UseParentAct(true)
                .SkipParentAssert(false);

            mod.Add("inheritance, parent value type")
                .BasedOn("Happy Path")
                .UseParentArrange(false)
                .Arrange(bag =>
                {
                    bag.key = new C1();
                    dynamic mock = new MockBuilder();
                    mock[4, bag.key] = new C2();

                    bag.subject = new ObjectBase(DxSettings.GlobalSettings, mock.IndexedValues);
                })
                .UseParentAct(true)
                .SkipParentAssert(false);

            mod.Add("inheritance, child key type")
                .BasedOn("Happy Path")
                .UseParentArrange(false)
                .Arrange(bag =>
                {
                    bag.key = new object();
                    dynamic mock = new MockBuilder();
                    mock[4, bag.key] = new C1();

                    bag.subject = new ObjectBase(DxSettings.GlobalSettings, mock.IndexedValues);
                })
                .UseParentAct<IEnumerable<IEnumerable<MethodArg>>>(true)
                .Assert((bag, result) => Assert.IsEmpty(result));

            mod.Add("inheritance, child value type")
                .BasedOn("Happy Path")
                .UseParentArrange(false)
                .Arrange(bag =>
                {
                    bag.key = new C1();
                    dynamic mock = new MockBuilder();
                    mock[4, bag.key] = new object();

                    bag.subject = new ObjectBase(DxSettings.GlobalSettings, mock.IndexedValues);
                })
                .UseParentAct<IEnumerable<IEnumerable<MethodArg>>>(true)
                .Assert((bag, result) => Assert.IsEmpty(result));

            mod.Add("null key")
                .BasedOn("Happy Path")
                .UseParentArrange(false)
                .Arrange(bag =>
                {
                    bag.key = null;
                    dynamic mock = new MockBuilder();
                    mock[4, bag.key] = new C1();

                    bag.subject = new ObjectBase(DxSettings.GlobalSettings, mock.IndexedValues);
                })
                .UseParentAct(true)
                .SkipParentAssert(false);

            mod.Add("null val")
                .BasedOn("Happy Path")
                .UseParentArrange(false)
                .Arrange(bag =>
                {
                    bag.key = new C1();
                    dynamic mock = new MockBuilder();
                    mock[4, bag.key] = null;

                    bag.subject = new ObjectBase(DxSettings.GlobalSettings, mock.IndexedValues);
                })
                .UseParentAct(true)
                .SkipParentAssert(false);

            mod.Add("null key is value type")
                .BasedOn("Happy Path")
                .UseParentArrange(false)
                .Arrange(bag =>
                {
                    bag.key = new C1();
                    dynamic mock = new MockBuilder();
                    mock[null, bag.key] = new C1();

                    bag.subject = new ObjectBase(DxSettings.GlobalSettings, mock.IndexedValues);
                })
                .UseParentAct<IEnumerable<IEnumerable<MethodArg>>>(true)
                .Assert((bag, result) => Assert.IsEmpty(result));

            mod.Add("null value is value type")
                .Arrange(bag =>
                {
                    dynamic mock = new MockBuilder();
                    mock[4, new C1()] = null;

                    bag.subject = new ObjectBase(DxSettings.GlobalSettings, mock.IndexedValues);
                })
                .Act(bag =>
                    (IEnumerable<IEnumerable<MethodArg>>)bag.subject.GetMockedIndexKeys<int>(new[] { typeof(int), typeof(C1) }))
                .Assert((bag, result) => Assert.IsEmpty(result));

            Dx.Run(mod);
        }

        [Test]
        public void GetSetProperties()
        {
            // arrange
            var prop1 = new object();
            var prop2 = new object();
            var prop3 = new object();
            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object> { { "abc", prop1 } }));

            // act
            // 
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
            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()), false);

            // act
            // assert
            Assert.AreEqual(null, subject.GetProperty<object>("abc"));
            Assert.AreEqual(0, subject.GetProperty<int>("abc"));
        }

        [Test]
        public void GetPropertyDoesntExistStrict()
        {
            // arrange
            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()), true);

            // act
            // assert
            Assert.Throws(typeof(InvalidOperationException), () => subject.GetProperty<object>("abc"));
        }

        [Test]
        public void GetInvalidPropertyType()
        {
            // arrange
            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object> { { "abc", new object() } }));

            // act
            // assert
            Assert.Throws(typeof(InvalidOperationException), () => subject.GetProperty<string>("abc"));
        }

        #endregion

        #region Indexes

        [Test]
        public void GetSetIndexes()
        {
            // arrange
            var key1 = new MethodArg[] { new MethodArg<object>(new object()), new MethodArg<string>("asdsadoihasoid") };
            var key2 = new MethodArg[] { new MethodArg<int>(4), new MethodArg<List>(new List()) };
            var val1 = new object();
            var val2 = new object();
            var val3 = new object();
            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<IEnumerable<object>, object>(new Dictionary<IEnumerable<object>, object> 
                {
                    { key1.Select(k => k.Arg), val1 } 
                }));

            // act
            Assert.AreEqual(val1, subject.GetIndex<object>(key1));
            subject.SetIndex(key1, val2);
            Assert.AreEqual(val2, subject.GetIndex<object>(key1));
            subject.SetIndex(key2, val3);
            Assert.AreEqual(val3, subject.GetIndex<object>(key2));
        }

        [Test]
        public void GetIndexeDoesntExistNonStrict()
        {
            // arrange
            var subject = new ObjectBase(DxSettings.GlobalSettings);

            // act
            // assert
            Assert.AreEqual(null, subject.GetIndex<object>(new MethodArg[0]));
            Assert.AreEqual(0, subject.GetIndex<int>(new MethodArg[0]));
        }

        [Test]
        public void GetIndexeDoesntExistStrict()
        {
            // arrange
            var subject = new ObjectBase(DxSettings.GlobalSettings, true);

            // act
            // assert
            Assert.Throws(typeof(InvalidOperationException), () => subject.GetIndex<object>(new MethodArg[0]));
        }

        [Test]
        public void GetInvalidIndexeType()
        {
            var key = new[] { new MethodArg<string>("asdsadas") };

            // arrange
            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<IEnumerable<object>, object>(new Dictionary<IEnumerable<object>, object> { { key.Select(k => k.Arg), new object() } }));

            // act
            // assert
            Assert.Throws(typeof(InvalidOperationException), () => subject.GetIndex<string>(key));
        }

        #endregion

        #region Invoke void

        [Test]
        public void Invoke_Void_HasMock()
        {
            // arrange
            var ok = false;
            var arg = new object();
            var method = new MethodMockBuilder(null, new[] 
            { 
                Dx.Method<object>(a =>
                {
                    ok = true;
                    return a == arg;
                }) 
            });

            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }));

            // act
            // assert
            subject.Invoke("abc", new[] { new MethodArg<object>(arg) });
            Assert.IsTrue(ok);
        }

        [Test]
        public void Invoke_Void_NoMock_NonStrict()
        {
            // arrange
            var arg = new object();

            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup() }
            }));

            // act
            // assert
            subject.Invoke("abc", new[] { new MethodArg<object>(arg) });
        }

        [Test]
        public void Invoke_Void_NoMock_Strict()
        {
            

            // arrange
            var arg = new object();

            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup() }
            }), true);

            // act
            // assert
            Assert.Throws(typeof(InvalidOperationException), () =>
                subject.Invoke("abc", new[] { new MethodArg<object>(arg) }));
        }

        [Test]
        public void Invoke_Void_NoValidMock_NonStrict()
        {
            

            // arrange
            var ok = false;
            var arg = new object();
            var method = new MethodMockBuilder(null, new[] 
            { 
                Dx.Method<object>(a =>
                {
                    ok = true;
                    Assert.AreEqual(a, arg);
                    return false;
                }) 
            });

            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }));

            // act
            // assert
            subject.Invoke("abc", new[] { new MethodArg<object>(arg) });
            Assert.IsTrue(ok);
        }

        [Test]
        public void Invoke_Void_NoValidMock_Strict()
        {
            

            // arrange
            var arg = new object();
            var method = new MethodMockBuilder(null, new[] 
            { 
                Dx.Method<object>(a =>
                {
                    Assert.AreEqual(a, arg);
                    return false;
                }) 
            });

            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }), true);

            // act
            // assert
            Assert.Throws(typeof(InvalidOperationException), () =>
                subject.Invoke("abc", new[] { new MethodArg<object>(arg) }));
        }

        #endregion

        #region invoke non void

        [Test]
        public void Invoke_NonVoid_HasMock()
        {
            

            // arrange
            var ok = false;
            var arg = new object();
            var returnVal = new object();
            var method = new MethodMockBuilder(null, new[] 
            { 
                Dx.Method<object>(a =>
                {
                    ok = true;
                    return a == arg;
                }) 
            });
            (method as dynamic).Returns(returnVal);

            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }));

            // act
            // assert
            var output = subject.Invoke<object>("abc", new[] { new MethodArg<object>(arg) });
            Assert.AreEqual(returnVal, output);
            Assert.IsTrue(ok);
        }

        [Test]
        public void Invoke_NonVoid_Null()
        {
            

            // arrange
            var ok = false;
            var arg = new object();
            object returnVal = null;
            var method = new MethodMockBuilder(null, new[] 
            { 
                Dx.Method<object>(a =>
                {
                    ok = true;
                    return a == arg;
                }) 
            });
            (method as dynamic).Returns(returnVal);

            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }));

            // act
            // assert
            var output = subject.Invoke<object>("abc", new[] { new MethodArg<object>( arg) });
            Assert.AreEqual(returnVal, output);
            Assert.IsTrue(ok);
        }

        [Test]
        public void Invoke_NonVoid_ValueType()
        {
            

            // arrange
            var ok = false;
            var arg = new object();
            var returnVal = 555;
            var method = new MethodMockBuilder(null, new[] 
            { 
                Dx.Method<object>(a =>
                {
                    ok = true;
                    return a == arg;
                }) 
            });
            (method as dynamic).Returns(returnVal);

            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }));

            // act
            // assert
            var output = subject.Invoke<int>("abc", new[] { new MethodArg<object>( arg) });
            Assert.AreEqual(returnVal, output);
            Assert.IsTrue(ok);
        }

        [Test]
        public void Invoke_NonVoid_Cast()
        {
            

            // arrange
            var ok = false;
            var arg = new object();
            var returnVal = 555;
            var method = new MethodMockBuilder(null, new[] 
            { 
                Dx.Method<object>(a =>
                {
                    ok = true;
                    return a == arg;
                }) 
            });
            (method as dynamic).Returns(returnVal);

            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }));

            // act
            // assert
            var output = subject.Invoke<object>("abc", new[] { new MethodArg<object>(arg) });
            Assert.AreEqual(returnVal, output);
            Assert.IsTrue(ok);
        }

        [Test]
        public void Invoke_NonVoid_VoidMethod()
        {
            

            // arrange
            var ok = false;
            var arg = new object();
            var method = new MethodMockBuilder(null, new[] 
            { 
                Dx.Method<object>(a =>
                {
                    ok = true;
                    return true;
                }) 
            });

            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }));

            // act
            // assert
            var output = subject.Invoke<object>("abc", new[] { new MethodArg<object>( arg) });
            Assert.IsNull(output);
            Assert.IsTrue(ok);
        }

        [Test]
        public void Invoke_NonVoid_VoidMethod_RefType()
        {
            

            // arrange
            var ok = false;
            var arg = new object();
            var method = new MethodMockBuilder(null, new[] 
            { 
                Dx.Method<object>(a =>
                {
                    ok = true;
                    return true;
                }) 
            });

            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }));

            // act
            // assert
            var output = subject.Invoke<int>("abc", new[] { new MethodArg<object>(arg) });
            Assert.AreEqual(output, 0);
            Assert.IsTrue(ok);
        }

        [Test]
        public void Invoke_NonVoid_NoMock_NonStrict()
        {
            

            // arrange
            var arg = new object();

            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup() }
            }));

            // act
            // assert
            var output = subject.Invoke<object>("abc", new[] { new MethodArg<object>( arg) });
            Assert.IsNull(output);
        }

        [Test]
        public void Invoke_NonVoid_NoMock_Strict()
        {
            

            // arrange
            var arg = new object();

            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup() }
            }), true);

            // act
            // assert
            Assert.Throws(typeof(InvalidOperationException), () =>
                subject.Invoke("abc", new[] { new MethodArg<object>(arg) }));
        }

        [Test]
        public void Invoke_NonVoid_NoValidMock_NonStrict()
        {
            

            // arrange
            var ok = false;
            var arg = new object();
            var method = new MethodMockBuilder(null, new[] 
            { 
                Dx.Method<object>(a =>
                {
                    ok = true;
                    Assert.AreEqual(a, arg);
                    return false;
                }) 
            });

            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }));

            // act
            // assert
            var output = subject.Invoke<object>("abc", new[] { new MethodArg<object>( arg) });
            Assert.IsTrue(ok);
            Assert.IsNull(output);
        }

        [Test]
        public void Invoke_NonVoid_NoValidMock_Strict()
        {
            // arrange
            var arg = new object();
            var method = new MethodMockBuilder(null, new[] 
            { 
                Dx.Method<object>(a =>
                {
                    Assert.AreEqual(a, arg);
                    return false;
                }) 
            });

            var subject = new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                { "abc", new MethodGroup(method) }
            }), true);

            // act
            // assert
            Assert.Throws(typeof(InvalidOperationException), () =>
                subject.Invoke("abc", new[] { new MethodArg<object>(arg) }));
        }

        #endregion
    }
}
