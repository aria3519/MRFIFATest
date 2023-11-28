using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private List<Material> _mats = new List<Material>();

    private int num = 0;
    private float time = 0;
    

    private void Update()
    {
        time += Time.deltaTime;
        if (time > 1f)
        {
            num++;
            if (num > 1)
                num = 0;

            gameObject.GetComponent<MeshRenderer>().material = _mats[num];
            time = 0;
        }
    }

}
