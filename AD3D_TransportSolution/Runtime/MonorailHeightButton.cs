using UnityEngine;

namespace AD3D_TransportSolution.Runtime
{
    // The Up or Down button on a pylon's height panel: telescopes the pylon one level
    public class MonorailHeightButton : MonoBehaviour, IHandTarget
    {
        public MonorailPylon Pylon;
        public int Step;   // +1 up, -1 down

        public void OnHandHover(GUIHand hand)
        {
            if (Pylon == null || !Pylon.IsRegistered)
                return;
            HandReticle.main.SetText(HandReticle.TextType.Hand, Step > 0 ? "Raise pylon" : "Lower pylon", false);
            HandReticle.main.SetText(HandReticle.TextType.HandSubscript,
                $"Height {Pylon.Level + 1}/{MonorailPylon.Levels}: rail {Pylon.RailHeight:0.0} m above the foot", false);
            var atEnd = Step > 0 ? Pylon.Level >= MonorailPylon.Levels - 1 : Pylon.Level <= 0;
            HandReticle.main.SetIcon(atEnd ? HandReticle.IconType.HandDeny : HandReticle.IconType.Hand);
        }

        public void OnHandClick(GUIHand hand)
        {
            if (Pylon != null && Pylon.IsRegistered)
                ErrorMessage.AddMessage(Pylon.TryStep(Step));
        }
    }
}
