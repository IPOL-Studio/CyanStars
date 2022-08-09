using UnityEngine;
using UnityEngine.SceneManagement;

public class JsonInformation : MonoBehaviour
{
    public string jsonDataFilePath;

    private void Start()
    {
        DontDestroyOnLoad(this);
        jsonDataFilePath = "/新格式.json";
        SceneManager.LoadScene("BundleRes/Scenes/Dialogue");
    }

    public string GetJsonDataFilePath()
    {
        return Application.streamingAssetsPath + jsonDataFilePath;
    }
}
