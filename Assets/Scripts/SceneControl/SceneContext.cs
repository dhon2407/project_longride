using UnityEngine;

[CreateAssetMenu(fileName = "SceneContext", menuName = "SceneControl/SceneContext")]
public class SceneContext : ScriptableObject
{
	public AssetReferenceScene[] subScenes;
}
