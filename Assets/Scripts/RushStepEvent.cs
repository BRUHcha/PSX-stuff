using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace JTools
{
    public class RushStepEvent : MonoBehaviour
    {
        public void PlayStepSound(AudioClip sound)
        {
            RushController.RushStepEventHandler(sound);
        }
    }
}