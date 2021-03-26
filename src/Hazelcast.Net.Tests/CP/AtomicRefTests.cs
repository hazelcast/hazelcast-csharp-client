using System;
using System.Threading.Tasks;
using Hazelcast.Protocol;
using Hazelcast.Protocol.Models;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.CP
{
    [TestFixture]
    public class AtomicRefTests: SingleMemberClientRemoteTestBase
    {
        [Test]
        public async Task Name()
        {
            var name = CreateUniqueName();
            await using var aref = await Client.CPSubsystem.GetAtomicRefAsync<string>(name);

            Assert.That(aref.Name, Is.EqualTo(name));
        }

        [Test]
        public async Task Get()
        {
            await using var aref = await Client.CPSubsystem.GetAtomicRefAsync<string>(CreateUniqueName());

            Assert.That(await aref.GetAsync(), Is.Null);
        }

        [Test]
        public async Task SetAndGet()
        {
            await using var aref = await Client.CPSubsystem.GetAtomicRefAsync<string>(CreateUniqueName());
            var value = RandomString();

            await aref.SetAsync(value);
            Assert.That(await aref.GetAsync(), Is.EqualTo(value));
        }

        [Test]
        public async Task SetAndGetNull()
        {
            await using var aref = await Client.CPSubsystem.GetAtomicRefAsync<string>(CreateUniqueName());

            var value = RandomString();
            await aref.SetAsync(value);
            Assert.That(await aref.GetAsync(), Is.EqualTo(value));

            await aref.SetAsync(null);
            Assert.That(await aref.GetAsync(), Is.Null);
        }

        [Test]
        public async Task GetAndSet()
        {
            await using var aref = await Client.CPSubsystem.GetAtomicRefAsync<string>(CreateUniqueName());
            var (value1, value2) = (RandomString(), RandomString());

            await aref.SetAsync(value1);
            Assert.That(await aref.GetAsync(), Is.EqualTo(value1));
            Assert.That(await aref.GetAndSetAsync(value2), Is.EqualTo(value1));
            Assert.That(await aref.GetAsync(), Is.EqualTo(value2));
        }

        [TestCase("1", "22", "333", false)]
        [TestCase("1", "1", "333", true)]
        public async Task CompareAndSet(string initial, string comparand, string value, bool result)
        {
            await using var aref = await Client.CPSubsystem.GetAtomicRefAsync<string>(CreateUniqueName());

            await aref.SetAsync(initial);
            Assert.That(await aref.GetAsync(), Is.EqualTo(initial));
            Assert.That(await aref.CompareAndSetAsync(comparand, value), Is.EqualTo(result));
            Assert.That(await aref.GetAsync(), Is.EqualTo(result ? value : initial));
        }

        [Test]
        public async Task MultipleDestroy()
        {
            await using var aref = await Client.CPSubsystem.GetAtomicRefAsync<string>(CreateUniqueName());

            await aref.DestroyAsync();
            await aref.DestroyAsync();
        }

        [Test]
        public async Task AfterDestroy()
        {
            await using var aref = await Client.CPSubsystem.GetAtomicRefAsync<string>(CreateUniqueName());

            await aref.DestroyAsync();

            var e = await AssertEx.ThrowsAsync<RemoteException>(async () => await aref.SetAsync(RandomString()));
            Assert.That(e.Error, Is.EqualTo(RemoteError.DistributedObjectDestroyed));
        }

        private string RandomString() => Guid.NewGuid().ToString("N");
    }
}