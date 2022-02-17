using System.Collections.Generic;
using CyanStars.Input;
using UnityEngine;

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
        foreach (var (keyPos, state, keyTag) in m_inputProviderSO.Inputs)
        {
            if (state == InputState.Down && !_keyDict.ContainsKey(keyTag))
            {
                var key = Instantiate(m_prefabKey);
                key.name = keyTag;
                key.transform.position = new Vector3(28 * keyPos - 14, 0.02f, -50f);
                _keyDict.Add(keyTag, key);
            }
            else if (state == InputState.Up && _keyDict.TryGetValue(keyTag, out var key))
            {
                _keyDict.Remove(keyTag);
                Destroy(key);
            }
        }
    }

    
}

