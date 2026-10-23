using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using Wobble.Save;

namespace Wobble.UI
{
    // Thin adapter over official String Tables; contains no translation dictionary.
    public sealed class MenuLocalization : MonoBehaviour
    {
        public event Action Changed;
        public bool Ready { get; private set; }
        private SaveManager save;
        public string LocaleCode => save.Data.selectedLocale;

        public void Initialize(SaveManager save)
        {
            this.save = save;
            StartCoroutine(Load());
        }

        private IEnumerator Load()
        {
            yield return LocalizationSettings.InitializationOperation;
            if (LocalizationSettings.InitializationOperation.Status != AsyncOperationStatus.Succeeded)
            { Debug.LogError("WOBBLE localization initialization failed."); yield break; }
            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            {
                var table=LocalizationSettings.StringDatabase.GetTableAsync("Wobble", locale);
                yield return table;
                if(table.Status!=AsyncOperationStatus.Succeeded || table.Result==null)
                {Debug.LogError("WOBBLE String Table unavailable: "+locale.Identifier.Code);yield break;}
            }
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(save.Data.selectedLocale);
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            Ready = true;
            Changed?.Invoke();
        }

        public void Select(string code)
        {
            if (!Ready || (code != "tr" && code != "en")) return;
            save.Data.selectedLocale = code;
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(code);
            save.Save();
            Changed?.Invoke();
        }

        public string Text(string key, params object[] args) => Ready
            ? LocalizationSettings.StringDatabase.GetLocalizedString("Wobble", key, arguments: args)
            : string.Empty;
        private void OnLocaleChanged(Locale locale) => Changed?.Invoke();
        private void OnDestroy() { if (Ready) LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged; }
    }
}
