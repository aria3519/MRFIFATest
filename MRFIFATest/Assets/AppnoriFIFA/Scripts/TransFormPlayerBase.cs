using System.Collections.Generic;
using System.Threading.Tasks;
using FStudio.MatchEngine.Graphics;
using UnityEngine;
namespace FStudio.MatchEngine
{
    public class TransFormPlayerBase : MonoBehaviour
    {

        public Animator anim;

      
        [SerializeField] private List<SkinnedMeshRenderer> _bodys = new List<SkinnedMeshRenderer>();

        [SerializeField] private IKController _ikCon;
      


        [SerializeField] private List<GameObject> _hands = new List<GameObject>();
        [SerializeField] private TransFormBall _ball = null;
        [SerializeField] private GameObject _target;

        private bool _isHold = false;
        /*private async void Awake()
        {
            await Task.Run(() =>
            {
                TransFormManager.Current.AddPlayer(this);



            });
        }*/


        private void Awake()
        {
            _target.SetActive(false);
            TransFormManager.Current.AddPlayer(this);
        }

        /*private void OnEnable()
        {
            _target.SetActive(false);
            TransFormManager.Current.AddPlayer(this);
        }*/

        private void OnDisable()
        {
            if (TransFormManager.Current != null)
                TransFormManager.Current.RemovePlayer(this);
        }
        public void TransPostion(Vector3 pos)
        {
            this.transform.position = pos;
            

        }
        public void TransRotation(Quaternion qu)
        {
            this.transform.rotation = qu;

        }
        public void TransLocalSize(float size)
        {
            this.transform.localScale = new Vector3(size*2f,size * 2f, size * 2f);

        }
        public Animator GetAnim()
        {
            if (anim == null)
                return null;
            else
                return anim;
        }
        

        public void SetColor(Material color1)
        {
           
            foreach (var gm in _bodys)
                gm.material = color1;
        }

        public void HoldBall(bool ishold, TransFormBall ball)
        {
            if(ishold)
            {
                _isHold = true;
                _ball = ball;
                


                _ikCon.LerpTo(1);

                // 현재 공을 잡는 위치 계산


                /*var forward = transform.forward;
                forward.y = 0;

                var agl = Vector3.Angle(forward, Direction);
                var sign = Mathf.Sign(Vector3.Dot(Vector3.up, Vector3.Cross(forward, Direction)));
                var angle = agl * sign;

                new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));*/

                //ball.transform.rotation = new Quaternion(0, num, 0, 0);
                ball.transform.forward = transform.forward;
                _ikCon.GetHand(ball.handPos[0].transform, ball.handPos[1].transform);

                anim.SetBool("IsHoldingBall", true);
                
            }

            else
            {
                _isHold = false;
                _ball = null;
                _ikCon.LerpTo(0);
                anim.SetBool("IsHoldingBall", false);
            }

        }


        public void OnPlay(bool isplay)
        {
            _target.SetActive(isplay);
        }

        

    }

}