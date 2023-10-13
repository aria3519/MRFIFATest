using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using FStudio.MatchEngine.Graphics;

namespace FStudio.MatchEngine
{
    public class TransFormPlayerBase : MonoBehaviour
    {

        public Animator anim;

      
        [SerializeField] private List<SkinnedMeshRenderer> _bodys = new List<SkinnedMeshRenderer>();

        [SerializeField] private IKController _ikCon;
        private async void Awake()
        {
            await Task.Run(() =>
            {
                TransFormManager.Current.AddPlayer(this);
               
            });
        }
        public void TransPostion(Vector3 pos)
        {
            this.transform.position = pos;
            
        }
        public void TransRotation(Quaternion qu)
        {
            this.transform.rotation = qu;

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

        public void HoldBall(bool ishold)
        {
            if(ishold)
                _ikCon.LerpTo(1);
            else
                _ikCon.LerpTo(0);
        }

        

    }

}