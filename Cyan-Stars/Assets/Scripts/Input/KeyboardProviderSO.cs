using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

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

        private HashSet<KeyCode> _downedKeySet = new HashSet<KeyCode>();
        private Enumerator _enumerator;

        protected override IEnumerable<InputData> GetInputIter()
        {
            _enumerator ??= new Enumerator(this);
            _enumerator.Reset();
            return _enumerator;
        }
        
        private class Enumerator : IEnumerable<InputData>, IEnumerator<InputData>
        {
            private int _index;
            private InputData _current;
            private KeyboardProviderSO _provider;

            public Enumerator(KeyboardProviderSO provider)
            {
                _index = 0;
                _current = default;
                _provider = provider;
            }

            public bool MoveNext()
            {
                var maps = _provider.m_maps;
                while (_index < maps.Length)
                {
                    var key = maps[_index].m_key;
                    if (UnityEngine.Input.GetKeyDown(key))
                    {
                        _provider._downedKeySet.Add(key);
                        _current = new InputData(1f * maps[_index].m_pos / _provider.m_keyBoardLength, InputState.Down, maps[_index].Tag);
                        _index++;
                        return true;
                    }
                    
                    if (_provider._downedKeySet.Contains(key) && !UnityEngine.Input.GetKey(key))
                    {
                        _provider._downedKeySet.Remove(maps[_index].m_key);
                        _current = new InputData(0, InputState.Up, maps[_index].Tag);
                        _index++;
                        return true;
                    }
                    _index++;
                }

                _current = default;
                return false;
            }

            public void Reset()
            {
                _index = 0;
                _current = default;
            }

            public InputData Current => _current;

            object IEnumerator.Current => _current;

            public void Dispose() { }

            public IEnumerator<InputData> GetEnumerator() => this;

            IEnumerator IEnumerable.GetEnumerator() => this;
        }
    }
}
