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


        [SerializeField] private Button _canMove;

        /*private async void Start()
        {
            await Task.Run(() =>
            {
                TransFormManager.Current.AddMap(this);
                //_myTrans = GetComponent<Transform>();
            });
        }*/
        /* private void Awake()
         {
             TransFormManager.Current.AddMap(this);
         }*/


        /*private void Update()
        {

            if(Time.timeScale ==0 )
            {
                CheckOnOff();
            }
            
        }*/

        private void OnEnable()
        {
            _canMove.onClick.AddListener(() => CheckOnOff()); 
            TransFormManager.Current.AddMap(this);
        }



        public void  ChangeSize(float size_X)
        {

            transform.localScale = new Vector3(size_X, size_X, size_X);
        }


        private void OnDisable()
        {
            if(TransFormManager.Current !=null)
                TransFormManager.Current.ReomveMap();
        }


        public void CheckOnOff()
        {
           

            TransFormManager.Current.OnOffButton();
        }
        
    }

}