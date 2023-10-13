using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using FStudio.Utilities;
using System.Threading.Tasks;
using System;
using Appnori.Util;
using FStudio.MatchEngine.Balls;
using FStudio.MatchEngine.Players.PlayerController;
using FStudio.Database;
using FStudio.MatchEngine.Graphics;
namespace FStudio.MatchEngine
{

    public class TransFormManager : SceneObjectSingleton<TransFormManager>
    {
        

        private int total_num = 0;
        
        //private List<CodeBasedController> Players_Postion = new List<CodeBasedController>();
        public List<TransFormPlayerBase> transPlayers { private set; get; } = new List<TransFormPlayerBase>();
        private TransFormBall transBall = null;

        private TransFormMap _baseMap = null;

        [SerializeField]
        private List<Material> _mat = new List<Material>();
        [SerializeField]
        private int Pos_x = 150;

        [SerializeField]
        private float _X = 1f;
        [SerializeField]
        private float _Z = 1f;

        private Dictionary<string, int> animatorVariableHashes = null;
        public Notifier<float> Check_X;
        public Notifier<float> Check_Z;


        private void Awake()
        {
            //Debug.LogError("TransFormManager Awake");
            Addressables.InstantiateAsync("AppnoriFIFA/Prefabs/TransFormMap.prefab", new Vector3(Pos_x, 0, 0), Quaternion.identity);
            

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
        
        public void SetDic(Dictionary<string, int> dict)
        {
            animatorVariableHashes = dict;
        }
        private void OnChange_X(float x)
        {
            _baseMap.ChangeSize(x, Check_Z.Value);
        }
        private void OnChange_Z(float z)
        {
            _baseMap.ChangeSize(Check_X.Value,z);
        }
        public void TransFormBallPos(Vector3 pos)
        {
            if (transBall == null)
                return;

            Vector3 tempPos = pos;
            tempPos.x *= _X;
            tempPos.z *= _Z;
            tempPos.x += Pos_x;
            tempPos.y += 0.3f;
            transBall.TransPostion(tempPos);
        }

        public void TransFormPlayerPos(Vector3 pos, int num)
        {
            Vector3 tempPos = pos;
            tempPos.x *= _X;
            tempPos.z *= _Z;
            tempPos.x += Pos_x;


            if (transPlayers.Count > 0)
                transPlayers[num].TransPostion(tempPos);
        }
        public void TransFormPlayerRota(Quaternion Q,int num)
        {
            if (transPlayers.Count > 0)
                transPlayers[num].TransRotation(Q);
        }
        public void TransFormPlayerAnimTri(int num, PlayerAnimatorVariable i)
        {
            if (transPlayers.Count > 0)
                transPlayers[num].GetAnim().SetTrigger(animatorVariableHashes[i.ToString()]);
        }
        public void TransFormPlayerAnimBool(int num , PlayerAnimatorVariable i,bool b)
        {
            if (transPlayers.Count > 0)
                transPlayers[num].GetAnim().SetBool(animatorVariableHashes[i.ToString()], b);
        }
        public void TransFormPlayerAnimLayer(int num, int layer , float f)
        {
            if (transPlayers.Count > 0)
                transPlayers[num].GetAnim().SetLayerWeight(layer,f);
        }
        public void TransFormPlayerAnimFloat(int num, PlayerAnimatorVariable i , float f)
        {
           if(transPlayers.Count>0)
            transPlayers[num].GetAnim().SetFloat(animatorVariableHashes[i.ToString()], f);
        }

        public void TransFormAniHoldBall(int num,bool isHold)
        {
            if (transPlayers.Count > 0)
            {
                transPlayers[num].HoldBall(isHold);

            }
                
        }


        public  void SetPlayer1(CodeBasedController player)
        {
            player.myNumber = total_num;
            total_num++;
            if (total_num > 10)
                player.myNumber = -1;
            if (player.myNumber<10 && player.myNumber!=-1)
            Addressables.InstantiateAsync("Assets/AppnoriFIFA/Prefabs/player.prefab",new Vector3(200, 0, 0),Quaternion.identity);
            
            
            
        }

        public void SetPlayerColor(int num)
        {
            if (transPlayers.Count <= 0) return;

            if (num < 5) transPlayers[num].SetColor(_mat[0]);
            else transPlayers[num].SetColor(_mat[1]);


           

        }
        public void SetBall(Ball player)
        {

           
            Addressables.InstantiateAsync("Assets/AppnoriFIFA/Prefabs/Ball.prefab", new Vector3(0, 0, 0), Quaternion.identity);

        }

        public void AddMap(TransFormMap map)
        {
            _baseMap = map;
        }
        public void AddPlayer(TransFormPlayerBase pl)
        {
            transPlayers.Add(pl);
        }
        public void AddBall(TransFormBall ball)
        {
            transBall = ball;
        }




    }
}
