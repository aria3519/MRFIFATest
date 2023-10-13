using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace FStudio.MatchEngine
{
    public class TransFormBall : MonoBehaviour
    {

       
        private  void Awake()
        {
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
        


    }

}