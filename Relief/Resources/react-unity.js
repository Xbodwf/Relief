import * as React from 'react';

const ReactUnity = {
    // Unity场景根节点
    _root: null,
    _container: null,
    _fiberRoot: null,

    createRoot(container) {
        if (this._root) {
            throw new Error('只能创建一个Root实例');
        }

        // 创建根GameObject
        this._container = new UnityEngine.GameObject('ReactRoot');
        UnityEngine.Object.DontDestroyOnLoad(this._container);

        this._root = {
            render: (element) => {
                this._render(element);
            },
            unmount: () => {
                this._unmount();
            }
        };

        return this._root;
    },

    _render(element) {
        // 清理旧的渲染
        if(this._fiberRoot) {
            this._unmount();
        }

        // 创建新的渲染树
        this._fiberRoot = this._createFiberRoot(element);
        this._updateContainer(element);
    },

    _unmount() {
        if(this._fiberRoot) {
            // 清理所有GameObject
            this._destroyGameObjects(this._fiberRoot);
            this._fiberRoot = null;
        }
    },

    _createFiberRoot(element) {
        // 创建Fiber树结构
        return {
            current: {
                type: element.type,
                props: element.props,
                stateNode: null,
                child: null,
                sibling: null,
                return: null
            }
        };
    },

    _updateContainer(element) {
        // 递归创建GameObject
        this._reconcileChildren(this._fiberRoot.current, element);
    },

    _reconcileChildren(parentFiber, element) {
        if (!element) return;

        const gameObject = this._createGameObject(element, parentFiber);
        
        if (element.props && element.props.children) {
            React.Children.forEach(element.props.children, child => {
                this._reconcileChildren(gameObject, child);
            });
        }
    },

    _createGameObject(element, parentFiber) {
        const go = new UnityEngine.GameObject(element.type.name || 'GameObject');
        
        // 设置父级
        go.transform.SetParent(
            parentFiber.stateNode ? 
            parentFiber.stateNode.transform : 
            this._container.transform
        );

        // 处理组件属性
        if (element.props) {
            this._applyProps(go, element.props);
        }

        return go;
    },

    _applyProps(gameObject, props) {
        // 处理位置
        if (props.position) {
            gameObject.transform.position = new UnityEngine.Vector3(
                props.position.x || 0,
                props.position.y || 0,
                props.position.z || 0
            );
        }

        // 处理旋转
        if (props.rotation) {
            gameObject.transform.eulerAngles = new UnityEngine.Vector3(
                props.rotation.x || 0,
                props.rotation.y || 0,
                props.rotation.z || 0
            );
        }

        // 处理缩放
        if (props.scale) {
            gameObject.transform.localScale = new UnityEngine.Vector3(
                props.scale.x || 1,
                props.scale.y || 1,
                props.scale.z || 1
            );
        }

        // 添加组件
        if (props.components) {
            props.components.forEach(componentType => {
                gameObject.AddComponent(componentType);
            });
        }
    },

    _destroyGameObjects(fiber) {
        if (!fiber) return;

        // 递归销毁所有GameObject
        if (fiber.stateNode) {
            UnityEngine.Object.Destroy(fiber.stateNode);
        }

        this._destroyGameObjects(fiber.child);
        this._destroyGameObjects(fiber.sibling);
    }
};


export default ReactUnity;
export const createRoot = ReactUnity.createRoot;