using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;


public class TransInputListener : MonoBehaviour
{


    [SerializeField] private XRController _con1;
    [SerializeField] private XRBaseController _con2;

   


    private List<float> _ColList = new List<float>();

    private void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            float temp = -1;
            _ColList.Add(temp);
        }
    }


    void Update()
    {
        CheckInputTri2();
        CheckInputPrimary();
        CheckInputGrip();
        ColTime();
    }


    private bool CheckInputTri()
    {
        bool tri = false;
        if (_con1.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out tri)
            && _ColList[0]==-1)
        {
            if (tri)
            {
                //D
                //Debug.LogError("CheckInputTri");
                _ColList[0] = 0;
               


                return true;
            }

        }
        return false;

    }
    private bool CheckInputTri2()
    {
        bool tri = false;



        
        if (_con1.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out tri)
            && _ColList[0] == -1)
        {
            if (tri)
            {
                //D
                //Debug.LogError("CheckInputTri");
                _ColList[0] = 0;



                return true;
            }

        }
        return false;

    }
    private bool CheckInputPrimary()
    {
        bool tri = false;
        if (_con1.inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out tri)
            &&_ColList[1] == -1)
        {
            if (tri)
            {
                // Q
                //Debug.LogError("CheckInputPrimary");
                _ColList[1] = 0;
                //Input.GetKeyUp(KeyCode.Q);

                return true;
            }

        }
        return false;

    }
    private bool CheckInputGrip()
    {
        float tri = 0;
        if (_con1.inputDevice.TryGetFeatureValue(CommonUsages.grip, out tri)
            && _ColList[2] == -1)
        {
            if (tri>0.5f)
            {
                // S
               // Debug.LogError("CheckInputGrip");
                //Input.(KeyCode.S);

                
                _ColList[2] = 0;
                return true;
            }

        }
        return false;

    }
    
    private void ColTime()
    {

        for(int i=0;i<_ColList.Count ;i++)
        {
            if (_ColList[i] != -1) _ColList[i] += Time.deltaTime;
            if (_ColList[i] > 1f) _ColList[i] = -1;

        }
        
    }

    


}
