using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using FStudio.Utilities;
using System.Threading.Tasks;
using System;
using Appnori.Utils;
using FStudio.MatchEngine.Balls;
using FStudio.MatchEngine.Players.PlayerController;
using FStudio.Database;
using FStudio.MatchEngine.Graphics;

using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
namespace FStudio.MatchEngine
{

    public class TransFormManager : SceneObjectSingleton<TransFormManager>
    {
        

        private int total_num = 0;
        private bool once = false;
        
        //private List<CodeBasedController> Players_Postion = new List<CodeBasedController>();
        public List<TransFormPlayerBase> _transPlayers { private set; get; } = new List<TransFormPlayerBase>();
        private TransFormBall _transBall = null;

        private TransFormMap _transMap = null;

        [SerializeField]
        private List<Material> _mat = new List<Material>();
 
       
        [SerializeField] private Vector3 Pos;
        [SerializeField] private Quaternion Qut;

        [SerializeField]
        private float _total_rate = 1f;
        
      

        private Dictionary<string, int> animatorVariableHashes = null;
        public Notifier<float> Check_X;
       
        public Notifier<Vector3> ChangeMap_Pos;
        public Notifier<Quaternion> Check_Qut;


        [SerializeField] private List<ParticleSystem> Eff_goals = new List<ParticleSystem>();

        [SerializeField] private AudioSource _Sound_Goal;

        [SerializeField] private bool movingMap = false;

        private void Awake()
        {
            //Debug.LogError("TransFormManager Awake");


            Pos = new Vector3(199.29f, 0.8f, -3f);
            Qut = new Quaternion(0, 0, 0, 0);
            ChangeMap_Pos.Value = Pos;
            Check_Qut.Value = Qut;
            ChangeMap_Pos.OnDataChanged += OnChange_Pos;
            Check_Qut.OnDataChanged += OnChange_Qut;

            Addressables.InstantiateAsync("AppnoriFIFA/Prefabs/TransFormMap.prefab", new Vector3(ChangeMap_Pos.Value.x, ChangeMap_Pos.Value.y, ChangeMap_Pos.Value.z), Quaternion.identity);
            
            _total_rate = 1f;
            //_transMap.ChangeSize(_X);
            Check_X.Value = _total_rate;
           

            Check_X.OnDataChanged += OnChange_SizeX;
           
        }


        private void Update()
        {
            //Moving();
            if (Check_X.Value != _total_rate) Check_X.Value = _total_rate;
            if (ChangeMap_Pos.Value != Pos) ChangeMap_Pos.Value = Pos;
            if (Check_Qut.Value != Qut) Check_Qut.Value = Qut;

            
         

        }
        


        private void Moving()
        {
            /*Vector3 temp = new Vector3(199.29f, 0.8f, -3f);
            temp.x += XRController.posVec.x;
            temp.y += XRController.posVec.y;
            temp.z += XRController.posVec.z;
            Pos = temp;*/
            /*if (_transMap.transform.parent == null)
            {
                _transMap.transform.parent = GameObject.Find("RightHand Controller").transform;
                Time.timeScale = 0;
            }*/


            if (!movingMap)
            {
                
                if(_transMap.transform.parent!=null)
                {
                    Time.timeScale = 1.5f;
                    _transMap.transform.SetParent(null);
                }
               
                return;
            }

            _transMap.transform.parent = GameObject.Find("RightHand Controller").transform;
            Time.timeScale = 0;




        }
       

        public void SetDic(Dictionary<string, int> dict)
        {
            animatorVariableHashes = dict;
        }
        private void OnChange_SizeX(float x)
        {
            // 42.6 ,0, 29.95
            //* (29.95f / 42.6f);
           /* foreach (TransFormPlayerBase pl in _transPlayers)
            {
                if (pl.transform.parent == null)
                    pl.transform.parent = _transMap.transform;
            }

            if (_transBall.transform.parent == null)
                _transBall.transform.parent = _transMap.transform;
*/
            _transMap.ChangeSize(x);
           


            _transBall.TransSize(x);

            /*foreach(TransFormPlayerBase player in _transPlayers)
            {
                player.TransLocalSize(x);
            }*/
           
        }
       
        private void OnChange_Pos(Vector3 x)
        {
            // 42.6 ,0, 29.95
            //* (29.95f / 42.6f);
            Vector3 temp;

            temp = _transMap.transform.position;
            temp.x = x.x;
            temp.y = x.y;
            temp.z = x.z;
            _transMap.transform.position = temp;

        }
        private void OnChange_Qut(Quaternion x)
        {
            // 42.6 ,0, 29.95
            //* (29.95f / 42.6f);
            Quaternion temp;

            temp = _transMap.transform.rotation;
            temp.x = x.x;
            temp.y = x.y;
            temp.z = x.z;
            _transMap.transform.localRotation = temp;

        }

        public void TransFormBallPos(Vector3 pos)
        {
            if (_transBall == null)
                return;

            Vector3 tempPos = pos;
            tempPos.x *= _total_rate;
            //tempPos.z *= _X * (29.95f / 42.6f);
            tempPos.x += Pos.x;
            tempPos.y += 0.2f;
            tempPos.y *= _total_rate;
           
            tempPos.y += Pos.y;
            tempPos.z *= _total_rate;
            tempPos.z += Pos.z;
            _transBall.TransPostion(tempPos);
        }

        public void TransFormPlayerPos(Vector3 pos, int num)
        {
            //* (29.95f / 42.6f);
            Vector3 tempPos = pos;
            tempPos.x *= _total_rate;
            tempPos.y *= _total_rate;
            tempPos.z *= _total_rate;
            tempPos.x += Pos.x;
            tempPos.y += Pos.y;
            tempPos.z += Pos.z;

            

            if (_transPlayers.Count > 0)
                _transPlayers[num].TransPostion(tempPos);
        }
        public void TransFormPlayerRota(Quaternion Q,int num)
        {
            if (_transPlayers.Count > 0)
                _transPlayers[num].TransRotation(Q);
        }
        public void TransFormPlayerAnimTri(int num, PlayerAnimatorVariable i)
        {
            if (_transPlayers.Count > 0)
                _transPlayers[num].GetAnim().SetTrigger(animatorVariableHashes[i.ToString()]);
        }
        public void TransFormPlayerAnimBool(int num , PlayerAnimatorVariable i,bool b)
        {
            if (_transPlayers.Count > 0)
                _transPlayers[num].GetAnim().SetBool(animatorVariableHashes[i.ToString()], b);
        }
        public void TransFormPlayerAnimLayer(int num, int layer , float f)
        {
            if (_transPlayers.Count > 0)
                _transPlayers[num].GetAnim().SetLayerWeight(layer,f);
        }
        public void TransFormPlayerAnimFloat(int num, PlayerAnimatorVariable i , float f)
        {
           if(_transPlayers.Count>0)
            _transPlayers[num].GetAnim().SetFloat(animatorVariableHashes[i.ToString()], f);
        }

        public void TransFormAniHoldBall(int num,bool isHold)
        {
            if (_transPlayers.Count > 0
                && (num == 0 || num ==5))
            {
               /* float temp = -135f;
                if (num == 0) temp = 135f;*/
                _transPlayers[num].HoldBall(isHold, _transBall);

            }
                
        }
        public void TransFormCanPlay(int num, bool isplay)
        {
            _transPlayers[num].OnPlay(isplay);

        }


        public  void SetPlayer1(CodeBasedController player)
        {
            player.myNumber = total_num;
            total_num++;
            if (total_num > 10) 
                player.myNumber = -1;
            if (player.myNumber<10 && player.myNumber!=-1)
            Addressables.InstantiateAsync("Assets/AppnoriFIFA/Prefabs/player.prefab",new Vector3(200, 0, 0),Quaternion.identity);
            /*foreach (TransFormPlayerBase player1 in _transPlayers)
            {
                player1.TransLocalSize(_total_rate);
            }*/


        }

        public void SetPlayerColor(int num)
        {
            if (_transPlayers.Count <= 0) return;

            if (num < 5) _transPlayers[num].SetColor(_mat[0]);
            else _transPlayers[num].SetColor(_mat[1]);


           

        }
        public void SetBall(Ball player)
        {

           
            Addressables.InstantiateAsync("Assets/AppnoriFIFA/Prefabs/Ball.prefab", new Vector3(0, 0, 0), Quaternion.identity);

        }

        public void AddMap(TransFormMap map)
        {
            _transMap = map;
            _total_rate = 0.02f;
        }
        public void ReomveMap()
        {
            _transMap = null;
            _total_rate = 1;
            Pos = new Vector3(199.29f, 0.8f, -3f);
        }
        public void AddPlayer(TransFormPlayerBase pl)
        {

            pl.transform.parent = _transMap.transform;
            _transPlayers.Add(pl);
            foreach (TransFormPlayerBase player1 in _transPlayers)
            {
                player1.TransLocalSize(1);
            }


        }
        public void RemovePlayer(TransFormPlayerBase pl)
        {
            _transPlayers.Remove(pl);
        }
        public void AddBall(TransFormBall ball)
        {
            _transBall = ball;

            _transBall.TransSize(_total_rate);
            //_transBall.transform.parent = _transMap.transform;
        }


        public void EffGoal(int isHome)
        {
           

            //Eff_goals[isHome].Play();


        }

        public void OnOffButton()
        {
            if (!movingMap)
            {
                movingMap = true;
                if (_transMap.transform.parent != null)
                {
                    Time.timeScale = 1.5f;
                    _transMap.transform.SetParent(null);
                }

                
            }
            else
            {
                movingMap = false;
                _transMap.transform.parent = GameObject.Find("RightHand Controller").transform;
                Time.timeScale = 0;
            }

            


        }


    }
}
