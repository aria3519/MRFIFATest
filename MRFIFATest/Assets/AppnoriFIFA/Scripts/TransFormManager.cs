using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using FStudio.Utilities;
using FStudio.Input;
using System.Threading.Tasks;
using System;
using Appnori.Utils;
using FStudio.MatchEngine.Balls;
using FStudio.MatchEngine.Players.PlayerController;
using UnityEngine.InputSystem;

using System.Linq;
using UnityEngine.InputSystem.XR;
using FStudio.Database;
using FStudio.MatchEngine.Graphics;

using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

using Unity.XR.PXR;
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
        private bool checkTarget = false;
        private Vector3 mapPostions = new Vector3(0,0,0);
        
      

        private Dictionary<string, int> animatorVariableHashes = null;
        public Notifier<float> Check_X;
       
        public Notifier<Vector3> ChangeMap_Pos;
        public Notifier<Quaternion> Check_Qut;


        [SerializeField] private List<ParticleSystem> Eff_goals = new List<ParticleSystem>();

        [SerializeField] private AudioSource _Sound_Goal;

        [SerializeField] private bool movingMap = true;
        [SerializeField] private bool sizeup = false;
        [SerializeField] private bool sizedown = false;


        [SerializeField] private InputActionAsset inputSystem ;
        [SerializeField] private GameObject _ballShadow;
        public float _BallShadowA;
        public float _MapSizeA;
        public float _MapMovingA;

        private List<Vector3> ContrlGap = new List<Vector3>();
        private List<Vector3> ContrlPos = new List<Vector3>();
        private List<ActionBasedController> _XRcontros = new List<ActionBasedController>();
        private void Awake()
        {
            //Debug.LogError("TransFormManager Awake");
            //42.2 ,0, 31.4
            // Pos = new Vector3(200.14f, 0.8f, -2.411f);
            // Pos = new Vector3(199.29f, 0.8f, -2.5f);
            Pos = new Vector3(200.14f, 0.8f, -2.411f);
            Qut = new Quaternion(0, 0, 0, 0);
            ChangeMap_Pos.Value = Pos;
            Check_Qut.Value = Qut;
            ChangeMap_Pos.OnDataChanged += OnChange_Pos;
            Check_Qut.OnDataChanged += OnChange_Qut;

            Addressables.InstantiateAsync("Assets/AppnoriFIFA/Prefabs/TransFormMap2.prefab", new Vector3(ChangeMap_Pos.Value.x, ChangeMap_Pos.Value.y, ChangeMap_Pos.Value.z), Quaternion.identity);
            
            _total_rate = 2f;
            //_transMap.ChangeSize(_X);
            Check_X.Value = _total_rate;
           
            Check_X.OnDataChanged += OnChange_SizeX;
            ContrlPos.Add(new Vector3(0, 0, 0));
            ContrlPos.Add(new Vector3(0, 0, 0));
            //initInput();
            StartCoroutine(FindXRContr());
            

        }

        private IEnumerator FindXRContr()
        {

            while (true)
            {

                if (GameObject.Find("XR Origin/Camera Offset/LeftHand Controller") != null
                    && GameObject.Find("XR Origin/Camera Offset/RightHand Controller") != null)

                {
                    var contrL = GameObject.Find("XR Origin/Camera Offset/LeftHand Controller").GetComponent<ActionBasedController>();
                    var contrR = GameObject.Find("XR Origin/Camera Offset/RightHand Controller").GetComponent<ActionBasedController>();

                    _XRcontros.Add(contrL);
                    _XRcontros.Add(contrR);
                    yield break;
                }


                yield return null;
            }
        }

        protected override void  OnEnable()
        {
            //PXR_Boundary.EnableSeeThroughManual(true);
            //ControllerManager.Current.SetLine(false);
        }
        private void OnDisable()
        {
            //PXR_Boundary.EnableSeeThroughManual(false);
            //ControllerManager.Current.SetLine(true);
        }

        private void initInput()
        {
            inputSystem.Enable();
            InputActionMap Map = inputSystem.actionMaps[0];



            InputAction Moving = Map.FindAction("Moving");
            InputAction MovingUpDown = Map.FindAction("MovingUpDown");
            InputAction SizeUp = Map.FindAction("SizeUp");
            InputAction SizeDown = Map.FindAction("SizeDown");

            Moving.performed += ActionMoving;
            MovingUpDown.performed += ActionMovingUPDown;
            SizeUp.performed += ActionSizeUp;
            SizeUp.canceled += ActionSizeUpEnd;
            SizeDown.performed += ActionSizeDown;
            SizeDown.canceled += ActionSizeDownEnd;
            /* SizeUp.started += ActionSizeUpStart;
             SizeUp.canceled += ActionSizeUpEnd;
             SizeDown.started += ActionSizeDownStart;
             SizeDown.canceled += ActionSizeDownEnd;*/

            //Debug.LogError("initInput");
        }
        private void Romoveinput()
        {
            inputSystem.Disable();
            InputActionMap Map = inputSystem.actionMaps[0];
            InputAction Moving = Map.FindAction("Moving");
            InputAction MovingUpDown = Map.FindAction("MovingUpDown");
            InputAction SizeUp = Map.FindAction("SizeUp");
            InputAction SizeDown = Map.FindAction("SizeDown");

            Moving.performed -= ActionMoving;
            MovingUpDown.performed -= ActionMovingUPDown;
            SizeUp.performed -= ActionSizeUp;
            SizeUp.canceled -= ActionSizeUpEnd;
            SizeDown.performed -= ActionSizeDown;
            SizeDown.canceled -= ActionSizeDownEnd;
            /* SizeUp.started += ActionSizeUpStart;
             SizeUp.canceled += ActionSizeUpEnd;
             SizeDown.started += ActionSizeDownStart;
             SizeDown.canceled += ActionSizeDownEnd;*/
            //Debug.LogError("Romoveinput");
        }


        public void ActionMoving(InputAction.CallbackContext ctx)
        {
            //Debug.LogError("Moving");
            

            Vector2 temp = ctx.action.ReadValue<Vector2>();
            //Debug.LogError("Moving"+ temp);
            Pos.x += temp.x*0.001f;
            Pos.z += temp.y*0.001f;


            


        }
        public void ActionMovingUPDown(InputAction.CallbackContext ctx)
        {
            //Debug.LogError("Moving");


            Vector2 temp = ctx.action.ReadValue<Vector2>();
            //Debug.LogError("Moving"+ temp);

            Pos.y += temp.y * 0.001f;
           


        }

        
        
        public void ActionSizeUp(InputAction.CallbackContext ctx)
        {
            var temp = ctx.action.ReadValue<float>();
            //Debug.LogError("ActionSizeUp" + temp);
            //Debug.LogError("SizeUpDown");

            _total_rate += temp * 0.0003f;
            sizeup = true;
            
        }
        public void ActionSizeUpEnd(InputAction.CallbackContext ctx)
        {
            var temp = ctx.action.ReadValue<float>();
            //Debug.LogError("ActionSizeUp" + temp);
            //Debug.LogError("SizeUpDown");

            //_total_rate += temp * 0.0003f;
            sizeup = false;
          
        }
        public void ActionSizeDown(InputAction.CallbackContext ctx)
        {
            var temp = ctx.action.ReadValue<float>();
            //Debug.LogError("ActionSizeUp" + temp);
            //Debug.LogError("SizeUpDown");

            _total_rate -= temp * 0.0003f;
            sizedown = true;
          

        }
        public void ActionSizeDownEnd(InputAction.CallbackContext ctx)
        {
            var temp = ctx.action.ReadValue<float>();
            //Debug.LogError("ActionSizeUp" + temp);
            //Debug.LogError("SizeUpDown");

            sizedown = false;
          

        }


        private void ChangeMapSize()
        {
            /* if (ControllerManager._ButtonSt.GripL == false
                   && ControllerManager._ButtonSt.GripR == false)
                 return;*/
            if (Time.timeScale != 0)
                return;

            Vector3 contrlPos = new Vector3(0, 0, 0);
            Vector3 contrlPosOther = new Vector3(0, 0, 0);

            if (ControllerManager._ButtonSt.GripL == true
                 && ControllerManager._ButtonSt.GripR == true)
            {
                contrlPos = ControllerManager.Current._XRcontros[0].transform.position;
                contrlPosOther = ControllerManager.Current._XRcontros[1].transform.position;

                ContrlGap.Add(contrlPos);
                ContrlGap.Add(contrlPosOther);

                if (ContrlGap.Count > 2)
                {

                    if (Vector3.Distance(contrlPos, contrlPosOther) - Vector3.Distance(ContrlGap[0], ContrlGap[1]) 
                    > 0.01f)
                {
                    _total_rate *= (1+(Vector3.Distance(contrlPos, contrlPosOther) - Vector3.Distance(ContrlGap[0], ContrlGap[1])));
                   
                    //_total_rate += _MapSizeA ;
                    // _total_rate += _MapSizeA * (Vector3.Distance(contrlPos, contrlPosOther) - Vector3.Distance(ContrlGap[0], ContrlGap[1]));

                }
                else if(Vector3.Distance(ContrlGap[0], ContrlGap[1]) - Vector3.Distance(contrlPos, contrlPosOther)
                    > 0.01f)
                {

                    if((1 - (Vector3.Distance(ContrlGap[0], ContrlGap[1]) - Vector3.Distance(contrlPos, contrlPosOther)))>0)
                    _total_rate *= (1 - (Vector3.Distance(ContrlGap[0], ContrlGap[1]) - Vector3.Distance(contrlPos, contrlPosOther)));
                   
                    //_total_rate -= _MapSizeA ;
                    //_total_rate -= _MapSizeA* (Vector3.Distance(ContrlGap[0], ContrlGap[1]) - Vector3.Distance(contrlPos, contrlPosOther));
                }

                /* if(check== 1) _total_rate += _MapSizeA * Vector3.Distance(contrlPos, contrlPosOther);
                 else if(check == 0) _total_rate -= _MapSizeA * Vector3.Distance(contrlPos, contrlPosOther);*/
                //if (check == 1) _total_rate += _MapSizeA ;
                //else if (check == 0) _total_rate -= _MapSizeA ;



               

                    ContrlGap.Remove(ContrlGap.First<Vector3>());
                    ContrlGap.Remove(ContrlGap.First<Vector3>());
                }



            }
            else
            {
                ContrlGap.Clear();
            }
            Vector3 temp = new Vector3(0, 0, 0);
            if (ControllerManager._ButtonSt.TriL == true
                )
            {
                 //&& checkTarget == false
                contrlPos = ControllerManager.Current._XRcontros[0].transform.position;
                //_transMap.transform.position = contrlPos;
                // 이전 위치 기억 하고 이전 위치에서 현재 위치 빼서 적용하기

                if (ContrlPos[0] == new Vector3(0, 0, 0))
                {
                    ContrlPos[0] = contrlPos;
                    mapPostions = _transMap.transform.position;
                    return;
                }
                temp = mapPostions;
                /*if ((contrlPos - ContrlPos[0]).magnitude > 0.01f)
                {
                    temp += (contrlPos - ContrlPos[0]).normalized * _MapMovingA;
                    //ChangeMap_Pos.Value = Vector3.Lerp(_transMap.transform.position, temp, Time.unscaledDeltaTime);
                    Pos = temp;
                    //checkTarget = true;
                }*/

                //temp += (contrlPos - ContrlPos[0]).normalized * _MapMovingA;
                //Pos = temp;
                //ContrlPos[0] = contrlPos;

                temp += (contrlPos - ContrlPos[0]).normalized * ((contrlPos - ContrlPos[0]).magnitude * _MapMovingA);
                Pos = temp;


            }
            else if (ControllerManager._ButtonSt.TriR == true
               )
            {
                //&& checkTarget == false
                contrlPos = ControllerManager.Current._XRcontros[1].transform.position;

                //_transMap.transform.position = contrlPos;
                if (ContrlPos[1] == new Vector3(0, 0, 0))
                {
                    mapPostions = _transMap.transform.position;
                    ContrlPos[1] = contrlPos;
                    return;
                }
                temp = mapPostions;
                /*if ((contrlPos - ContrlPos[1]).magnitude > 0.01f)
                {
                    temp += (contrlPos - ContrlPos[1]).normalized * _MapMovingA;
                    //ChangeMap_Pos.Value = Vector3.Lerp(_transMap.transform.position, temp, Time.unscaledDeltaTime);
                    Pos = temp;
                    //checkTarget = true;
                }*/
                /*temp += (contrlPos - ContrlPos[1]).normalized * _MapMovingA;
                Pos = temp;*/

                // Debug.LogError("1ss" + (contrlPos - ContrlPos[1]).normalized);
                //Debug.LogError("2ss" + Time.unscaledDeltaTime);


                temp += (contrlPos - ContrlPos[1]).normalized * ((contrlPos - ContrlPos[1]).magnitude* _MapMovingA);

                Pos = temp;



                //ContrlPos[1] = contrlPos;
            }
            else
            {
                ContrlPos[0] = new Vector3(0, 0, 0);
                ContrlPos[1] = new Vector3(0, 0, 0);


            }



        }
        

        private void Update()
        {
            
            if (Check_X.Value != _total_rate) Check_X.Value = _total_rate;
            if (ChangeMap_Pos.Value != Pos) ChangeMap_Pos.Value = Pos;
            if (Check_Qut.Value != Qut) Check_Qut.Value = Qut;



            ChangeMapSize();
        }


        /*private void LateUpdate()
        {
            //ChangeMapSize();
        }*/




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
           


            //_transBall.TransSize(x);

            /*foreach(TransFormPlayerBase player in _transPlayers)
            {
                player.TransLocalSize(x);
            }*/
           
        }
       
        private void OnChange_Pos(Vector3 x)
        {
            // 42.6 ,0, 29.95
            //* (29.95f / 42.6f);

            //StartCoroutine(MapMoving(x));








            Vector3 temp;

            temp = _transMap.transform.position;
            temp.x = x.x;
            //temp.x += _total_rate*42.2f;
            temp.y = x.y;
            temp.z = x.z;
            //temp.z += _total_rate* 31.4f;

            _transMap.transform.position =  temp;

            //_transMap.transform.position = Vector3.Lerp(_transMap.transform.position,temp, Time.unscaledDeltaTime);
            //checkTarget = false;
        }

        private IEnumerator MapMoving(Vector3 x)
        {
            float targerRate = 0;
            Vector3 temp;
            temp = _transMap.transform.position;
            temp.x = x.x;
            //temp.x += _total_rate*42.2f;
            temp.y = x.y;
            temp.z = x.z;
            //temp.z += _total_rate* 31.4f;
            while (true)
            {
               
                //_transMap.transform.position =     temp;
                targerRate += 0.2f;
                _transMap.transform.position = Vector3.Lerp(_transMap.transform.position,temp, targerRate);
                //Debug.LogError("ssss"+_transMap.transform.position);
                if (targerRate >= 1)
                {
                    checkTarget = false;
                    break;
                }
                yield return null;
            }
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
            tempPos.x += _transMap.transform.position.x;
            tempPos.x -= 42.2f *_total_rate;
            tempPos.y += 0.3f;
            tempPos.y *= _total_rate;
           
            tempPos.y += _transMap.transform.position.y;

            float y = tempPos.y;
            tempPos.z *= _total_rate;
            tempPos.z += _transMap.transform.position.z;
            tempPos.z += -31.4f *+_total_rate;
            _transBall.TransPostion(tempPos);
            tempPos.y =_transMap.transform.position.y+0.005f;
            //_transBall.TransPostionShadow(tempPos);


            if (_ballShadow != null)
            {
                _ballShadow.transform.position = tempPos;

                float GabY = 1;
                if ((y - _ballShadow.transform.position.y) > 0.01f)
                    GabY = 1 / (transform.position.y - _ballShadow.transform.position.y);
                else
                    GabY = (1 / _BallShadowA) * 0.02f;

                if (GabY >= 0.02f)
                    _ballShadow.transform.localScale = new Vector3(GabY * _BallShadowA,
                       GabY * _BallShadowA,
                       GabY * _BallShadowA
                        );

            }
        }

        public void TransFormPlayerPos(Vector3 pos, int num)
        {
            //* (29.95f / 42.6f);
            Vector3 tempPos = pos;
            tempPos.x *= _total_rate;
            tempPos.y *= _total_rate;
            tempPos.z *= _total_rate;
            tempPos.x += _transMap.transform.position.x;
            tempPos.x -= 42.2f * _total_rate;
            tempPos.y += _transMap.transform.position.y;
            tempPos.z += _transMap.transform.position.z;
            tempPos.z += -31.4f * +_total_rate ;



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
                _transBall.SetHoldGk(isHold);

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
            if (total_num > 22) 
                player.myNumber = -1;
            if (player.myNumber<22 && player.myNumber!=-1)
            Addressables.InstantiateAsync("Assets/AppnoriFIFA/Prefabs/player.prefab",new Vector3(200, 0, 0),Quaternion.identity);
            /*foreach (TransFormPlayerBase player1 in _transPlayers)
            {
                player1.TransLocalSize(_total_rate);
            }*/


        }

        public void SetPlayerColor(int num , Vector3 pos)
        {
            if (_transPlayers.Count <= 0) return;

            if (num < 11)
            {
                _transPlayers[num].SetColor(_mat[0]);
               
            }
            else
            {
                _transPlayers[num].SetColor(_mat[1]);
            }
            _transPlayers[num].TransPostion(pos);



        }
        public void SetBall(Ball player)
        {

           
            Addressables.InstantiateAsync("Assets/AppnoriFIFA/Prefabs/Ball.prefab", new Vector3(0, 0, 0), Quaternion.identity);

        }

        public void AddMap(TransFormMap map)
        {
            _transMap = map;

            //_transMap.transform.position = new Vector3(200.14f, 0.8f, -2.411f);
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
            
            //pl.transform.parent = _transMap._map.transform; ;
            _transPlayers.Add(pl);
            foreach (TransFormPlayerBase player1 in _transPlayers)
            {
                player1.TransLocalSize(1f);
            }


        }
        public void RemovePlayer(TransFormPlayerBase pl)
        {
            _transPlayers.Remove(pl);
        }
        /*public void AddBall(TransFormBall ball)
        {
            _transBall = ball;



            _transBall.TransSize(0.35f);
            _transBall.transform.parent = _transMap.transform;
        }*/



        public IEnumerator AddBall(TransFormBall ball)
        {

            if (_transBall == null)
                yield return null;

           
            _transBall = ball;



            _transBall.TransSize(1f);
            //_transBall.transform.parent = _transMap._map.transform;
            _transBall.transform.parent = _transMap.transform;
            _total_rate = 0.02f;



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
                Time.timeScale = 1.5f;
                // 맵 크기 변경 모드 해제 


                //Romoveinput();
                ControllerManager.Current.RemoveInputAction();
            }
            else
            {
                movingMap = false;
                Time.timeScale = 0;
                // 맵 크기 변경 모드

                //initInput();

                ControllerManager.Current.OnInputAction();

            }

            


        }



        

    }
}
