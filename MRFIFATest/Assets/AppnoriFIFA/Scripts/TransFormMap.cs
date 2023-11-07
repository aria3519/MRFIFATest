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


        [SerializeField] private ButtonListener _butListener;


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
        }*/




        /*private void OnEnable()
        {
            
            TransFormManager.Current.AddMap(this);
        }*/



        public void  ChangeSize(float size_X)
        {

            transform.localScale = new Vector3(size_X, size_X, size_X);
        }


        private void OnDisable()
        {
            if(TransFormManager.Current !=null)
                TransFormManager.Current.ReomveMap();
        }


       
        
    }

}