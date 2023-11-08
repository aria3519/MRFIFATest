using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FStudio.MatchEngine
{
    public class StopButton : ButtonListener
    {

        private bool _nowState = false;

        private float _timer = -1;


        private void Update()
        {
            if (_timer >= 0) _timer += Time.time;
        }


        public override void OnButtonClick()
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