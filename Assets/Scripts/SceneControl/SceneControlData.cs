using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Linq;
using System.Collections;
using UnityEngine.AddressableAssets;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor;
#endif

[System.Serializable]
public struct SceneContextRelationship
{
	public AssetReferenceScene scene;
	public AssetReferenceScene[] subscenes;
}

public class SceneControlData : ScriptableObject
{
#if UNITY_EDITOR
	[MenuItem("Tools/SceneControl/Settings")]
	private static void Menu()
	{
		SceneControlData self = GetContext();
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = self;
	}
#endif

    [SerializeField] private AssetReferenceScene[] m_globalScenes;
    [SerializeField] private SceneContextRelationship[] m_relationships;

	public string[] GlobalScenes => m_globalScenes.Select(_e => _e.AssetGUID).ToArray();

	private Dictionary<string, string[]> m_relationshipsCached;
	public int LoadedRelationships { get; private set; }
	public Dictionary<string, string[]> Relationships
	{
		get
		{
			if (m_relationshipsCached is not null)
			{
				return m_relationshipsCached;
			}

			m_relationshipsCached = new Dictionary<string, string[]>();

			if (m_relationships is null)
			{
				return m_relationshipsCached;
			}

			LoadedRelationships = 0;
			foreach (SceneContextRelationship relationship in m_relationships)
			{
				string key = GetAddressableEditorPath(relationship.scene.AssetGUID);

                m_relationshipsCached.Add(key, relationship.subscenes.Select(_element => GetAddressableEditorPath(_element.AssetGUID)).ToArray());
            }

			return m_relationshipsCached;
        }
    }
    public IEnumerator Load()
    {

		// Load global scenes first.
        foreach (AssetReferenceScene subscene in m_globalScenes)
        {
            AsyncOperationHandle<IList<IResourceLocation>> subOperation = Addressables.LoadResourceLocationsAsync(subscene.AssetGUID);

            while (!subOperation.IsDone)
                yield return null; // Wait, we are still loading..
        }

		// Load scene hierarchies.
        foreach (SceneContextRelationship relationship in m_relationships)
		{
			AsyncOperationHandle<IList<IResourceLocation>> operation = Addressables.LoadResourceLocationsAsync(relationship.scene.AssetGUID);

			while (!operation.IsDone)
				yield return null; // Wait, we are still loading..
			

            foreach (AssetReferenceScene subscene in relationship.subscenes)
            {
                AsyncOperationHandle<IList<IResourceLocation>> subOperation = Addressables.LoadResourceLocationsAsync(subscene.AssetGUID);

                while (!subOperation.IsDone)
                    yield return null; // Wait, we are still loading..
            }

            // We are done loading!
            LoadedRelationships++;
        }
    }

	private string GetAddressableEditorPath(string _assetGUID)
	{
#if UNITY_EDITOR
        if (!AddressableAssetSettingsDefaultObject.SettingsExists)
		{
			Debug.LogError("Addressables Default Settings does not exist, please create that via Window > Addressables > Groups");
			return _assetGUID;
		}

        if (AddressableAssetSettingsDefaultObject.Settings == null)
		{
            Debug.LogWarning($"Addressable Settings is null.\nisUpdating: {EditorApplication.isUpdating}   isCompiling: {EditorApplication.isUpdating}  settingsExist: {UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.SettingsExists}");
			AddressableAssetSettingsDefaultObject.Settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(AddressableAssetSettingsDefaultObject.DefaultAssetPath);
        }

        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
		if (settings == null)
        {
            // https://forum.unity.com/threads/addressableassetsettingsdefaultobject-settings-sometimes-null.964103/
            Debug.LogError("Attempted to load Addressable Asset Settings, it exists, but it failed. This is a Unity Bug.");
            return _assetGUID;
        }

        List<AddressableAssetEntry> allEntries = new List<AddressableAssetEntry>(settings.groups.SelectMany(_entry => _entry?.entries ?? new AddressableAssetEntry[0]));
        
		// Search immediate groups' entries.
		AddressableAssetEntry foundEntry = allEntries.FirstOrDefault(_e => _e.guid == _assetGUID || _e.address == _assetGUID);
		if (foundEntry != null)
			return foundEntry.AssetPath;
		// Search Sub Asset's Entries.
        /*foreach (AddressableAssetEntry entry in allEntries)
		{
			if (!entry.IsFolder) // Not possible to look through the sub items in a folder, thanks unity.
				continue;


			if (entry.SubAssets == null)
				continue;
			foundEntry = entry.SubAssets.FirstOrDefault(_se => _se.guid == _assetGUID || _se.address == _assetGUID);
			if (foundEntry != null)
				return foundEntry.AssetPath;
		}*/

        return null;
#else
		return _assetGUID;
#endif
    }

    /*
    AsyncOperationHandle<IList<IResourceLocation>> operation = Addressables.LoadResourceLocationsAsync(relationship.scene.AssetGUID);

    operation.Completed += (_) => { 
        string result = LoadedSceneData(operation);
        if (result != null)
        {
            string[] subscenes = m_relationshipsCached[relationship.scene.AssetGUID];
        }
        LoadedRelationships++;
    };
    */

    private string LoadedSceneData(AsyncOperationHandle<IList<IResourceLocation>> _operation)
    {
        if (_operation.Status == AsyncOperationStatus.Failed)
        {
            return null;
        }
        IResourceLocation location = _operation.Result.First();
		return location.PrimaryKey;
    }

    public static SceneControlData GetContext()
	{
		SceneControlData instance = null;

		string scenecontrolPath = $"{Application.dataPath}/Resources/scenecontrol";
#if !UNITY_EDITOR // In the Editor always reload this, so we can get the latest information on this data file.
        if (instance == null)
#endif
        {
			instance = Resources.Load<SceneControlData>("scenecontrol/data");
			if (instance == null && File.Exists(scenecontrolPath + "/data.asset"))
			{
				Debug.LogError($"Failed to load SceneControl Path: [{Directory.CreateDirectory(scenecontrolPath).GetDirectories().Length}] `{scenecontrolPath}`");

				instance = ScriptableObject.CreateInstance<SceneControlData>();
				return instance; // Don't Attempt to load something and save it off, because it crashed.
			}
		}

		if (instance == null)
		{
#if UNITY_EDITOR
			instance = ScriptableObject.CreateInstance<SceneControlData>();

			if (!Directory.Exists(scenecontrolPath))
			{
				Debug.Log($"Created SceneControl Path: [{Directory.CreateDirectory(scenecontrolPath).GetDirectories().Length}] `{scenecontrolPath}`");
			}

			AssetDatabase.CreateAsset(instance, "Assets/Resources/scenecontrol/data.asset");
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
#else
			Logger.LogError("Build Error, Scene Control does not have the correct data to actually do it's job.");
			//throw new System.Exception("Scene Control does not have it's data asset!");
#endif
		}

#if DEBUG
		/*Logger.LogDebug("SceneControl - Loading Context from memory.");
		foreach (KeyValuePair<string, SceneContextRelationship> relationship in instance.Relationships)
		{
			Logger.LogDebug($"\t{relationship.Value.scene.AssetGUID}");
			foreach (SceneContext context in relationship.Value.contexts)
			{
				Logger.LogDebug($"\t\t{context.name}");
				foreach (AssetReferenceScene subscene in context.subScenes)
				{
					Logger.LogDebug($"\t\t\t{subscene.AssetGUID}");
				}
			}
		}*/
#endif

		_ = instance.Relationships; // Init relationships.

		return instance;
	}

}