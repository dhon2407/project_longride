using System.Collections.Generic;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

public static class SceneController
{
	public delegate void LoadSceneCallback(Scene _scene);
	public static float KLoadingPercentage => kLoadRequiredScenes > 0 ? kLoadedScenes / kLoadRequiredScenes : 1.0f;
	public static bool KDoneLoading => KLoadingPercentage >= 1.0f;

	public static AssetReferenceScene CurrentScene { get; private set; }

	public delegate void OnLoadedSceneDelegate();
	private static event OnLoadedSceneDelegate OnLoadedScene;

	private static int kLoadedScenes;
	private static int kLoadRequiredScenes;
	private static bool kInvoked = false;

	public static void LoadSceneAsync(AssetReferenceScene _scene, LoadSceneMode _mode, LoadSceneCallback _callback = null)
	{
		kInvoked = false;
		kLoadedScenes = 0;
		kLoadRequiredScenes = 1;
		//Logger.LogInfo("Loading Scene by Asset Reference: " + _scene.AssetGUID);

		LoadSceneAsync(_scene.AssetGUID, _mode, _callback);
    }
    public static void LoadSceneAsync(string _scenePath, LoadSceneMode _mode, LoadSceneCallback _callback = null)
	{
		kInvoked = false;
		kLoadedScenes = 0;
		kLoadRequiredScenes = 1;
		//Logger.LogInfo("Loading Scene by Resource Path: " + _scenePath);

		Addressables.LoadResourceLocationsAsync(_scenePath, typeof(object)).Completed += (AsyncOperationHandle<IList<IResourceLocation>> _obj) =>
		{
			HandleAssetLoad(_obj, _mode, _callback);
		};
	}

#if UNITY_EDITOR
	public static void LoadSceneAsync(string _scenePath, OpenSceneMode _mode, LoadSceneCallback _callback = null)
	{
		LoadSceneMode runtimeMode = LoadSceneMode.Additive;
		if (_mode == OpenSceneMode.Single)
			runtimeMode = LoadSceneMode.Single;
		LoadSceneAsync(_scenePath, runtimeMode, _callback);
	}

	/// <summary>
	/// [Editor Only]
	/// This will load any sub scene.
	/// </summary>
	/// <param name="_scenePath"></param>
	public static void HandleSubLoading(string _scenePath)
	{
		HandleSubScenes(_scenePath);
	}
#endif

	private static void HandleAssetLoad(AsyncOperationHandle<IList<IResourceLocation>> _obj, LoadSceneMode _mode, LoadSceneCallback _callback)
	{
        //Logger.LogInfo($"Handling Scene: [{_obj.Status}] ({_obj.Result.Count} items) = ( { string.Join(", ", _obj.Result.Select(_e => _e.PrimaryKey).ToArray()) } )");

		if (!_obj.IsValid())
			return;


        IList<IResourceLocation> locations = _obj.Result;

		// Ugh... ready for a Unity bloody mess?
		// We have to ask the Addressables ResourceManager, if it knows the runtime key of this asset.
		// If it knows it, it loads it, if doesn't then too bad.
		// It would be amazing, if `subscene.LoadSceneAsync()`... would just work in the editor too. I don't understand, why Unity likes to make multiple paths for code.
		ResourceManager rm = Addressables.ResourceManager;
		if (rm == null)
			return;

		// Find the IResourceLocation corresponding to the AddressableReference
		foreach (IResourceLocation location in locations)
		{
			// Use the IResourceLocation
			string path = rm.TransformInternalId(location);
			if (!path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) && !path.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase))
				path = "Assets/" + path;
			if (path.LastIndexOf(".unity", StringComparison.OrdinalIgnoreCase) == -1)
				path += ".unity";

#if UNITY_EDITOR
			if (!EditorApplication.isPlaying) // In Editor Mode
			{
				Scene scene = EditorSceneManager.OpenScene(path, (OpenSceneMode)_mode);

				if (_callback != null)
					_callback(scene);

				if (_mode == LoadSceneMode.Single)
					HandleSubScenes(location.PrimaryKey);
				HandleLoadedScene(scene);

				return;
			}
#endif

			// Unity is funny.
			AsyncOperationHandle<UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator> iao = Addressables.InitializeAsync();
			iao.Completed += (_) =>
			{
				// Oh Unity full of surprises.
				AsyncOperationHandle<IList<IResourceLocation>> validateAddress = Addressables.LoadResourceLocationsAsync(location);
				validateAddress.Completed += (_) =>
				{
					if (validateAddress.Status != AsyncOperationStatus.Succeeded)
					{
						Debug.LogError($"Failed to load Addressable Asset: {location.ProviderId}");
						return;
					}

					Addressables.LoadSceneAsync(location, _mode).Completed += (AsyncOperationHandle<SceneInstance> _obj) =>
					{
						SceneInstance instance = _obj.Result;
						if (_callback != null)
							_callback(instance.Scene);

                        if (_mode == LoadSceneMode.Single)
						{
                            CurrentScene = new AssetReferenceScene(location.PrimaryKey);
							HandleSubScenes(location.PrimaryKey);
						}
						HandleLoadedScene(instance.Scene);
					};
				};
			};
		}
	}


	private static void HandleSubScenes(string _sceneGUID)
	{
		// Load Context Scenes
		SceneControlData sceneControl = SceneControlData.GetContext();
		if (sceneControl == null)
		{
			return;
		}

		if (sceneControl.GlobalScenes != null)
		{
			foreach (string globalScene in sceneControl.GlobalScenes)
			{
				kLoadRequiredScenes += 1;

				AsyncOperationHandle<IList<IResourceLocation>> operation = Addressables.LoadResourceLocationsAsync(globalScene);
				operation.Completed += (_) => {
                    LoadSceneAsync(globalScene, LoadSceneMode.Additive, HandleLoadedScene);
                };
			}
		}

        if (!sceneControl.Relationships.TryGetValue(_sceneGUID, out string[] subscenes))
		{
			return;
		}

		foreach (string subscene in subscenes)
		{
			kLoadRequiredScenes += 1;
			
            AsyncOperationHandle<IList<IResourceLocation>> operation = Addressables.LoadResourceLocationsAsync(subscene);
            operation.Completed += _ => {
				LoadSceneAsync(subscene, LoadSceneMode.Additive, HandleLoadedScene);
            };
        }
	}

	private static void HandleLoadedScene(Scene _scene)
	{
		//Logger.LogDebug("Finished Loading Scene:", _scene.name);

		kLoadedScenes += 1;
		if (!kInvoked && KDoneLoading)
		{
			//Logger.LogDebug("Finished Loading all scenes...");
			OnLoadedScene?.Invoke();
			kInvoked = true;
		}
	}

	public static void AddListener(OnLoadedSceneDelegate _callback)
	{
		OnLoadedScene += _callback;
		if (KDoneLoading)
			_callback();
	}
	public static void RemoveListener(OnLoadedSceneDelegate _callback)
	{
		OnLoadedScene -= _callback;
	}
}