using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;
using Dynamox.Mocks.Info;
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

            var subject = new ObjectBase(DxSettings.GlobalSettings, mock);

            // act
            //assert
            Assert.True(subject.HasMockedFieldOrProperty<C1>("Prop1"));
            Assert.True(subject.HasMockedFieldOrProperty<object>("Prop1"));
            Assert.False(subject.HasMockedFieldOrProperty<C2>("Prop1"));

            Assert.True(subject.HasMockedFieldOrProperty<C1>("Prop2"));
            Assert.True(subject.HasMockedFieldOrProperty<object>("Prop2"));
            Assert.False(subject.HasMockedFieldOrProperty<C2>("Prop2"));

            Assert.True(subject.HasMockedFieldOrProperty<C1>("Prop3"));
            Assert.True(subject.HasMockedFieldOrProperty<int>("Prop3"));
            Assert.True(subject.HasMockedFieldOrProperty<string>("Prop3")); 
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

                    bag.subject = new ObjectBase(DxSettings.GlobalSettings, mock);
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

                    bag.subject = new ObjectBase(DxSettings.GlobalSettings, mock);
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

                    bag.subject = new ObjectBase(DxSettings.GlobalSettings, mock);
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

                    bag.subject = new ObjectBase(DxSettings.GlobalSettings, mock);
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

                    bag.subject = new ObjectBase(DxSettings.GlobalSettings, mock);
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

                    bag.subject = new ObjectBase(DxSettings.GlobalSettings, mock);
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

                    bag.subject = new ObjectBase(DxSettings.GlobalSettings, mock);
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

                    bag.subject = new ObjectBase(DxSettings.GlobalSettings, mock);
                })
                .UseParentAct<IEnumerable<IEnumerable<MethodArg>>>(true)
                .Assert((bag, result) => Assert.IsEmpty(result));

            mod.Add("null value is value type")
                .Arrange(bag =>
                {
                    dynamic mock = new MockBuilder();
                    mock[4, new C1()] = null;

                    bag.subject = new ObjectBase(DxSettings.GlobalSettings, mock);
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
            var values = new MockBuilder();
            ((dynamic)values).abc = prop1;
            var subject = new ObjectBase(DxSettings.GlobalSettings, values);

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
            var subject = new ObjectBase(DxSettings.GlobalSettings, false);

            // act
            // assert
            Assert.AreEqual(null, subject.GetProperty<object>("abc"));
            Assert.AreEqual(0, subject.GetProperty<int>("abc"));
        }

        [Test]
        public void GetPropertyDoesntExistStrict()
        {
            // arrange
            var subject = new ObjectBase(DxSettings.GlobalSettings, true);

            // act
            // assert
            Assert.Throws(typeof(InvalidMockException), () => subject.GetProperty<object>("abc"));
        }

        [Test]
        public void GetInvalidPropertyType()
        {
            // arrange
            var vaues = new MockBuilder();
            ((dynamic)vaues).abc = new object();
            var subject = new ObjectBase(DxSettings.GlobalSettings, vaues);

            // act
            // assert
            Assert.Throws(typeof(InvalidMockException), () => subject.GetProperty<string>("abc"));
        }

        #endregion

        #region Indexes

        [Test]
        public void GetSetIndexes()
        {
            // arrange
            var key1 = new MethodArg[] { new MethodArg<object>(new object(), string.Empty), new MethodArg<string>("asdsadoihasoid", string.Empty) };
            var key2 = new MethodArg[] { new MethodArg<int>(4, string.Empty), new MethodArg<List>(new List(), string.Empty) };
            var val1 = new object();
            var val2 = new object();
            var val3 = new object();
            var values = new MockBuilder();
            ((dynamic)values)[key1[0].Arg, key1[1].Arg] = val1;
            var subject = new ObjectBase(DxSettings.GlobalSettings, values);

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
            Assert.Throws(typeof(InvalidMockException), () => subject.GetIndex<object>(new MethodArg[0]));
        }

        [Test]
        public void GetInvalidIndexeType()
        {
            var key = new[] { new MethodArg<string>("asdsadas", string.Empty) };

            // arrange
            var values = new MockBuilder();
            ((dynamic)values)[key[0].Arg] = new object();
            var subject = new ObjectBase(DxSettings.GlobalSettings, values);

            // act
            // assert
            Assert.Throws(typeof(InvalidMockException), () => subject.GetIndex<string>(key));
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
                Dx.Args<object>(a =>
                {
                    ok = true;
                    return a == arg;
                }) 
            });
            var values = new MockBuilder();
            ((dynamic)values).abc = new MethodGroup(method);

            var subject = new ObjectBase(DxSettings.GlobalSettings, values);

            // act
            // assert
            subject.Invoke("abc", new[] { new MethodArg<object>(arg, string.Empty) });
            Assert.IsTrue(ok);
        }

        [Test]
        public void Invoke_Void_NoMock_NonStrict()
        {
            // arrange
            var arg = new object();
            var values = new MockBuilder();
            ((dynamic)values).abc = new MethodGroup();

            var subject = new ObjectBase(DxSettings.GlobalSettings, values);

            // act
            // assert
            subject.Invoke("abc", new[] { new MethodArg<object>(arg, string.Empty) });
        }

        [Test]
        public void Invoke_Void_NoMock_Strict()
        {
            

            // arrange
            var arg = new object();
            var values = new MockBuilder();
            ((dynamic)values).abc = new MethodGroup();

            var subject = new ObjectBase(DxSettings.GlobalSettings, values, true);

            // act
            // assert
            Assert.Throws(typeof(InvalidMockException), () =>
                subject.Invoke("abc", new[] { new MethodArg<object>(arg, string.Empty) }));
        }

        [Test]
        public void Invoke_Void_NoValidMock_NonStrict()
        {
            

            // arrange
            var ok = false;
            var arg = new object();
            var method = new MethodMockBuilder(null, new[] 
            { 
                Dx.Args<object>(a =>
                {
                    ok = true;
                    Assert.AreEqual(a, arg);
                    return false;
                }) 
            });

            var values = new MockBuilder();
            ((dynamic)values).abc = new MethodGroup(method);

            var subject = new ObjectBase(DxSettings.GlobalSettings, values);

            // act
            // assert
            subject.Invoke("abc", new[] { new MethodArg<object>(arg, string.Empty) });
            Assert.IsTrue(ok);
        }

        [Test]
        public void Invoke_Void_NoValidMock_Strict()
        {
            

            // arrange
            var arg = new object();
            var method = new MethodMockBuilder(null, new[] 
            { 
                Dx.Args<object>(a =>
                {
                    Assert.AreEqual(a, arg);
                    return false;
                }) 
            });

            var values = new MockBuilder();
            ((dynamic)values).abc = new MethodGroup(method);

            var subject = new ObjectBase(DxSettings.GlobalSettings, values, true);

            // act
            // assert
            Assert.Throws(typeof(InvalidMockException), () =>
                subject.Invoke("abc", new[] { new MethodArg<object>(arg, string.Empty) }));
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
                Dx.Args<object>(a =>
                {
                    ok = true;
                    return a == arg;
                }) 
            });
            (method as dynamic).DxReturns(returnVal);

            var values = new MockBuilder();
            ((dynamic)values).abc = new MethodGroup(method);

            var subject = new ObjectBase(DxSettings.GlobalSettings, values);

            // act
            // assert
            var output = subject.Invoke<object>("abc", new[] { new MethodArg<object>(arg, string.Empty) });
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
                Dx.Args<object>(a =>
                {
                    ok = true;
                    return a == arg;
                }) 
            });
            (method as dynamic).DxReturns(returnVal);

            var values = new MockBuilder();
            ((dynamic)values).abc = new MethodGroup(method);

            var subject = new ObjectBase(DxSettings.GlobalSettings, values);

            // act
            // assert
            var output = subject.Invoke<object>("abc", new[] { new MethodArg<object>(arg, string.Empty) });
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
                Dx.Args<object>(a =>
                {
                    ok = true;
                    return a == arg;
                }) 
            });
            (method as dynamic).DxReturns(returnVal);

            var values = new MockBuilder();
            ((dynamic)values).abc = new MethodGroup(method);

            var subject = new ObjectBase(DxSettings.GlobalSettings, values);

            // act
            // assert
            var output = subject.Invoke<int>("abc", new[] { new MethodArg<object>(arg, string.Empty) });
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
                Dx.Args<object>(a =>
                {
                    ok = true;
                    return a == arg;
                }) 
            });
            (method as dynamic).DxReturns(returnVal);

            var values = new MockBuilder();
            ((dynamic)values).abc = new MethodGroup(method);

            var subject = new ObjectBase(DxSettings.GlobalSettings, values);

            // act
            // assert
            var output = subject.Invoke<object>("abc", new[] { new MethodArg<object>(arg, string.Empty) });
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
                Dx.Args<object>(a =>
                {
                    ok = true;
                    return true;
                }) 
            });

            var values = new MockBuilder();
            ((dynamic)values).abc = new MethodGroup(method);

            var subject = new ObjectBase(DxSettings.GlobalSettings, values);

            // act
            // assert
            var output = subject.Invoke<object>("abc", new[] { new MethodArg<object>(arg, string.Empty) });
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
                Dx.Args<object>(a =>
                {
                    ok = true;
                    return true;
                }) 
            });

            var values = new MockBuilder();
            ((dynamic)values).abc = new MethodGroup(method);

            var subject = new ObjectBase(DxSettings.GlobalSettings, values);

            // act
            // assert
            var output = subject.Invoke<int>("abc", new[] { new MethodArg<object>(arg, string.Empty) });
            Assert.AreEqual(output, 0);
            Assert.IsTrue(ok);
        }

        [Test]
        public void Invoke_NonVoid_NoMock_NonStrict()
        {
            // arrange
            var arg = new object();

            var values = new MockBuilder();
            ((dynamic)values).abc = new MethodGroup();

            var subject = new ObjectBase(DxSettings.GlobalSettings, values);

            // act
            // assert
            var output = subject.Invoke<object>("abc", new[] { new MethodArg<object>(arg, string.Empty) });
            Assert.IsNull(output);
        }

        [Test]
        public void Invoke_NonVoid_NoMock_Strict()
        {
            

            // arrange
            var arg = new object();

            var values = new MockBuilder();
            ((dynamic)values).abc = new MethodGroup();

            var subject = new ObjectBase(DxSettings.GlobalSettings, values, true);

            // act
            // assert
            Assert.Throws(typeof(InvalidMockException), () =>
                subject.Invoke("abc", new[] { new MethodArg<object>(arg, string.Empty) }));
        }

        [Test]
        public void Invoke_NonVoid_NoValidMock_NonStrict()
        {
            

            // arrange
            var ok = false;
            var arg = new object();
            var method = new MethodMockBuilder(null, new[] 
            { 
                Dx.Args<object>(a =>
                {
                    ok = true;
                    Assert.AreEqual(a, arg);
                    return false;
                }) 
            });

            var values = new MockBuilder();
            ((dynamic)values).abc = new MethodGroup(method);

            var subject = new ObjectBase(DxSettings.GlobalSettings, values);

            // act
            // assert
            var output = subject.Invoke<object>("abc", new[] { new MethodArg<object>(arg, string.Empty) });
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
                Dx.Args<object>(a =>
                {
                    Assert.AreEqual(a, arg);
                    return false;
                }) 
            });

            var values = new MockBuilder();
            ((dynamic)values).abc = new MethodGroup(method);

            var subject = new ObjectBase(DxSettings.GlobalSettings, values, true);

            // act
            // assert
            Assert.Throws(typeof(InvalidMockException), () =>
                subject.Invoke("abc", new[] { new MethodArg<object>(arg, string.Empty) }));
        }

        #endregion
    }
}
