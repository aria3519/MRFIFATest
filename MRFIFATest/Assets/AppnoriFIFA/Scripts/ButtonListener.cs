using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FStudio.MatchEngine
{
    public class ButtonListener : MonoBehaviour
    {

        private bool _nowState = false;

        private float _timer = -1;


        private void Update()
        {
            if (_timer >= 0) _timer += Time.time;
        }


        public void OnButtonClick()
        {
            if (_nowState
                && _timer > 1f)
            {
                _timer = -1f;
                _nowState = false;
                TransFormManager.Current.OnOffButton();
            }
            else
            {
                _nowState = true;
                _timer = 0;
                TransFormManager.Current.OnOffButton();
            }
        }
    }

}