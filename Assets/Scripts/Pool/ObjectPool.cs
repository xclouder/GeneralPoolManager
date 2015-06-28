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

	public abstract class PooledObject : IPoolable, IDisposable{

		internal Action<PooledObject, bool> ReturnToPool { get; set; }

		internal bool Disposed { get; set; }

		internal bool ReleaseResources()
		{
			bool successFlag = true;
			
			try
			{
				OnReleaseResources();
			}
			catch (Exception)
			{
				successFlag = false;
				
			}
			
			return successFlag;
		}

		public void ResetState()
		{

			OnResetState();

		}

		protected virtual void OnResetState()
		{
			
		}

		protected virtual void OnReleaseResources()
		{
			
		}

		#region Returning object to pool - Dispose and Finalizer
		
		private void HandleReAddingToPool(bool reRegisterForFinalization)
		{
			if (!Disposed)
			{
				// If there is any case that the re-adding to the pool failes, release the resources and set the internal Disposed flag to true
				try
				{
					// Notifying the pool that this object is ready for re-adding to the pool.
					ReturnToPool(this, reRegisterForFinalization);
				}
				catch (Exception)
				{
					Disposed = true;
					this.ReleaseResources();
				}
			}
		}
		
		~PooledObject()
		{
			// Resurrecting the object
			HandleReAddingToPool(true);
		}
		
		public void Dispose()
		{
			// Returning to pool
			HandleReAddingToPool(false);
		}
		
		#endregion

	}

	public class PooledObjectWrapper<T> : PooledObject where T : class
	{
		public Action<T> WrapperReleaseResourcesAction { get; set; }
		public Action<T> WrapperResetStateAction { get; set; }
		
		public T InternalResource { get; private set; }
		
		public PooledObjectWrapper(T resource)
		{
			if (resource == null)
			{
				throw new ArgumentException("resource cannot be null");
			}
			
			// Setting the internal resource
			InternalResource = resource;
		}
		
		protected override void OnReleaseResources()
		{
			if (WrapperReleaseResourcesAction != null)
			{
				WrapperReleaseResourcesAction(InternalResource);
			}
		}
		
		protected override void OnResetState()
		{
			if (WrapperResetStateAction != null)
			{
				WrapperResetStateAction(InternalResource);
			}
		}
	}

	public class AutoReturnObjectPool<T> : ObjectPool<T> where T : PooledObject
	{
		protected override T CreateObject ()
		{
			var o = base.CreateObject ();
			o.ReturnToPool = (Action<PooledObject, bool>)ReturnObjectToPoolWithRegisterFlag;

			return o;
		}

		private void ReturnObjectToPoolWithRegisterFlag(PooledObject objectToReturnToPool, bool reRegisterForFinalization)
		{
			bool succ = base.ReturnObjectToPool(objectToReturnToPool as T);

			if (succ && reRegisterForFinalization)
			{
				GC.ReRegisterForFinalize(objectToReturnToPool);
			}

		}
	}

}