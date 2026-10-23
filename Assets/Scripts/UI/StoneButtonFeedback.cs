using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Wobble.UI
{
    // Event-driven spring only while interacting; no idle Update or gameplay input ownership.
    public sealed class StoneButtonFeedback : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler
    {
        public void OnPointerDown(PointerEventData e){if(GetComponent<Selectable>().IsInteractable())Animate(.965f);}
        public void OnPointerUp(PointerEventData e)=>Animate(1);
        public void OnPointerExit(PointerEventData e)=>Animate(1);
        private void Animate(float scale){StopAllCoroutines();if(isActiveAndEnabled)StartCoroutine(Spring(scale));}
        private System.Collections.IEnumerator Spring(float scale)
        {
            var start=transform.localScale;
            for(var t=0f;t<.12f;t+=Time.unscaledDeltaTime){transform.localScale=Vector3.Lerp(start,Vector3.one*scale,Mathf.SmoothStep(0,1,t/.12f));yield return null;}
            transform.localScale=Vector3.one*scale;
        }
        private void OnDisable(){StopAllCoroutines();transform.localScale=Vector3.one;}
    }
}
