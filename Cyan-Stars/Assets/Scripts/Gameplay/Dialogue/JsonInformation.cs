using UnityEngine;
using UnityEngine.SceneManagement;

public class JsonInformation : MonoBehaviour
{
    public string jsonDataFilePath;

    private void Start()
    {
        DontDestroyOnLoad(this);
        jsonDataFilePath = "/测试.json";
        SceneManager.LoadScene("BundleRes/Scenes/Dialogue");
    }

    public string GetJsonDataFilePath()
    {
        return Application.streamingAssetsPath + jsonDataFilePath;
    }
}
