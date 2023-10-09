using System.Collections;
using UnityEngine;

public class InitializationScript : MonoBehaviour
{
    [SerializeField] private AssetReferenceScene m_initialScene;

    void Start()
    {
        // Immediately attempt to load this scene!
        if (m_initialScene == null)
        {
			Debug.LogError("Initial Scene not setup correctly!");
            return;
        }
		
        StartCoroutine(WaitUntilDoneLoading());
	}

	private IEnumerator WaitUntilDoneLoading()
	{
        SceneControlData data = SceneControlData.GetContext();
        
        if (data == null)
        {
            Debug.LogError("No Data on SceneControlData!");
            yield break;
        }

        // Wait until we fully load.
        yield return data.Load();

        SceneController.LoadSceneAsync(m_initialScene, UnityEngine.SceneManagement.LoadSceneMode.Single);
	}
}
