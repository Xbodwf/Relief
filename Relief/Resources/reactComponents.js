const UnityComponents = {
    // 基础组件工厂
    createComponent(type, props) {
        return {
            $$typeof: Symbol.for('react.element'),
            type,
            props: props || {},
            key: props?.key || null
        };
    },

    // GameObject 基础组件
    GameObject(props) {
        const go = new UnityEngine.GameObject(props.name || 'GameObject');
        
        // 应用变换
        if (props.position) {
            go.transform.position = new UnityEngine.Vector3(
                props.position.x || 0,
                props.position.y || 0,
                props.position.z || 0
            );
        }
        if (props.rotation) {
            go.transform.eulerAngles = new UnityEngine.Vector3(
                props.rotation.x || 0,
                props.rotation.y || 0,
                props.rotation.z || 0
            );
        }
        if (props.scale) {
            go.transform.localScale = new UnityEngine.Vector3(
                props.scale.x || 1,
                props.scale.y || 1,
                props.scale.z || 1
            );
        }

        // 添加组件
        if (props.components) {
            props.components.forEach(component => {
                go.AddComponent(component);
            });
        }

        return go;
    },

    // UI组件
    Canvas(props) {
        const canvas = this.GameObject({
            name: props.name || 'Canvas',
            ...props
        });

        // 添加Canvas组件
        const canvasComponent = canvas.AddComponent(UnityEngine.Canvas);
        canvasComponent.renderMode = props.renderMode || UnityEngine.RenderMode.ScreenSpaceOverlay;

        // 添加CanvasScaler
        const scaler = canvas.AddComponent(UnityEngine.UI.CanvasScaler);
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new UnityEngine.Vector2(1920, 1080);

        // 添加GraphicRaycaster
        canvas.AddComponent(UnityEngine.UI.GraphicRaycaster);

        return canvas;
    },

    Text(props) {
        const textObj = this.GameObject({
            name: props.name || 'Text',
            ...props
        });

        const text = textObj.AddComponent(UnityEngine.UI.Text);
        text.text = props.text || '';
        text.fontSize = props.fontSize || 24;
        text.color = props.color || new UnityEngine.Color(1, 1, 1, 1);
        text.alignment = props.alignment || UnityEngine.TextAnchor.MiddleCenter;
        text.font = UnityEngine.Resources.GetBuiltinResource(UnityEngine.Font, "Arial");

        // 设置RectTransform
        const rect = text.GetComponent(UnityEngine.RectTransform);
        if (props.width) rect.sizeDelta = new UnityEngine.Vector2(props.width, rect.sizeDelta.y);
        if (props.height) rect.sizeDelta = new UnityEngine.Vector2(rect.sizeDelta.x, props.height);

        return textObj;
    },

    // TextMeshPro UI 组件
    TextMeshProUGUI(props) {
        const textObj = this.GameObject({
            name: props.name || 'TextMeshPro',
            ...props
        });

        const text = textObj.AddComponent(TMPro.TextMeshProUGUI);
        text.text = props.text || '';
        text.fontSize = props.fontSize || 24;
        text.color = props.color || new UnityEngine.Color(1, 1, 1, 1);
        text.alignment = props.alignment || TMPro.TextAlignmentOptions.Center;
        text.font = props.font || TMPro.TMP_Settings.defaultFontAsset;
        text.enableAutoSizing = props.autoSize || false;
        
        if (props.autoSize) {
            text.fontSizeMin = props.minFontSize || 10;
            text.fontSizeMax = props.maxFontSize || 100;
        }

        // 设置富文本
        text.richText = props.richText !== false;
        
        // 设置超链接
        if (props.onLinkClick) {
            text.isLinksEnabled = true;
            textObj.AddComponent(TMPro.TMP_TextEventHandler)
                .onLinkSelection.AddListener(new UnityEngine.Events.UnityAction$1(
                    TMPro.TMP_LinkInfo
                )(props.onLinkClick));
        }

        // 设置文本溢出模式
        text.overflowMode = props.overflow || TMPro.TextOverflowModes.Overflow;

        // 设置换行模式
        text.enableWordWrapping = props.wordWrap !== false;

        // 设置RectTransform
        const rect = text.GetComponent(UnityEngine.RectTransform);
        if (props.width) rect.sizeDelta = new UnityEngine.Vector2(props.width, rect.sizeDelta.y);
        if (props.height) rect.sizeDelta = new UnityEngine.Vector2(rect.sizeDelta.x, props.height);

        return textObj;
    },

    // TextMeshPro 3D 组件
    TextMeshPro(props) {
        const textObj = this.GameObject({
            name: props.name || 'TextMeshPro3D',
            ...props
        });

        const text = textObj.AddComponent(TMPro.TextMeshPro);
        text.text = props.text || '';
        text.fontSize = props.fontSize || 24;
        text.color = props.color || new UnityEngine.Color(1, 1, 1, 1);
        text.alignment = props.alignment || TMPro.TextAlignmentOptions.Center;
        text.font = props.font || TMPro.TMP_Settings.defaultFontAsset;
        
        // 3D 文本特有属性
        text.autoSizeTextContainer = props.autoSizeContainer !== false;
        text.enableCulling = props.enableCulling !== false;
        
        // 材质设置
        if (props.material) {
            text.material = props.material;
        }
        
        // 文本外观
        text.outlineWidth = props.outlineWidth || 0;
        text.outlineColor = props.outlineColor || new UnityEngine.Color(0, 0, 0, 1);
        
        // 阴影设置
        if (props.enableShadow) {
            text.enableVertexGradient = true;
            text.shadowColor = props.shadowColor || new UnityEngine.Color(0, 0, 0, 0.5);
            text.shadowOffset = props.shadowOffset || new UnityEngine.Vector2(1, -1);
        }

        return textObj;
    },

    Button(props) {
        const buttonObj = this.GameObject({
            name: props.name || 'Button',
            ...props
        });

        const button = buttonObj.AddComponent(UnityEngine.UI.Button);
        const image = buttonObj.AddComponent(UnityEngine.UI.Image);
        
        // 使用 TextMeshPro 替代原有 Text
        const textObj = this.TextMeshProUGUI({
            name: 'ButtonText',
            text: props.text || 'Button',
            fontSize: props.fontSize || 24,
            color: props.textColor || new UnityEngine.Color(0, 0, 0, 1),
            alignment: TMPro.TextAlignmentOptions.Center
        });
        textObj.transform.SetParent(buttonObj.transform, false);

        if (props.onClick) {
            button.onClick.AddListener(new UnityEngine.Events.UnityAction(props.onClick));
        }

        return buttonObj;
    },

    // 3D基础组件
    Cube(props) {
        const cube = this.GameObject({
            name: props.name || 'Cube',
            ...props
        });

        // 添加网格过滤器和渲染器
        const meshFilter = cube.AddComponent(UnityEngine.MeshFilter);
        meshFilter.mesh = UnityEngine.Resources.GetBuiltinResource(UnityEngine.Mesh, "Cube");

        const renderer = cube.AddComponent(UnityEngine.MeshRenderer);
        if (props.material) {
            renderer.material = props.material;
        } else {
            renderer.material = new UnityEngine.Material(UnityEngine.Shader.Find('Standard'));
        }

        // 添加碰撞器
        if (props.collider !== false) {
            cube.AddComponent(UnityEngine.BoxCollider);
        }

        return cube;
    },

    Sphere(props) {
        const sphere = this.GameObject({
            name: props.name || 'Sphere',
            ...props
        });

        // 添加网格过滤器和渲染器
        const meshFilter = sphere.AddComponent(UnityEngine.MeshFilter);
        meshFilter.mesh = UnityEngine.Resources.GetBuiltinResource(UnityEngine.Mesh, "Sphere");

        const renderer = sphere.AddComponent(UnityEngine.MeshRenderer);
        if (props.material) {
            renderer.material = props.material;
        } else {
            renderer.material = new UnityEngine.Material(UnityEngine.Shader.Find('Standard'));
        }

        // 添加碰撞器
        if (props.collider !== false) {
            sphere.AddComponent(UnityEngine.SphereCollider);
        }

        return sphere;
    },

    // 相机组件
    Camera(props) {
        const cameraObj = this.GameObject({
            name: props.name || 'Camera',
            ...props
        });

        const camera = cameraObj.AddComponent(UnityEngine.Camera);
        camera.clearFlags = props.clearFlags || UnityEngine.CameraClearFlags.Skybox;
        camera.backgroundColor = props.backgroundColor || new UnityEngine.Color(0.19, 0.19, 0.19, 1);
        camera.fieldOfView = props.fieldOfView || 60;
        camera.nearClipPlane = props.near || 0.3;
        camera.farClipPlane = props.far || 1000;

        return cameraObj;
    },

    // 灯光组件
    Light(props) {
        const lightObj = this.GameObject({
            name: props.name || 'Light',
            ...props
        });

        const light = lightObj.AddComponent(UnityEngine.Light);
        light.type = props.type || UnityEngine.LightType.Directional;
        light.intensity = props.intensity || 1;
        light.color = props.color || new UnityEngine.Color(1, 1, 1, 1);
        light.shadows = props.shadows || UnityEngine.LightShadows.Soft;

        return lightObj;
    }
};

export default UnityComponents;
export { UnityComponents };