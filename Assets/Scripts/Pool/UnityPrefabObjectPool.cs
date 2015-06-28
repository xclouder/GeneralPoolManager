using UnityEngine;
using System.Collections;

namespace JDK.Pool
{

	public class UnityPrefabObjectPool : MonoBehaviour, IObjectPool<GameObject>{

		private ObjectPool<GameObject> InnerPool {get;set;}

		public GameObject prefab;

		void Awake()
		{
			InnerPool = new ObjectPool<GameObject>(CreateObjectFromPrefab);
		}

		private GameObject CreateObjectFromPrefab()
		{
			var go = GameObject.Instantiate(prefab) as GameObject;
			go.SetActive(false);

			return go;
		}

#region

		/// <summary>
		/// Get this instance.
		/// </summary>
		System.Object IObjectPool.Get()
		{
			return Get();
		}
		
		/// <summary>
		/// Return the specified o.
		/// </summary>
		/// <param name="o">O.</param>
		public virtual bool Return(System.Object o)
		{
			return Return(o as GameObject);
		}
		
		/// <summary>
		/// Shutdown this instance.
		/// </summary>
		public virtual void Shutdown()
		{
			InnerPool.Shutdown();
		}
		
		/// <summary>
		/// prewarm the pool
		/// </summary>
		/// <param name="warmCount">Warm count.</param>
		public void PreWarm(int warmCount)
		{
			InnerPool.PreWarm(warmCount);
		}

		public virtual GameObject Get()
		{
			var go = InnerPool.Get();
			go.SetActive(true);

			return go;
		}

		public bool Return(GameObject o)
		{
			o.SetActive(false);

			return InnerPool.Return(o);
		}

#endregion
	}

}