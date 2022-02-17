using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Input
{
    [CreateAssetMenu(menuName = "Cyan Stars/Input/Keyboard Input Provider", fileName = "KeyboardInputProviderSO")]
    public sealed class KeyboardProviderSO : InputProviderSO
    {
        [Serializable]
        public struct KeyMap
        {
            public int m_pos;
            public KeyCode m_key;
            public string Tag { get; private set; }

            public KeyMap(int pos, KeyCode key)
            {
                m_pos = pos;
                m_key = key;
                Tag = m_key.ToString();
            }
        }
        
        //键盘操作部分长度，单位毫米，一般是按键“Q”左缘到“]”左缘的长度。
        public int m_keyBoardLength = 209;

        //下面用来输入各个按键左缘的位置……肝疼
        [SerializeField] private KeyMap[] m_maps =
        {
            //第一排
            new KeyMap(0, KeyCode.Q),
            new KeyMap(19, KeyCode.W),
            new KeyMap(38, KeyCode.E),
            new KeyMap(57, KeyCode.R),
            new KeyMap(76, KeyCode.T),
            new KeyMap(95, KeyCode.Y),
            new KeyMap(114, KeyCode.U),
            new KeyMap(133, KeyCode.I),
            new KeyMap(152, KeyCode.O),
            new KeyMap(171, KeyCode.P),
            new KeyMap(190, KeyCode.LeftBracket),
            new KeyMap(209, KeyCode.RightBracket),

            //第二排
            new KeyMap(5, KeyCode.A),
            new KeyMap(24, KeyCode.S),
            new KeyMap(43, KeyCode.D),
            new KeyMap(62, KeyCode.F),
            new KeyMap(81, KeyCode.G),
            new KeyMap(100, KeyCode.H),
            new KeyMap(119, KeyCode.J),
            new KeyMap(138, KeyCode.K),
            new KeyMap(157, KeyCode.L),
            new KeyMap(176, KeyCode.Semicolon),
            new KeyMap(195, KeyCode.Quote),

            //第三排
            new KeyMap(15, KeyCode.Z),
            new KeyMap(34, KeyCode.X),
            new KeyMap(53, KeyCode.C),
            new KeyMap(72, KeyCode.V),
            new KeyMap(91, KeyCode.B),
            new KeyMap(110, KeyCode.N),
            new KeyMap(129, KeyCode.M),
            new KeyMap(148, KeyCode.Comma),
            new KeyMap(167, KeyCode.Period),
            new KeyMap(186, KeyCode.Slash),
        };

        private HashSet<KeyCode> _downedKey = new HashSet<KeyCode>();

        public override IEnumerable<(float, InputState, string)> GetInputEnumerator()
        {
            foreach (var map in m_maps)
            {
                if (UnityEngine.Input.GetKeyDown(map.m_key))
                {
                    _downedKey.Add(map.m_key);
                    yield return (1f * map.m_pos / m_keyBoardLength, InputState.Down, map.Tag);
                }
                else if (_downedKey.Contains(map.m_key) && !UnityEngine.Input.GetKey(map.m_key))
                {
                    _downedKey.Remove(map.m_key);
                    yield return (0, InputState.Up, map.Tag);
                }
            }
        }
    }
}
