using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FStudio.MatchEngine
{
    public class ChangeColorChar : MonoBehaviour
    {

        [SerializeField] private Material _mat;
        [SerializeField] private List<SkinnedMeshRenderer> _bodys = new List<SkinnedMeshRenderer>();
        // Start is called before the first frame update
        void Start()
        {
            foreach (var gm in _bodys)
                gm.material = _mat;
        }

        
    }
}
