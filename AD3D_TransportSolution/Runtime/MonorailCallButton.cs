using UnityEngine;

namespace AD3D_TransportSolution.Runtime
{
    // The button on a pylon's call box: brings the nearest free pod to this pylon
    public class MonorailCallButton : MonoBehaviour, IHandTarget
    {
        public MonorailPylon Pylon;

        public void OnHandHover(GUIHand hand)
        {
            if (Pylon == null || !Pylon.IsRegistered)
                return;
            var distance = MonorailPod.FreePodDistance(Pylon);
            HandReticle.main.SetText(HandReticle.TextType.Hand, "Call monorail pod", false);
            HandReticle.main.SetText(HandReticle.TextType.HandSubscript,
                distance < 0f ? "No free pod on this line" : distance < 0.5f ? "The pod is here" : $"Nearest free pod: {distance:0} m", false);
            HandReticle.main.SetIcon(distance < 0f ? HandReticle.IconType.HandDeny : HandReticle.IconType.Hand);
        }

        public void OnHandClick(GUIHand hand)
        {
            if (Pylon != null && Pylon.IsRegistered)
                ErrorMessage.AddMessage(MonorailPod.Call(Pylon));
        }
    }
}
