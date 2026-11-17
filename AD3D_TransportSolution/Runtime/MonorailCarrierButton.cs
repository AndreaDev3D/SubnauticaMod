using UnityEngine;

namespace AD3D_TransportSolution.Runtime
{
    // A deck button on a Prawn carrier's post: slides the deck out on its own side, or back in
    public class MonorailCarrierButton : MonoBehaviour, IHandTarget
    {
        public MonorailPrawnCarrier Carrier;
        public int Side;   // +1 / -1 across the car

        public void OnHandHover(GUIHand hand)
        {
            if (Carrier == null || !Carrier.IsBuilt)
                return;
            HandReticle.main.SetText(HandReticle.TextType.Hand, Carrier.DeckIn ? "Slide deck out" : "Slide deck in", false);
            HandReticle.main.SetText(HandReticle.TextType.HandSubscript, Carrier.DeckStatus, false);
            HandReticle.main.SetIcon(HandReticle.IconType.Hand);
        }

        public void OnHandClick(GUIHand hand)
        {
            if (Carrier != null && Carrier.IsBuilt)
            {
                var message = Carrier.ToggleDeck(Side);
                if (message != null)
                    ErrorMessage.AddMessage(message);
            }
        }
    }
}
