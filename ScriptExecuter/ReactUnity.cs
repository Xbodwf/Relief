using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime.Interop;
using UnityEngine;

namespace ScriptExecuter
{
    public class ReactUnity
    {
        private readonly Engine _engine;
        private readonly UnityBridge _unityBridge;

        public ReactUnity(Engine engine, UnityBridge unityBridge)
        {
            _engine = engine;
            _unityBridge = unityBridge;
            RegisterMethods();
        }

        private void RegisterMethods()
        {
            _engine.SetValue("ReactUnity", this);
        }

        public void SetStage(JsValue stageElement)
        {
            // TODO: Implement stage setting logic here.
            // This will likely involve converting stageElement to a Unity GameObject
            // and setting it as the active stage or parent for other elements.
            Debug.Log($"SetStage called with: {stageElement}");
        }

        public JsValue UseState(JsValue initialState)
        {
            // TODO: Implement useState logic
            Debug.Log($"UseState called with: {initialState}");
            return JsValue.Undefined;
        }

        public JsValue UseEffect(JsValue effect, JsValue dependencies)
        {
            // TODO: Implement useEffect logic
            Debug.Log($"UseEffect called with: {effect}, {dependencies}");
            return JsValue.Undefined;
        }

        public class Root
        {
            private readonly ReactUnity _reactUnity;
            private readonly GameObject _container;

            public Root(ReactUnity reactUnity, GameObject container)
            {
                _reactUnity = reactUnity;
                _container = container;
            }

            public JsValue render(JsValue element) {
                _reactUnity.UpdateRenderElement(element, _container);
                return JsValue.Undefined;
            }
        }

        public Root CreateRoot(GameObject container)
        {
            return new Root(this, container);
        }

        private void UpdateRenderElement(JsValue element, GameObject container)
        {
            // This is a simplified render function. 
            // A full implementation would involve diffing and updating the Unity scene graph.
            // For now, we'll just create the GameObject and attach it to the container.
            
            if (element.IsObject() && element.AsObject().HasProperty("gameObject"))
            {
                string gameObjectId = element.AsObject().Get("gameObject").AsString();
                GameObject newGameObject = _unityBridge.GetGameObject(gameObjectId);
                if (newGameObject != null) {
                    newGameObject.transform.SetParent(container.transform, false);
                }
            }
            else if (element.IsArray()) // Handle fragments
            {
                var elements = element.AsArray();
                foreach (var item in elements)
                {
                    UpdateRenderElement(item, container);
                }
            }
        }
    }
}