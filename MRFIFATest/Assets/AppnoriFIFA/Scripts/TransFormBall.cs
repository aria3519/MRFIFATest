using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace FStudio.MatchEngine
{
    public class TransFormBall : MonoBehaviour
    {
        
        public List<GameObject> handPos { set; get; } = new List<GameObject>();

        public GameObject _shadow;
        public float _A = 300f;

        private void Awake()
        {
            // handPos 자기 자식 2개 넣어줘야함
            handPos.Add(GameObject.Find("HandL"));
            handPos.Add(GameObject.Find("HandR"));

             

            StartCoroutine(TransFormManager.Current.AddBall(this));
        }

        /*private void OnEnable()
        {
            handPos.Add(GameObject.Find("HandL"));
            handPos.Add(GameObject.Find("HandR"));

            TransFormManager.Current.AddBall(this);
        }*/
        public void TransPostion(Vector3 pos)
        {
            this.transform.position = pos;

        }
        public void TransPostionShadow(Vector3 pos)
        {
            if(_shadow!= null)
            {
                _shadow.transform.position = pos;

                float GabY = 1;
                if ((transform.position.y - _shadow.transform.position.y) > 0.01f)
                    GabY = 1/(transform.position.y - _shadow.transform.position.y);
                else
                    GabY = (1 / _A) *0.5f;

                if(GabY >= 0.5f)
                _shadow.transform.localScale = new Vector3(GabY * _A,
                   GabY * _A,
                   GabY * _A
                    ); 

            }


        }
        public void TransRotation(Quaternion qu)
        {
            this.transform.rotation = qu;

        }
        public void TransSize(float size)
        {
            this.transform.localScale  = new Vector3(size*3f,size * 3f, size * 3f);

        }



    }

}