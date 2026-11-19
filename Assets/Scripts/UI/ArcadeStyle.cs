using TMPro;
using UnityEngine;
using Wobble.Gameplay;

namespace Wobble.UI
{
    // Bright Architectural Arcade: scoped to arcade presentation, never the five world themes.
    public static class ArcadeStyle
    {
        public static readonly Color Mist=Hex(0xE7F5F7), Cream=Hex(0xFFF7E8), Ink=Hex(0x152B3F);
        public static readonly Color Coral=Hex(0xFF765C), Gold=Hex(0xF7C96D), Aqua=Hex(0x48B4CE), Violet=Hex(0x7B70D6);
        public static Color Accent(ArcadeMode mode)=>mode==ArcadeMode.Rush?Coral:mode==ArcadeMode.Balance?Violet:mode==ArcadeMode.Trials?Gold:Aqua;
        private static Color Hex(uint c)=>new Color((c>>16&255)/255f,(c>>8&255)/255f,(c&255)/255f);
        public static RectTransform Rect(string name,Transform parent,Vector2 pos,Vector2 size)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;return r;
        }
        public static UnityEngine.UI.Image Surface(string name,Transform parent,Vector2 pos,Vector2 size,Color color,bool rounded=true)
        {
            var r=Rect(name,parent,pos,size);var image=r.gameObject.AddComponent<UnityEngine.UI.Image>();image.color=color;image.raycastTarget=false;
            if(rounded)CasualSurface.Style(image,false);return image;
        }
        public static TextMeshProUGUI Text(string name,Transform parent,string value,Vector2 pos,Vector2 size,float points,bool display=true)
        {
            var r=Rect(name,parent,pos,size);var t=r.gameObject.AddComponent<TextMeshProUGUI>();
            t.font=Resources.Load<TMP_FontAsset>(display?"Menu/WobbleDisplay":"Menu/WobbleBody");
            t.text=value;t.fontSize=points;t.color=Ink;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.enableAutoSizing=false;return t;
        }
        public static UnityEngine.UI.Button Button(string name,Transform parent,string text,Vector2 pos,Vector2 size,Color color,UnityEngine.Events.UnityAction action)
        {
            var image=Surface(name,parent,pos,size,color);image.raycastTarget=true;
            var b=image.gameObject.AddComponent<UnityEngine.UI.Button>();b.onClick.AddListener(action);b.gameObject.AddComponent<StoneButtonFeedback>();
            var colors=b.colors;colors.highlightedColor=Color.white;colors.pressedColor=new Color(.88f,.88f,.88f);colors.disabledColor=new Color(.65f,.65f,.65f,.55f);b.colors=colors;
            Text(name+" label",b.transform,text,Vector2.zero,size-new Vector2(32,16),38);return b;
        }
        public static void Fit(RectTransform content,Canvas canvas)
        {
            var safe=Screen.safeArea;var bounds=((RectTransform)canvas.transform).rect;
            if(Screen.width<=0||Screen.height<=0)return;
            content.anchorMin=content.anchorMax=new Vector2(safe.center.x/Screen.width,safe.center.y/Screen.height);
            content.localScale=Vector3.one*Mathf.Min(bounds.width*safe.width/Screen.width/1080f,bounds.height*safe.height/Screen.height/1900f);
        }
        public static float Ease(float t)=>1-Mathf.Pow(1-Mathf.Clamp01(t),3);
    }
}
