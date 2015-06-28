using System.Collections;
using NUnit.Framework;

using JDK.Pool;

public class PoolManagerTest {

	internal class MyTestClass
	{

	}

	[Test]
	public void TestGetPool()
	{
		PoolManager.Instance.ShutdownAllPoolsAndClean();

		var t = typeof(MyTestClass);
		var pool = PoolManager.Instance.GetPool(t);

		Assert.IsNotNull(pool);

		PoolManager.Instance.RegisterPool(t, new ObjectPool<MyTestClass>());

		var pool2 = PoolManager.Instance.GetPool(t);
		Assert.NotNull(pool2);


	}

	[Test]
	public void TestGetObject()
	{
		PoolManager.Instance.ShutdownAllPoolsAndClean();

		var o = PoolManager.Instance.GetObject<MyTestClass>();
		Assert.IsTrue(o != null && o.GetType() == typeof(MyTestClass));

		var origin = o;

		PoolManager.Instance.ReturnObject(o);
		var another = PoolManager.Instance.GetObject<MyTestClass>();

		Assert.IsTrue(another == origin);
	}

}
