using System.Collections.Generic;
using CyanStars.Input;
using UnityEngine;
using UnityEngine.Profiling;

public class InputKeyControl : MonoBehaviour
{
    [SerializeField]private InputProviderSO m_inputProviderSO;
    [SerializeField]private GameObject m_prefabKey;

    private Dictionary<string, GameObject> _keyDict = new Dictionary<string, GameObject>(10);
    
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        Profiler.BeginSample("CyanStars.Input.Keyboard.Iter");
#endif
        foreach (var data in m_inputProviderSO.Inputs)
        {
            if (data.State == InputState.Down && !_keyDict.ContainsKey(data.Tag))
            {
                var key = Instantiate(m_prefabKey);
                key.name = data.Tag;
                key.transform.position = new Vector3(28 * data.Pos - 14, 0.02f, -50f);
                _keyDict.Add(data.Tag, key);
            }
            else if (data.State == InputState.Up && _keyDict.TryGetValue(data.Tag, out var key))
            {
                _keyDict.Remove(data.Tag);
                Destroy(key);
            }
        }
#if UNITY_EDITOR
        Profiler.EndSample();
#endif
    }

    
}

