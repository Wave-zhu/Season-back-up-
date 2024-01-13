using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Season.BattleSubSystem;
using Season.SceneBehaviors;
using Season.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Season.AssetsSubSystem
{
    public class UiSubSystem : MonoBehaviour
    {
        protected static Canvas Canvas;
        protected static RectTransform CanvasRectTransform;
        protected static Transform CanvasTransform;

        public void SwitchRenderCamera(UnityEngine.Camera renderCamera)
        {
            Canvas.worldCamera = renderCamera;
        }
        public RectTransform GetCanvasRectTransform() => CanvasRectTransform;
        public Transform GetCanvasTransform() => CanvasTransform;
        
        private Dictionary<string, CustomWidget> _cache;
        private HashSet<string> _currentWidgets;
        private string GetKeyByEnum<T>(T enumType) where T : struct, Enum
        {
            return typeof(T).ToString() + enumType;
        }

        public GameObject GetCache<T>(T enumType, SceneBehavior scene) where T : struct, Enum
        {
            string key = GetKeyByEnum(enumType);
            return _cache.TryGetValue(key, out var customWidget) ? customWidget.widget : null;
        }

        private async Task RegisterWidget<T>(T enumType) where T : struct, Enum
        {
            var key = GetKeyByEnum(enumType);
            if (_cache.ContainsKey(key)) return;
            var asset = await Addressables.LoadAssetAsync<GameObject>(SceneAssets.MainInstance.SceneBehavior.GetAssetByEnum(enumType)).Task;
            var widget = Instantiate(asset, CanvasTransform);
            Addressables.Release(asset);
            widget.SetActive(false);
            var defaultItem = widget.GetComponent<IInitWidget>();
            if (defaultItem != null)
            {
                defaultItem.InitializeUiItem();
            }
            _cache[key] = new CustomWidget()
            {
                widget = widget,
            };
            var tickItem = widget.GetComponent<ITickWhenOpen>();
            if (tickItem != null)
            {
                _cache[key].tick = tickItem.TickWhenOpen;
            }
        }
        
        private async Task TryOpenWidget<T>(T enumType) where T : struct, Enum
        {
            string key = GetKeyByEnum(enumType);
            await RegisterWidget(enumType);
            _cache[key].widget.SetActive(true);
            _cache[key].tick?.Invoke();
            _currentWidgets.Add(key);
        }

        private void TryDeactivateWidget<T>(T enumType) where T : struct, Enum
        {
            string key = GetKeyByEnum(enumType);
            if (_cache.ContainsKey(key))
            {
                _cache[key].widget.SetActive(false);
            }
            _currentWidgets.Remove(key);
        }
        private void TryCloseWidget<T>(T enumType) where T : struct, Enum
        {
            string key = GetKeyByEnum(enumType);
            if (_cache.ContainsKey(key)) {
                _cache[key].tick = null;
                Destroy(_cache[key].widget);
                _cache.Remove(key);
            }
            _currentWidgets.Remove(key);
        }

        public async Task OpenWidget<T>(T enumType) where T : struct, Enum
        {
            int widgets = (int)(object)enumType;
            while (widgets > 0) {
                T widget = (T)Enum.ToObject(typeof(T), widgets & -widgets);
                await TryOpenWidget(widget);
                widgets &= widgets - 1;
            }
        }

        protected void ModifyUiLayer<T>(T enumType, int i) where T : struct, Enum
        {
            string key = GetKeyByEnum(enumType);
            _cache.TryGetValue(key, out var widget);
            widget?.widget.transform.SetSiblingIndex(i);
        }
        public void Deactivated<T>(T enumType) where T : struct, Enum
        {
            int widgets = (int)(object)enumType;
            while (widgets > 0) {
                T widget = (T)Enum.ToObject(typeof(T), widgets & -widgets);
                TryDeactivateWidget(widget);
                widgets &= widgets - 1;
            }
        }

        public void SuspendWidgets()
        {
            foreach (var key in _currentWidgets)
            {
                _cache[key].widget.SetActive(false);
            }
        }

        public void ReenableWidgets()
        {
            foreach (var key in _currentWidgets)
            {
                _cache[key].widget.SetActive(true);
            }
        }

        public void Close<T>(T enumType) where T : struct, Enum
        {
            int widgets = (int)(object)enumType;
            while (widgets > 0) {
                T widget = (T)Enum.ToObject(typeof(T), widgets & -widgets);
                TryCloseWidget(widget);
                widgets &= widgets - 1;
            }
        }

        public void CloseAll()
        {
            foreach (var entry in _cache)
            {
                if(entry.Value.widget)
                    Destroy(entry.Value.widget);
            }
            _cache.Clear();
        }

        #region UiAnimation

        public static IEnumerator FadeIn(CanvasGroup group, float alpha, float duration)
        {
            var time = 0.0f;
            var originalAlpha = group.alpha;
            while (time < duration)
            {
                time += Time.deltaTime;
                group.alpha = Mathf.Lerp(originalAlpha, alpha, time / duration);
                yield return new WaitForEndOfFrame();
            }

            group.alpha = alpha;
        }
        public static IEnumerator FadeOut(CanvasGroup group, float alpha, float duration)
        {
            var time = 0.0f;
            var originalAlpha = group.alpha;
            while (time < duration)
            {
                time += Time.deltaTime;
                group.alpha = Mathf.Lerp(originalAlpha, alpha, time / duration);
                yield return new WaitForEndOfFrame();
            }

            group.alpha = alpha;
        }

        #endregion

        protected virtual void Awake()
        {
            _cache = new();
            _currentWidgets = new();
            if (Canvas) return;
            Canvas = FindObjectOfType<Canvas>();
            CanvasRectTransform = Canvas.GetComponent<RectTransform>();
            CanvasTransform = Canvas.transform;
        }
    }
}