using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AD3D_HabitatSolutionMod.BO.InGame
{
    public class Tooltip : MonoBehaviour, IHandTarget
    {
        public string Text;
        public string SubText;
        public void OnHandClick(GUIHand hand)
        {
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle.main.SetInteractText($"{Text}", $"{SubText}", false, false, HandReticle.Hand.Right);
            HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1.25f);
        }
    }
}
