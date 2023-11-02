using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace FStudio.MatchEngine
{
    public class TransFormBall : MonoBehaviour
    {
        
        public List<GameObject> handPos { set; get; } = new List<GameObject>();

        /*private void Awake()
        {
            // handPos �ڱ� �ڽ� 2�� �־������
            handPos.Add(GameObject.Find("HandL"));
            handPos.Add(GameObject.Find("HandR"));

            TransFormManager.Current.AddBall(this);
        }*/

        private void OnEnable()
        {
            handPos.Add(GameObject.Find("HandL"));
            handPos.Add(GameObject.Find("HandR"));

            TransFormManager.Current.AddBall(this);
        }
        public void TransPostion(Vector3 pos)
        {
            this.transform.position = pos;

        }
        public void TransRotation(Quaternion qu)
        {
            this.transform.rotation = qu;

        }
        public void TransSize(float size)
        {
            this.transform.localScale  = new Vector3(size*2f,size * 2f, size * 2f);

        }



    }

}