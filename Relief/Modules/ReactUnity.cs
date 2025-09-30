using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime.Interop;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Xml;
using Relief.Modules.vm;

namespace Relief
{
    public class ReactUnity
    {
        private readonly Engine _engine;
        private readonly UnityBridge _unityBridge;
        private readonly ReactState _reactState;
        private readonly ReactComponent _reactComponent;
        private readonly VirtualDOM _virtualDOM;
        private readonly Dictionary<string, Root> _roots = new Dictionary<string, Root>();

        public ReactUnity(Engine engine, UnityBridge unityBridge)
        {
            _engine = engine;
            _unityBridge = unityBridge;
            _reactState = new ReactState(engine);
            _reactComponent = new ReactComponent(engine, _reactState);
            _virtualDOM = new VirtualDOM(engine, unityBridge);
            RegisterMethods();
        }

        private void RegisterMethods()
        {
            _engine.SetValue("ReactUnity", this);
        }

        public void SetStage(JsValue stageElement)
        {
            try
            {
                if (stageElement.IsObject() && stageElement.AsObject().HasProperty("gameObject"))
                {
                    string gameObjectId = stageElement.AsObject().Get("gameObject").AsString();
                    GameObject stageGameObject = _unityBridge.GetGameObject(gameObjectId);

                    if (stageGameObject != null)
                    {
                        // Set as main stage
                        Debug.Log($"Stage set to GameObject: {stageGameObject.name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting stage: {ex.Message}");
            }
        }

        public JsValue UseState(JsValue initialState)
        {
            return _reactState.UseState(initialState);
        }

        public JsValue UseEffect(JsValue effect, JsValue dependencies = null)
        {
            return _reactState.UseEffect(effect, dependencies);
        }

        public void RegisterComponent(string name, JsValue componentDefinition)
        {
            _reactComponent.RegisterComponentDefinition(name, componentDefinition);
        }

        public class Root
        {
            private readonly ReactUnity _reactUnity;
            private readonly GameObject _container;
            private readonly string _rootId;
            private VirtualDOM.VNode _currentVTree;

            public Root(ReactUnity reactUnity, GameObject container)
            {
                _reactUnity = reactUnity;
                _container = container;
                _rootId = Guid.NewGuid().ToString();
            }

            public JsValue render(JsValue element)
            {
                try
                {
                    // Create new Virtual DOM tree
                    var newVTree = _reactUnity._virtualDOM.CreateVNode(element);

                    // Reconcile with previous tree
                    _reactUnity._virtualDOM.Reconcile(newVTree, _currentVTree, _container);

                    // Update current tree
                    _currentVTree = newVTree;

                    // Process any pending state updates
                    _reactUnity._reactState.ProcessRenderQueue();

                    Debug.Log($"Rendered Virtual DOM tree:\n{_reactUnity._virtualDOM.GetVDOMTree(newVTree)}");

                    return JsValue.Undefined;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error during render: {ex.Message}");
                    return JsValue.Undefined;
                }
            }

            public string GetRootId() => _rootId;
            public GameObject GetContainer() => _container;
            public VirtualDOM.VNode GetCurrentVTree() => _currentVTree;
        }

        public Root CreateRoot(GameObject container)
        {
            var root = new Root(this, container);
            _roots[root.GetRootId()] = root;
            return root;
        }

        private void UpdateRenderElement(JsValue element, GameObject container)
        {
            try
            {
                if (element.IsUndefined() || element.IsNull())
                {
                    return;
                }

                // Handle functional components
                if (element.IsObject() && element.AsObject().HasProperty("type") &&
                    element.AsObject().Get("type").IsObject() &&
                    element.AsObject().Get("type").AsObject() is Jint.Native.Function.Function componentFunc)
                {
                    var props = element.AsObject().HasProperty("props") ?
                        element.AsObject().Get("props") : JsValue.Undefined;

                    // Create component instance with lifecycle support
                    var componentInstance = _reactComponent.CreateComponent(
                        componentFunc.ToString(),
                        props
                    );

                    if (componentInstance != null)
                    {
                        var renderedElement = _reactComponent.MountComponent(componentInstance);
                        UpdateRenderElement(renderedElement, container);
                    }
                    return;
                }

                // Handle regular elements
                if (element.IsObject() && element.AsObject().HasProperty("gameObject"))
                {
                    string gameObjectId = element.AsObject().Get("gameObject").AsString();
                    GameObject newGameObject = _unityBridge.GetGameObject(gameObjectId);
                    if (newGameObject != null)
                    {
                        newGameObject.transform.SetParent(container.transform, false);
                    }
                }
                else if (element.IsArray()) // Handle fragments and arrays
                {
                    var elements = element.AsArray();
                    for (int i = 0; i < elements.Length; i++)
                    {
                        var item = elements.Get(i.ToString());
                        UpdateRenderElement(item, container);
                    }
                }
                else if (element.IsString()) // Handle text nodes
                {
                    // Create a text GameObject for string content
                    string textContent = element.AsString();
                    GameObject textObject = new GameObject($"Text: {textContent}");
                    textObject.transform.SetParent(container.transform, false);

                    // You could add a TextMesh component here if needed
                    Debug.Log($"Rendered text: {textContent}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating render element: {ex.Message}");
            }
        }

        public ReactState GetReactState()
        {
            return _reactState;
        }

        public ReactComponent GetReactComponent()
        {
            return _reactComponent;
        }

        public VirtualDOM GetVirtualDOM()
        {
            return _virtualDOM;
        }

        public void Cleanup()
        {
            foreach (var root in _roots.Values)
            {
                if (root.GetContainer() != null)
                {
                    UnityEngine.Object.Destroy(root.GetContainer());
                }
            }
            _roots.Clear();

            _reactComponent.Cleanup();
            _virtualDOM.ClearCache();
        }
    }
}
