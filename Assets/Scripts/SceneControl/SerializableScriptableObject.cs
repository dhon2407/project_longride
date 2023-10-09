using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SerializableScriptableObject : ScriptableObject
{
	[SerializeField, HideInInspector] private string m_guid;
	public string Guid => m_guid;

#if UNITY_EDITOR
	void OnValidate()
	{
		string path = AssetDatabase.GetAssetPath(this);
		m_guid = AssetDatabase.AssetPathToGUID(path);
	}
#endif
}