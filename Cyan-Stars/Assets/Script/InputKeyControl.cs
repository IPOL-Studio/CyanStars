using System.Collections.Generic;
using CyanStars.Input;
using UnityEngine;

public class InputKeyControl : MonoBehaviour
{
    [SerializeField]private InputProviderSO _inputProviderSo;

    private Dictionary<string, GameObject> _keyDict = new Dictionary<string, GameObject>(10);

    public GameObject PerfabKey;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var (keyPos, state, keyTag) in _inputProviderSo.GetInputEnumerator())
        {
            if (state == InputState.Down && !_keyDict.ContainsKey(keyTag))
            {
                var key = Instantiate(PerfabKey);
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

