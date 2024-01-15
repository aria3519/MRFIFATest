using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FStudio.MatchEngine
{
    public class StopButton : MonoBehaviour
    {


        private bool Check = false;
        [SerializeField] private float _timer = 0;
        [SerializeField] private List<Sprite> Icons = new List<Sprite>();
        [SerializeField] private List<Image> Images = new List<Image>();

        private void Start()
        {
            _timer = 0;
        }


        private void Update()
        {
            //if (_timer >= 0) _timer += Time.deltaTime;
            if (_timer >= 0 ) _timer += Time.unscaledDeltaTime;
            CheckControll();
        }

        

        private void CheckControll()
        {
            if (_timer < 1f)
                return;
            //Debug.LogError(" ControllerManager.Current._XRcontros[0].transform.position:" + ControllerManager.Current._XRcontros[0].transform.position);
            //Debug.LogError(" ControllerManager.Current._XRcontros[1].transform.position:" + ControllerManager.Current._XRcontros[1].transform.position);
            //Debug.LogError(" transform.position:" + transform.position);

            if ((transform.position.x - ControllerManager.Current._XRcontros[0].transform.position.x < 0.1f
                 && transform.position.x - ControllerManager.Current._XRcontros[0].transform.position.x > -0.1f)
                 &&
                 (transform.position.y - ControllerManager.Current._XRcontros[0].transform.position.y < 0.1f
                 && transform.position.y - ControllerManager.Current._XRcontros[0].transform.position.y > -0.1f)
                 &&
                 (transform.position.z - ControllerManager.Current._XRcontros[0].transform.position.z < 0.1f
                 && transform.position.z - ControllerManager.Current._XRcontros[0].transform.position.z > -0.1f)
                 ||
                 (transform.position.x - ControllerManager.Current._XRcontros[1].transform.position.x < 0.1f
                 && transform.position.x - ControllerManager.Current._XRcontros[1].transform.position.x > -0.1f)
                 &&
                 (transform.position.y - ControllerManager.Current._XRcontros[1].transform.position.y < 0.1f
                 && transform.position.y - ControllerManager.Current._XRcontros[1].transform.position.y > -0.1f)
                 &&
                 (transform.position.z - ControllerManager.Current._XRcontros[1].transform.position.z < 0.1f
                 && transform.position.z - ControllerManager.Current._XRcontros[1].transform.position.z > -0.1f))
            {
                _timer = 0f;
                OnButtonClick();
            }



        }



        public  void OnButtonClick()
        {
            //Debug.LogError("OnButtonClick");

            if (Check)
                Check = false;
            else
                Check = true;



            foreach (Image s in Images)
            {
                if (Check)
                    s.sprite = Icons[1];
                else
                    s.sprite = Icons[0];

            }
                

            TransFormManager.Current.OnOffButton(Check);


            /*Debug.LogError("OnButtonClick");
            if (_timer > 1f)
            {
                _timer = 0f;
                
                TransFormManager.Current.OnOffButton();
            }
            else
            {
                
                TransFormManager.Current.OnOffButton();
            }*/
        }

       

        


    }

}