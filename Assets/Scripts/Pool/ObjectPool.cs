using System;
using System.Collections;
using System.Collections.Generic;

namespace JDK.Pool
{
	public interface IPoolable
	{
		void ResetState();
	}

	public interface IObjectPool
	{
		/// <summary>
		/// Get this instance.
		/// </summary>
		Object Get();

		/// <summary>
		/// Return the specified o.
		/// </summary>
		/// <param name="o">O.</param>
		bool Return(Object o);

		/// <summary>
		/// Shutdown this instance.
		/// </summary>
		void Shutdown();

		/// <summary>
		/// prewarm the pool
		/// </summary>
		/// <param name="warmCount">Warm count.</param>
		void PreWarm(int warmCount);
	}

	public interface IObjectPool<T> : IObjectPool where T : class
	{
		new T Get();
		bool Return(T o);
	}
	
	public class ObjectPool<T> : IObjectPool<T> where T : class
	{
		private int _maxObjectsToRetain = 100;
		public int MaxObjectsToRetain {
			get {
				return _maxObjectsToRetain;
			}

			set {
				_maxObjectsToRetain = value;
			}
		}

		private Func<T> CreateObjectFunc;
		private Queue<T> objectQueue;
		public IEnumerable<T> PooledObjects { get { return objectQueue;}}

		private void DoInit(Func<T> crtFunc)
		{
			objectQueue = new Queue<T>();
			MaxObjectsToRetain = 100;

			CreateObjectFunc = crtFunc;
		}

		public ObjectPool()
		{
			DoInit(null);
		}
		
		public ObjectPool(Func<T> crtFunc)
		{
			DoInit(crtFunc);
		}
		
		public virtual T Get()
		{
			return GetPooledObject();
		}

		Object IObjectPool.Get()
		{
			return GetPooledObject();
		}

		protected T GetPooledObject()
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

		protected virtual T CreateObject()
		{
			T newObj = null;
			if (CreateObjectFunc!= null)
			{
				newObj = CreateObjectFunc();
			}
			
			newObj = Activator.CreateInstance<T>();

			return newObj;
		}
		
		public virtual bool Return(Object o)
		{
			return Return(o as T);
		}
		
		public virtual bool Return(T o)
		{
			return ReturnObjectToPool(o);
		}

		protected virtual bool ReturnObjectToPool(T o)
		{
			if (objectQueue.Count > MaxObjectsToRetain)
			{
				DestroyObject(o);

				// do not enqueue
				return true;
			}
			else
			{
				bool succ = true;
				try {
					ResetObjectState(o);
				}
				catch (Exception e)
				{
					Logger.Error(e.ToString());

					succ = false;
					DestroyObject(o);
				}

				if (succ)
				{
					objectQueue.Enqueue(o);
				}
				return succ;
			}
		}

		protected virtual void DestroyObject(T o){}

		protected virtual void ResetObjectState(T o){}

		public void Shutdown()
		{
			objectQueue.Clear();
		}
		
		public void PreWarm(int warmCount)
		{
			for (int i = 0; i < warmCount; i++)
			{
				var o = Get ();
				Return(o);
			}
		}
		
	}

	public class ResettableObjectPool<T> : ObjectPool<T> where T : class, IPoolable
	{
		protected override void ResetObjectState(T o){
			o.ResetState();
		}
	}



}