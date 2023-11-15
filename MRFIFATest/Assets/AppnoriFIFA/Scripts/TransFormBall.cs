using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

using System.Linq;
namespace FStudio.MatchEngine
{
    public class TransFormBall : MonoBehaviour
    {
        
        public List<GameObject> handPos { set; get; } = new List<GameObject>();

        public GameObject _shadow;
        public float _A = 300f;

        public bool IsholdGK { get; private set; } = false;


        private List<Vector3> _beforePos = new List<Vector3>();
        public Vector3 testVec;
        private void Awake()
        {
            // handPos 자기 자식 2개 넣어줘야함
            handPos.Add(GameObject.Find("HandL"));
            handPos.Add(GameObject.Find("HandR"));

             

            StartCoroutine(TransFormManager.Current.AddBall(this));
        }
        public void SetHoldGk(bool ishold)
        {
            IsholdGK = ishold;
            if(ishold)
                transform.Rotate(new Vector3(0, 0, 0));
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

            _beforePos.Add(pos);
            if (_beforePos.Count >= 2
                && !IsholdGK)
            {
                var frist = _beforePos.First();
                testVec = Vector3.Lerp(pos, frist, 0.5f) * _A;
                _beforePos.Remove(frist);

                transform.Rotate(testVec);
            }
            
           

        }
       /* public void TransPostionShadow(Vector3 pos)
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


        }*/
        public void TransRotation(Quaternion qu)
        {
            this.transform.rotation = qu;

        }
        public void TransSize(float size)
        {
            this.transform.localScale  = new Vector3(size*4f,size * 4f, size * 4f);

        }



    }

}