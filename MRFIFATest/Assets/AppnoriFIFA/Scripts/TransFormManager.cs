using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using FStudio.Utilities;
using System.Threading.Tasks;
using System;
using Appnori.Util;
using FStudio.MatchEngine.Players.PlayerController;
namespace FStudio.MatchEngine
{

    public class TransFormManager : SceneObjectSingleton<TransFormManager>
    {
        private bool isLoad = false;


        private int total_num = 0;
        
        //private List<CodeBasedController> Players_Postion = new List<CodeBasedController>();
        private List<TransFormPlayerBase> transPlayers = new List<TransFormPlayerBase>();



        [SerializeField]
        private int Pos_x = 120;

        [SerializeField]
        private float _X = 1f;
        [SerializeField]
        private float _Z = 1f;



        private void Awake()
        {
            //Debug.LogError("TransFormManager Awake");
            Addressables.InstantiateAsync("Assets/AppnoriFIFA/Prefabs/TransformMap.prefab", new Vector3(Pos_x, 0, 0), Quaternion.identity);
        }
        
       

      
        public void TransFormPlayerPos(Vector3 pos, int num)
        {
            Vector3 tempPos = pos;
            tempPos.x *= _X;
            tempPos.z *= _Z;
            tempPos.x += Pos_x;


            transPlayers[num].TransPostion(tempPos);
        }
        public void TransFormPlayerRota()
        {
            
        }
        
        public  void SetPlayer1(CodeBasedController player)
        {
            player.myNumber = total_num;
            total_num++;
            Addressables.InstantiateAsync("Assets/AppnoriFIFA/Prefabs/player.prefab",new Vector3(200, 0, 0), Quaternion.identity);
            
            
            
        }

        public void AddPlayer(TransFormPlayerBase pl)
        {
            transPlayers.Add(pl);
        }




    }
}
