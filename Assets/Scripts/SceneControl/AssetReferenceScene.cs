using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

[System.Serializable]
public class AssetReferenceScene : AssetReference
{
	[SerializeField] private string m_path;
	public string Path => m_path;
	public bool IsValidAsset => !string.IsNullOrEmpty(Path) || (!string.IsNullOrEmpty(m_AssetGUID) && !m_AssetGUID.Equals("[]"));

	/// <summary>
	/// Construct a new AssetReference object.
	/// </summary>
	/// <param name="_guid">The guid of the asset.</param>
	public AssetReferenceScene(string _guid) : base(_guid)
	{
	}

	public void PostLoad(SceneInstance _instance)
	{
		m_path = _instance.Scene.path;
	}

	/// <inheritdoc/>
	public override bool ValidateAsset(Object obj)
	{
#if UNITY_EDITOR
		System.Type type = obj.GetType();
		return typeof(UnityEditor.SceneAsset).IsAssignableFrom(type);
#else
        return true;
#endif

	}

	/// <inheritdoc/>
	public override bool ValidateAsset(string path)
	{
		m_path = path;
#if UNITY_EDITOR
		System.Type type = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(path);
		return typeof(UnityEditor.SceneAsset).IsAssignableFrom(type);
#else
        return true;
#endif
	}

#if UNITY_EDITOR
	/// <summary>
	/// Type-specific override of parent editorAsset.  Used by the editor to represent the asset referenced.
	/// </summary>
	public new UnityEditor.SceneAsset editorAsset => (UnityEditor.SceneAsset)base.editorAsset;
#endif
}
