using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace FStudio.MatchEngine
{

 

    

    public class TransFormMap : MonoBehaviour
    {

        private Transform _myTrans;

        

        /*private async void Start()
        {
            await Task.Run(() =>
            {
                TransFormManager.Current.AddMap(this);
                //_myTrans = GetComponent<Transform>();
            });
        }*/


        private void Awake()
        {
            TransFormManager.Current.AddMap(this);
        }

        public void  ChangeSize(float size_X,float size_Z)
        {

            transform.localScale = new Vector3(size_X, 1, size_Z);
        }

    }

}