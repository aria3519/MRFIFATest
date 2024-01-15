using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace FStudio.MatchEngine
{
    public abstract class ButtonListener :MonoBehaviour
    {
        //[SerializeField] protected InputAction _inputAction;
        public virtual void OnButtonClick()
        {
            
        }
    }

}