using AD3D_Common;
using UnityEngine;
using UnityEngine.UI;

namespace AD3D_LightSolutionMod.BO.InGame
{
    public class DeepEngineAnim : MonoBehaviour
    {
        public bool IsEnabled;
        public Button btnActivate;
        public GameObject engine;


        private Animation anim;
        // Start is called before the first frame update
        public void Start()
        {
            IsEnabled = false;

            engine = GameObjectFinder.FindByName(this.gameObject, "Engine");

            anim = engine.GetComponent<Animation>();
            anim.AddClip(AD3D_DeepEngineMod.BO.Utils.Helper.Bundle.LoadAsset<AnimationClip>("Drilling"), "Drilling");

            btnActivate = GameObjectFinder.FindByName(this.gameObject, "btnActivate").GetComponent<Button>();
            btnActivate.onClick.AddListener(() => StartNStop());

            StartNStop();
        }

        public void StartNStop()
        {
            IsEnabled = !IsEnabled;
            if (IsEnabled)
            {
                anim.Play("Drilling");
            }
            else
            {
                anim.Stop();
            }
        }
    }
}
