using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AsyncTests1;
using NuGet.Versioning;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

// this allows overriding whatever server version we're using
[assembly:HazelcastServerVersion("4.1")]

namespace AsyncTests1
{
    public static class ConditionAssert
    {
        private static bool GetCondition()
        {
            var testContext = TestContext.CurrentContext;
            var test = testContext.Test;
            var className = test.ClassName;
            var classType = Type.GetType(className);
            if (classType == null)
                throw new InvalidOperationException("no class");
            var methodName = test.MethodName;
            var methodInfos = classType.GetMethods().Where(x => x.Name == methodName); // overloads?
            MethodInfo methodInfo = null;
            foreach (var mi in methodInfos)
            {
                if (methodInfo == null)
                    methodInfo = mi;
                else
                    throw new NotSupportedException("overload");
            }
            if (methodInfo == null)
                throw new InvalidOperationException("no method");

            //var attribute = methodInfo.GetCustomAttribute<ConditionAttribute>();
            //if (attribute == null)
            //    throw new InvalidOperationException("no attribute");
            //return attribute.Condition;

            return false;
        }

        public static void IfCondition(TestDelegate code)
            => IfCondition(GetCondition(), code);

        public static void IfCondition(bool condition, TestDelegate code)
        {
            if (!condition)
                Assert.Throws<NotImplementedException>(code);
            else
                code();
        }
    }

    public class Assert : NUnit.Framework.Assert
    {
        private static bool GetCondition()
        {
            var testContext = TestContext.CurrentContext;
            var test = testContext.Test;
            var className = test.ClassName;
            var classType = Type.GetType(className);
            if (classType == null)
                throw new InvalidOperationException("no class");
            var methodName = test.MethodName;
            var methodInfos = classType.GetMethods().Where(x => x.Name == methodName); // overloads?
            MethodInfo methodInfo = null;
            foreach (var mi in methodInfos)
            {
                if (methodInfo == null)
                    methodInfo = mi;
                else
                    throw new NotSupportedException("overload");
            }
            if (methodInfo == null)
                throw new InvalidOperationException("no method");

            //var attribute = methodInfo.GetCustomAttribute<ConditionAttribute>();
            //if (attribute == null)
            //    throw new InvalidOperationException("no attribute");
            //return attribute.Condition;
            return false;
        }

        public static void IfCondition(TestDelegate code)
            => IfCondition(GetCondition(), code);

        public static void IfCondition(bool condition, TestDelegate code)
        {
            if (!condition)
                Assert.Throws<NotImplementedException>(code);
            else
                code();
        }
    }

    // - AssertHz is painful
    // - create our own Assert?
    // - get client version?

    [TestFixture]
    public class ConditionalTests
    {
        [Test]
        [ServerCondition("[4.1]")]
        public void CanDoSomethingOn41()
        {
            // this executes only if the server condition is met
            DoSomething();
        }

        [Test]
        [ServerCondition("[4.0]")]
        public void CannotDoSomethingOn40()
        {
            NUnit.Framework.Assert.Throws<NotImplementedException>(DoSomething);
        }

        [Test]
        public void Mixed()
        {
            ServerCondition.InRange("[4.1]", DoSomething);
            ServerCondition.InRange("[4.0]", () =>
            {
                NUnit.Framework.Assert.Throws<NotImplementedException>(DoSomething);
            });

            ServerCondition.InRange("[4.1]",
                DoSomething,
                () =>
                {
                    NUnit.Framework.Assert.Throws<NotImplementedException>(DoSomething);
                });
        }

        // does nothing for 4.1
        // throws for 4.0
        private static void DoSomething()
        {
            if (HazelcastServerVersionAttribute.ServerVersion < NuGetVersion.Parse("4.1"))
                throw new NotImplementedException();
        }
    }
}
