using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class TransKey : MonoBehaviour
{
    [SerializeField] private InputActionReference keyS;
    [SerializeField] private InputActionReference keyD;
    [SerializeField] private InputActionReference keyQ;

    [SerializeField] private InputAction useS;
    [SerializeField] private InputAction useD;
    [SerializeField] private InputAction useQ;

    public void UseKey(string st)
    {
        if(st == "Q")
        {
           

        }
        if (st == "D")
        {

        }
        if (st == "S")
        {

        }
    }
}
