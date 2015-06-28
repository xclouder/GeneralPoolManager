using System;
using System.Collections;
using System.Collections.Generic;

namespace JDK.Pool
{
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