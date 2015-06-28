using System;
using System.Collections;
using System.Collections.Generic;

namespace JDK.Pool
{
	public interface IObjectPool
	{
		Object Get();
		bool Return(Object o);
		
		void Shutdown();
		
		void PreWarm(int warmCount);
		
	}
	
	public interface IObjectPool<T> : IObjectPool where T : class
	{
		new T Get();
		bool Return(T o);
	}
	
	public class ObjectPool<T> : IObjectPool<T> where T : class
	{
		
		public static ObjectPool<T> Create(Type t)
		{
			var genericType = typeof(ObjectPool<>);
			var poolType = genericType.MakeGenericType(new Type[]{t});
			
			return Activator.CreateInstance(poolType) as ObjectPool<T>;
		}
		
		private Func<T> CreateObjectFunc;
		private Queue<T> objectQueue;
		
		public ObjectPool()
		{
			
		}
		
		public ObjectPool(Func<T> crtFunc)
		{
			objectQueue = new Queue<T>();
			CreateObjectFunc = crtFunc;
		}
		
		public T Get()
		{
			if (objectQueue.Count > 0)
			{
				return objectQueue.Dequeue();
			}
			else
			{
				return CreateObject();
			}
		}

		Object IObjectPool.Get()
		{
			return null;
		}
		
		private T CreateObject()
		{
			if (CreateObjectFunc!= null)
			{
				return CreateObjectFunc();
			}
			
			return Activator.CreateInstance<T>();
		}
		
		public bool Return(Object o)
		{
			objectQueue.Enqueue(o as T);

			return true;
		}
		
		public bool Return(T o)
		{
			objectQueue.Enqueue(o);

			return true;
		}

		public void Shutdown()
		{
			throw new NotImplementedException();
		}
		
		public void PreWarm(int warmCount)
		{
			throw new NotImplementedException();
		}
		
	}
	
	public class PoolManager
	{
		private IDictionary<Type, IObjectPool> poolDict;
		
		private PoolManager(){
			
			poolDict = new Dictionary<Type, IObjectPool>();
			
		}
		
		private static PoolManager instance;
		public static PoolManager Instance{
			get {
				
				if (instance == null)
				{
					instance = new PoolManager();
				}
				
				return instance;
				
			}
		}
		
		public T GetObject<T>() where T : class
		{
			var t = typeof(T);
			
			IObjectPool p = null;
			if (!poolDict.ContainsKey(t))
			{
				p = CreateCommonPool<T>();
				RegisterPool(t, p);
			}
			
			p = poolDict[t];
			return p.Get() as T;
		}
		
		public Object GetObject(Type t)
		{
			
			IObjectPool p = null;
			if (!poolDict.ContainsKey(t))
			{
				p = CreateCommonPool(t);
				
				RegisterPool(t, p);
			}
			
			p = poolDict[t];
			return p.Get();
		}
		
		public bool ReturnObject(Object o)
		{
			var t = o.GetType();
			
			var p = GetPool(t);
			
			if (p != null)
			{
				return p.Return(o);
			}
			
			return false;
		}
		
		public IObjectPool GetPool(Type t)
		{
			if (!poolDict.ContainsKey(t))
			{
				return null;
			}
			
			return poolDict[t];
		}
		
		public void RegisterPool(Type t, IObjectPool pool)
		{
			if (GetPool(t) != null)
			{
				Logger.Warning("pool for t:" + t.Name + " exist, now replace it with " + pool.GetType().Name);
			}
			
			poolDict[t] = pool;
		}
		
		private IObjectPool CreateCommonPool(Type t)
		{
			if (t.IsValueType)
			{
				throw new ArgumentException("Generic ObjetPool cannot create a ValueType Version");
			}
			
			var genericPoolType = typeof(IObjectPool<>);
			var typeArgs = new Type[] { t };
			var poolType = genericPoolType.MakeGenericType(typeArgs);
			
			return Activator.CreateInstance(poolType) as IObjectPool;
		}
		
		private IObjectPool CreateCommonPool<T>() where T : class
		{
			return new ObjectPool<T>();
		}
		
		public void ShutdownAllPoolsAndClean()
		{
			foreach (IObjectPool p in poolDict.Values)
			{
				//shutdown
				
			}
			
			poolDict.Clear();
		}
		
	}
	
}