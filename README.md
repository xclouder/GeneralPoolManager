# GeneralPoolManager
一个轻量级对象池实现

示例
```
class MyClass
{}

class MyResetableClass : IPoolable
{
  public void ResetState(){ // reset the state of this object }
}
```

```
void Demo()
{
    var o = PoolManager.Instance.GetObject<MyClass>();
		Assert.IsTrue(o != null && o.GetType() == typeof(MyClass));

		var origin = o;

		PoolManager.Instance.ReturnObject(o);
		var another = PoolManager.Instance.GetObject<MyClass>();

		Assert.IsTrue(another == origin);
}
```
