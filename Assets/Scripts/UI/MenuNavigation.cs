using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wobble.UI
{
    public enum MenuScreen { Home, Themes, Shop, Daily, Settings }
    public sealed class MenuNavigation : MonoBehaviour
    {
        private readonly Dictionary<MenuScreen, CanvasGroup> screens = new();
        private Coroutine transition;
        public MenuScreen Current { get; private set; }
        public void Register(MenuScreen id, GameObject screen) => screens.Add(id, screen.AddComponent<CanvasGroup>());
        public void Show(MenuScreen id, bool immediate = false)
        {
            if (transition != null) StopCoroutine(transition);
            var outgoing=screens.TryGetValue(Current,out var previous)&&previous.gameObject.activeSelf?previous:null;
            Current = id;
            foreach (var pair in screens)
            {
                pair.Value.interactable = pair.Value.blocksRaycasts = false;
                pair.Value.gameObject.SetActive(pair.Key == id||(!immediate&&pair.Value==outgoing));
            }
            var active = screens[id];
            active.transform.localScale = Vector3.one;
            if (immediate) { active.alpha = 1; ((RectTransform)active.transform).anchoredPosition=Vector2.zero; active.interactable = active.blocksRaycasts = true; }
            else transition = StartCoroutine(Reveal(active,outgoing==active?null:outgoing));
        }
        private IEnumerator Reveal(CanvasGroup group,CanvasGroup outgoing)
        {
            for (var t = 0f; t < .24f; t += Time.unscaledDeltaTime)
            {
                var ease = Mathf.SmoothStep(0, 1, t / .24f);
                group.alpha = ease;
                ((RectTransform)group.transform).anchoredPosition=new Vector2(0,Mathf.Lerp(-18,0,ease));
                if(outgoing!=null)outgoing.alpha=1-ease;
                yield return null;
            }
            group.alpha = 1;
            group.transform.localScale = Vector3.one;
            ((RectTransform)group.transform).anchoredPosition=Vector2.zero;
            if(outgoing!=null)outgoing.gameObject.SetActive(false);
            group.interactable = group.blocksRaycasts = true;
            transition = null;
        }
    }
}
