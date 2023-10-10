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

        private TransFormMap _baseMap = null;

        [SerializeField]
        private int Pos_x = 150;

        [SerializeField]
        private float _X = 1f;
        [SerializeField]
        private float _Z = 1f;


        public Notifier<float> Check_X;
        public Notifier<float> Check_Z;


        private void Awake()
        {
            //Debug.LogError("TransFormManager Awake");
            Addressables.InstantiateAsync("Assets/AppnoriFIFA/Prefabs/TransFormMap.prefab", new Vector3(Pos_x, 0, 0), Quaternion.identity);
            

            Check_X.Value = _X;
            Check_Z.Value = _Z;

            Check_X.OnDataChanged += OnChange_X;
            Check_Z.OnDataChanged += OnChange_Z;
        }


        private void Update()
        {
            if (Check_X.Value != _X) Check_X.Value = _X; 
            if (Check_Z.Value != _Z) Check_Z.Value = _Z; 



        }


        private void OnChange_X(float x)
        {
            _baseMap.ChangeSize(x, Check_Z.Value);
        }
        private void OnChange_Z(float z)
        {
            _baseMap.ChangeSize(Check_X.Value,z);
        }

        public void TransFormPlayerPos(Vector3 pos, int num)
        {
            Vector3 tempPos = pos;
            tempPos.x *= _X;
            tempPos.z *= _Z;
            tempPos.x += Pos_x;


            transPlayers[num].TransPostion(tempPos);
        }
        public void TransFormPlayerRota(Quaternion Q,int num)
        {
            transPlayers[num].TransRotation(Q);
        }
        
        public  void SetPlayer1(CodeBasedController player)
        {
            player.myNumber = total_num;
            total_num++;
            Addressables.InstantiateAsync("Assets/AppnoriFIFA/Prefabs/player.prefab",new Vector3(200, 0, 0), Quaternion.identity);
            
            
            
        }

        public void AddMap(TransFormMap map)
        {
            _baseMap = map;
        }
        public void AddPlayer(TransFormPlayerBase pl)
        {
            transPlayers.Add(pl);
        }




    }
}
