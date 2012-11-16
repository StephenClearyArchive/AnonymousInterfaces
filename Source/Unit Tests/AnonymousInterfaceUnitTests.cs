using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization;
using System.Dynamic;
using System.Linq.Expressions;
using Castle.DynamicProxy;
using System.Reflection;
using AnonymousInterfaces;

namespace UnitTests
{
    [TestClass]
    public class AnonymousInterfaceUnitTests
    {
        public interface ITestBase
        {
            void A();
        }

        public interface ITest : ITestBase
        {
            int valg { get; }
            int vals { set; }
            int val { get; set; }
            event Action X;
            string this[int index] { get; set; }
            new void A();
            void A(int arg);
            void A(out int arg);
            void A(int arg1, ref int arg2);
            int B();
            void C(params int[] args);
            void D(int arg1 = 3);
            void E<T>(T arg);
        }

        private delegate void TestDelegate1(out int arg);
        private delegate void TestDelegate2(int arg1, ref int arg);

        [TestMethod]
        public void Method_WithNoParameters()
        {
            int observed = 0;
            var instance = Anonymous.Implement<ITest>()
                .Method<Action>(x => x.A, () => { observed = 13; })
                .Create();

            instance.A();

            Assert.AreEqual(13, observed);
        }

        [TestMethod]
        public void Method_WithNormalParameters()
        {
            int observed = 0;
            var instance = Anonymous.Implement<ITest>()
                .Method<Action<int>>(x => x.A, x => { observed = x; })
                .Create();

            instance.A(13);

            Assert.AreEqual(13, observed);
        }

        [TestMethod]
        public void Method_WithOutParameters()
        {
            int retval = 11;
            var instance = Anonymous.Implement<ITest>()
                .Method<TestDelegate1>(x => x.A, (out int x) => { x = retval; })
                .Create();

            int val = 0;
            instance.A(out val);

            Assert.AreEqual(11, val);
        }

        [TestMethod]
        public void Method_WithRefParameters()
        {
            int observedX = 0;
            int observedY = 0;
            int retval = 11;
            var instance = Anonymous.Implement<ITest>()
                .Method<TestDelegate2>(x => x.A, (int x, ref int y) => { observedX = x; observedY = y; y = retval; })
                .Create();

            int valX = 3;
            int valY = 5;
            instance.A(valX, ref valY);

            Assert.AreEqual(3, observedX);
            Assert.AreEqual(5, observedY);
            Assert.AreEqual(11, valY);
        }

        [TestMethod]
        public void Method_WithReturnValue()
        {
            int retval = 11;
            var instance = Anonymous.Implement<ITest>()
                .Method<Func<int>>(x => x.B, () => retval)
                .Create();

            int val = instance.B();

            Assert.AreEqual(11, val);
        }

        [TestMethod]
        public void Method_WithParams()
        {
            int[] observed = null;
            var instance = Anonymous.Implement<ITest>()
                .Method<Action<int[]>>(x => x.C, x => { observed = x; })
                .Create();

            instance.C(3, 5, 7);

            Assert.IsTrue(observed.SequenceEqual(new[] { 3, 5, 7 }));
        }

        [TestMethod]
        public void Method_WithDefaultValues()
        {
            int observed = 0;
            var instance = Anonymous.Implement<ITest>()
                .Method<Action<int>>(x => x.D, x => { observed = x; })
                .Create();

            instance.D();

            Assert.AreEqual(3, observed);

            instance.D(13);

            Assert.AreEqual(13, observed);
        }

        private sealed class GenericMethodTest : ITest
        {
            int ITest.valg
            {
                get { throw new NotImplementedException(); }
            }

            int ITest.vals
            {
                set { throw new NotImplementedException(); }
            }

            int ITest.val
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            event Action ITest.X
            {
                add { throw new NotImplementedException(); }
                remove { throw new NotImplementedException(); }
            }

            string ITest.this[int index]
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            void ITest.A()
            {
                throw new NotImplementedException();
            }

            void ITest.A(int arg)
            {
                throw new NotImplementedException();
            }

            void ITest.A(out int arg)
            {
                throw new NotImplementedException();
            }

            void ITest.A(int arg1, ref int arg2)
            {
                throw new NotImplementedException();
            }

            int ITest.B()
            {
                throw new NotImplementedException();
            }

            void ITest.C(params int[] args)
            {
                throw new NotImplementedException();
            }

            void ITest.D(int arg1)
            {
                throw new NotImplementedException();
            }

            public int observed;

            void ITest.E<T>(T arg)
            {
                if (typeof(T) == typeof(int))
                    observed = (int)(object)arg;
                else
                    throw new NotImplementedException();
            }

            void ITestBase.A()
            {
                throw new NotImplementedException();
            }
        }


        [TestMethod]
        public void GenericMethod_RequiresImplementation()
        {
            var core = new GenericMethodTest();
            var instance = Anonymous.Implement<ITest>(core)
                .Create();

            instance.E(13);

            Assert.AreEqual(13, core.observed);
        }

        [TestMethod]
        public void Method_HiddenFromBaseInterface()
        {
            int observed = 0;
            var instance = Anonymous.Implement<ITest>()
                .Method<Action>(x => ((ITestBase)x).A, () => { observed = 13; })
                .Create() as ITestBase;

            instance.A();

            Assert.AreEqual(13, observed);
        }

        [TestMethod]
        public void PropertyGet_WhichAlsoHasSet()
        {
            int retval = 13;
            var instance = Anonymous.Implement<ITest>()
                .PropertyGet(x => x.val, () => retval)
                .Create();

            int val = instance.val;

            Assert.AreEqual(13, val);
        }

        [TestMethod]
        public void PropertyGet_WhichDoesNotHaveSet()
        {
            int retval = 13;
            var instance = Anonymous.Implement<ITest>()
                .PropertyGet(x => x.valg, () => retval)
                .Create();

            int val = instance.valg;

            Assert.AreEqual(13, val);
        }

        [TestMethod]
        public void PropertySet_WhichAlsoHasGet()
        {
            int observed = 0;
            var instance = Anonymous.Implement<ITest>()
                .PropertySet(x => x.val, x => { observed = x; })
                .Create();

            instance.val = 13;

            Assert.AreEqual(13, observed);
        }

        [TestMethod]
        public void PropertySet_WhichDoesNotHaveGet()
        {
            int observed = 0;
            var instance = Anonymous.Implement<ITest>()
                .PropertySet<int>("vals", x => { observed = x; })
                .Create();

            instance.vals = 13;

            Assert.AreEqual(13, observed);
        }

        [TestMethod]
        public void IndexGet()
        {
            int observedIndex = 0;
            string retval = "test";
            var instance = Anonymous.Implement<ITest>()
                .IndexGet<Func<int, string>>(index => { observedIndex = index; return retval; })
                .Create();

            string val = instance[15];

            Assert.AreEqual(15, observedIndex);
            Assert.AreEqual("test", val);
        }

        [TestMethod]
        public void IndexSet()
        {
            int observedIndex = 0;
            string observedValue = null;
            var instance = Anonymous.Implement<ITest>()
                .IndexSet<Action<int, string>>((index, value) => { observedIndex = index; observedValue = value; })
                .Create();

            instance[13] = "test";

            Assert.AreEqual(13, observedIndex);
            Assert.AreEqual("test", observedValue);
        }

        [TestMethod]
        public void EventSubscribe()
        {
            Action observed = null;
            var instance = Anonymous.Implement<ITest>()
                .EventSubscribe<Action>("X", x => { observed = x; })
                .Create();

            Action value = () => { };
            instance.X += value;

            Assert.AreEqual(value, observed);
        }

        [TestMethod]
        public void EventUnsubscribe()
        {
            Action observed = null;
            var instance = Anonymous.Implement<ITest>()
                .EventUnsubscribe<Action>("X", x => { observed = x; })
                .Create();

            Action value = () => { };
            instance.X -= value;

            Assert.AreEqual(value, observed);
        }
    }
}
