using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JDK.Pool
{

	public class UnityPrefabObjectPool : MonoBehaviour, IObjectPool<GameObject>{

		//TODO, override its DestroyObject method
		private ObjectPool<GameObject> InnerPool {get;set;}

		public GameObject Prefab {get;set;}

		public string prefabPath;

		void Awake()
		{
			InnerPool = new ObjectPool<GameObject>(CreateObjectFromPrefab);
		}

		private GameObject CreateObjectFromPrefab()
		{
			var go = GameObject.Instantiate(Prefab) as GameObject;
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

	public static class PrefabPoolHelper
	{
		private static IDictionary<string, string> prefabPathDict;

		private static IDictionary<string, UnityPrefabObjectPool> prefabPoolDict;
		public static GameObject GetPrefabObject(this PoolManager mgr, string key)
		{
			if (prefabPoolDict.ContainsKey(key))
			{
				return prefabPoolDict[key].Get();
			}

			return null;
		}

		public static void RegisterPrefabPool(this PoolManager mgr, string key, UnityPrefabObjectPool p)
		{
			if (prefabPoolDict.ContainsKey(key))
			{
				Logger.Debug("exist key:" + key + ", destroy origin and use new instead");

				var originPool = prefabPoolDict[key];
				GameObject.Destroy(originPool);
			}

			Object.DontDestroyOnLoad(p);
			prefabPoolDict[key] = p;
		}

		public static void RegisterPrefabPool(this PoolManager mgr, string key, string prefabPath)
		{
			GameObject go = new GameObject("PrefabPool-" + key);
			var pool = go.AddComponent<UnityPrefabObjectPool>();

			pool.Prefab = Resources.Load(prefabPath) as GameObject;

			if (pool.Prefab == null)
			{
				Logger.Debug("prefab load failed");
			}

			RegisterPrefabPool(mgr, key, pool);
		}


	}

}