using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace FStudio.MatchEngine
{





    public class TransFormMap : MonoBehaviour
    {

        private Transform _myTrans;




        public GameObject _map;
        private async void Awake()
        {
            await Task.Run(() =>
            {
                TransFormManager.Current.AddMap(this);
                //_myTrans = GetComponent<Transform>();
            });
        }
        /* private void Awake()
         {
             TransFormManager.Current.AddMap(this);
         }
 */



        /*private void OnEnable()
        {
            
            TransFormManager.Current.AddMap(this);
        }*/



        public void  ChangeSize(float size_X)
        {
            if (size_X > 1 || size_X < 0.005f) return;



            transform.localScale = new Vector3(size_X, size_X, size_X);
            //_map.transform.localScale  = new Vector3(size_X, size_X, size_X);
        }


        private void OnDisable()
        {
            if(TransFormManager.Current !=null)
                TransFormManager.Current.ReomveMap();
        }


       
        
    }

}