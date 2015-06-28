using System;
using System.Collections;
using System.Collections.Generic;

namespace JDK.Pool
{
	public class PoolManager
	{
		private IDictionary<Type, IObjectPool> poolDict;
		
		private PoolManager(){
			
			poolDict = new Dictionary<Type, IObjectPool>();

		}

//		public Func<IObjectPool<T>> CommonPoolGenerateFunc<T> {get;set;}


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
				var p = CreateCommonPool(t);
				poolDict[t] = p;

				return p;
			}
			
			return poolDict[t];
		}

		public bool ContainsPoolForType(Type t)
		{
			return poolDict.ContainsKey(t);
		}
		
		public void RegisterPool(Type t, IObjectPool pool)
		{
			if (ContainsPoolForType(t))
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
			
			var genericPoolType = typeof(ObjectPool<>);
			var typeArgs = new Type[] { t };
			var poolType = genericPoolType.MakeGenericType(typeArgs);

			return Activator.CreateInstance(poolType) as IObjectPool;
		}

		//TODO: How to do this with Func<T>?
		private IObjectPool CreateCommonPool<T>() where T : class
		{
			return new ObjectPool<T>();
		}
		
		public void ShutdownAllPoolsAndClean()
		{
			foreach (IObjectPool p in poolDict.Values)
			{
				p.Shutdown();
			}
			
			poolDict.Clear();
		}

	}
}