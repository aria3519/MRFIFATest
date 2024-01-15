using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FStudio.MatchEngine.Events;
using FStudio.MatchEngine.Tactics;
using FStudio.Events;

namespace FStudio.MatchEngine
{
    public class TackleButton : ButtonListener
    {

        private bool _nowState = false;

        private float _timer = -1;

        [SerializeField] private Tactics.TacticPresetTypes num;

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
                
            }
            else
            {
                _nowState = true;
                _timer = 0;

                if (MatchManager.Current.UserTeam.Team != null)
                {
                    MatchManager.Current.UserTeam.Team.TacticPresetType = num;
                    EventManager.Trigger(new TeamChangedTactic(MatchManager.Current.UserTeam, num));
                }

            }
        }
    }

}