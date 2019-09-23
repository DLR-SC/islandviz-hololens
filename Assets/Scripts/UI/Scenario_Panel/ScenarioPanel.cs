using System.Collections;

namespace HoloIslandVis.UI.Component
{
    public class ScenarioPanel : UIComponent
    {

        public override IEnumerator Activate()
        {
            gameObject.SetActive(true);
            yield break;
        }

        public override IEnumerator Deactivate()
        {
            gameObject.SetActive(false);
            yield break;
        }
    }
}