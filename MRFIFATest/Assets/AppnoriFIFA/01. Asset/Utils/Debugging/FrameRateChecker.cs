 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jisu.Utils
{
    public class FrameRateChecker : MonoBehaviour
    {
        [Range(0, 2000)]
        public float _posX = 30f;
        [Range(0, 1000)]
        public float _posY = 30f;

        public Color _textColor = Color.yellow;
        [Range(16, 80)]
        public int _textSize = 40;
        [Range(5, 500)]
        public int _frameCheckLength = 100;
        public bool _showGUI = true;
        public bool _showTextMesh = false;
        public TextMesh text;

        private float[] _arrFPS;
        private int _counter = 0;

        private float _curFPS;
        private float _avgFPS;
        private float _maxFPS;
        private float _minFPS;

        private void OnEnable()
        {
            Debug.Log("Frame Rate Checker Running");
            _arrFPS = new float[_frameCheckLength];
            _maxFPS = -9999f;
            _minFPS = 9999f;
            _curFPS = 0f;
            _counter = 0;
        }

        private void Update()
        {
            // Trace Array Length
            if (_arrFPS.Length != _frameCheckLength) _arrFPS = new float[_frameCheckLength];
            if (_counter >= _frameCheckLength) _counter = 0;

            // Set FPS
            _curFPS = 1 / Time.deltaTime;
            _arrFPS[_counter] = _curFPS;

            float sum = 0;
            _maxFPS = -9999;
            _minFPS = 9999;
            foreach (var fps in _arrFPS)
            {
                // Min Max
                if (fps > _maxFPS) _maxFPS = fps;
                if (fps < _minFPS) _minFPS = fps;

                // Average
                sum += fps;
            }
            _avgFPS = sum / _arrFPS.Length;

            // Add Counter
            _counter++;

            if (_showTextMesh && text != null)
            {
                text.text = $"Current : {_curFPS,9: 000.00}\n" +
                    $"Average : {_avgFPS,8: 000.00}\n" +
                    $"Max : {_maxFPS,15: 000.00}\n" +
                    $"Min : {_minFPS,16: 000.00}";
            }
        }

        private void OnGUI()
        {
            if (!_showGUI) return;

            GUIStyle labelStyle = GUI.skin.label;
            labelStyle.normal.textColor = _textColor;
            labelStyle.fontSize = _textSize;

            GUILayout.BeginArea(new Rect(_posX, _posY, 1000, 500));
            GUILayout.Label($"Current : {_curFPS,9: 000.00}", labelStyle);
            GUILayout.Label($"Average : {_avgFPS,8: 000.00}", labelStyle);
            GUILayout.Label($"Max : {_maxFPS,15: 000.00}", labelStyle);
            GUILayout.Label($"Min : {_minFPS,16: 000.00}", labelStyle);
            GUILayout.EndArea();
        }
    }
}