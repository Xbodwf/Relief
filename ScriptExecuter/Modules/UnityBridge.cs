using System;
using System.Collections.Generic;
using UnityEngine;
using Jint;
using Jint.Native;

namespace ScriptExecuter
{
    /// <summary>
    /// UnityBridge类用于连接JavaScript和Unity，实现伪React功能
    /// </summary>
    public class UnityBridge
    {
        private Engine engine;
        private Dictionary<string, GameObject> gameObjectCache = new Dictionary<string, GameObject>();

        public UnityBridge(Engine jsEngine)
        {
            engine = jsEngine;
            RegisterMethods();
        }

        /// <summary>
        /// 注册JavaScript可调用的方法
        /// </summary>
        private void RegisterMethods()
        {
            engine.SetValue("UnityBridge", this);
        }

        /// <summary>
        /// 创建GameObject
        /// </summary>
        /// <param name="tag">GameObject的名称</param>
        /// <returns>创建的GameObject的ID</returns>
        public string CreateGameObject(string tag)
        {
            try
            {
                GameObject gameObject = new GameObject(tag);
                string id = Guid.NewGuid().ToString();
                gameObjectCache[id] = gameObject;
                return id;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating GameObject: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 设置GameObject的属性
        /// </summary>
        /// <param name="gameObjectId">GameObject的ID</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="propertyValue">属性值</param>
        /// <returns>是否设置成功</returns>
        public bool SetGameObjectProperty(string gameObjectId, string propertyName, object propertyValue)
        {
            try
            {
                if (!gameObjectCache.TryGetValue(gameObjectId, out GameObject gameObject))
                {
                    Debug.LogError($"GameObject with ID {gameObjectId} not found");
                    return false;
                }

                switch (propertyName.ToLower())
                {
                    case "position":
                        if (propertyValue is JsValue jsPosition)
                        {
                            var x = jsPosition.AsObject().Get("x").AsNumber();
                            var y = jsPosition.AsObject().Get("y").AsNumber();
                            var z = jsPosition.AsObject().Get("z").AsNumber();
                            gameObject.transform.position = new Vector3((float)x, (float)y, (float)z);
                        }
                        break;
                    case "rotation":
                        if (propertyValue is JsValue jsRotation)
                        {
                            var x = jsRotation.AsObject().Get("x").AsNumber();
                            var y = jsRotation.AsObject().Get("y").AsNumber();
                            var z = jsRotation.AsObject().Get("z").AsNumber();
                            gameObject.transform.rotation = Quaternion.Euler((float)x, (float)y, (float)z);
                        }
                        break;
                    case "scale":
                        if (propertyValue is JsValue jsScale)
                        {
                            var x = jsScale.AsObject().Get("x").AsNumber();
                            var y = jsScale.AsObject().Get("y").AsNumber();
                            var z = jsScale.AsObject().Get("z").AsNumber();
                            gameObject.transform.localScale = new Vector3((float)x, (float)y, (float)z);
                        }
                        break;
                    case "active":
                        if (propertyValue is JsValue jsActive)
                        {
                            gameObject.SetActive(jsActive.AsBoolean());
                        }
                        break;
                    case "tag":
                        if (propertyValue is JsValue jsTag)
                        {
                            gameObject.tag = jsTag.AsString();
                        }
                        break;
                    case "name":
                        if (propertyValue is JsValue jsName)
                        {
                            gameObject.name = jsName.AsString();
                        }
                        break;
                    default:
                        // 处理自定义组件属性
                        SetCustomProperty(gameObject, propertyName, propertyValue);
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting GameObject property: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 设置自定义属性
        /// </summary>
        private void SetCustomProperty(GameObject gameObject, string propertyName, object propertyValue)
        {
            // 这里可以根据需要扩展，添加更多自定义组件和属性的处理
            if (propertyName.StartsWith("component."))
            {
                string[] parts = propertyName.Split('.');
                if (parts.Length >= 2)
                {
                    string componentName = parts[1];
                    // 添加组件
                    switch (componentName.ToLower())
                    {
                        case "rigidbody":
                            gameObject.AddComponent<Rigidbody>();
                            break;
                        case "boxcollider":
                            gameObject.AddComponent<BoxCollider>();
                            break;
                        // 可以添加更多组件类型
                    }
                }
            }
        }

        /// <summary>
        /// 设置父子关系
        /// </summary>
        /// <param name="childId">子GameObject的ID</param>
        /// <param name="parentId">父GameObject的ID</param>
        /// <returns>是否设置成功</returns>
        public bool SetParent(string childId, string parentId)
        {
            try
            {
                if (!gameObjectCache.TryGetValue(childId, out GameObject childObject))
                {
                    Debug.LogError($"Child GameObject with ID {childId} not found");
                    return false;
                }

                if (!gameObjectCache.TryGetValue(parentId, out GameObject parentObject))
                {
                    Debug.LogError($"Parent GameObject with ID {parentId} not found");
                    return false;
                }

                childObject.transform.SetParent(parentObject.transform, false);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting parent: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get GameObject
        /// </summary>
        /// <param name="gameObjectId">ID of GameObject</param>
        
        public GameObject GetGameObject(string gameObjectId)
        {
            if (gameObjectCache.TryGetValue(gameObjectId, out GameObject gameObject))
            {
                return gameObject;
            }
            return null;
        }

        public GameObject[] getGameObjects()
        {
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            return allObjects;
        }

        public GameObject querySelector(string query)
        {
            return GameObject.Find(query);
            
        }

        public GameObject getGameObjectByTag(string tag)
        {
            return GameObject.FindWithTag(tag); 
        }

        /// <summary>
        /// 销毁GameObject
        /// </summary>
        /// <param name="gameObjectId">ID of GameObject</param>
        public bool DestroyGameObject(string gameObjectId)
        {
            try
            {
                if (!gameObjectCache.TryGetValue(gameObjectId, out GameObject gameObject))
                {
                    Debug.LogError($"GameObject with ID {gameObjectId} not found");
                    return false;
                }

                GameObject.Destroy(gameObject);
                gameObjectCache.Remove(gameObjectId);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error destroying GameObject: {ex.Message}");
                return false;
            }
        }
    }
}