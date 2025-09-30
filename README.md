# ScriptExecuter

> A Javascript/TypeScript for Games with UnityModManager (like ADOFAI)

## Features

* Support Javascript/TypeScript

We use tsc(typescript compiler) to compile ts to js file 
and use Jint to run js file in Unity.

It's safe, easy and fast.

* Internal Modules

```typescript
import * as fs from 'fs';
import * as path from 'path';

export default function EndlessJourneyLoader(id:string, name:string) {
    console.log(`Welcome to ${name}[${id}] Loader`);

    const statsPath = path.join(`./${name}`, "stats.json");
    
    if (!fs.existsSync(`./${name}`)) {
        fs.mkdirSync(`./${name}`, { recursive: true });
    }


    try {
        let stats = {
            startCount: 0,
            firstStart: new Date().toISOString(),
            lastStart: new Date().toISOString()
        };

        if (fs.existsSync(statsPath)) {
            const existingData = fs.readFileSync(statsPath);
            stats = JSON.parse(existingData);
        }

        stats.startCount += 1;
        stats.lastStart = new Date().toISOString();

        fs.writeFileSync(statsPath, JSON.stringify(stats, null, 2), 'utf8');
        console.log(`这是你第 ${stats.startCount} 次打开游戏`);
        console.log(`上一次启动时间:  ${new Date(stats.lastStart).toLocaleString()}`);

    } catch (err) {
        console.error("Failed to update start stats:", err);
    }
}
```
(An easy template for a mod loader)

* Support Javascript JSX
```typescript
import React from 'react'; //internal module "react" for jsx using
import {createRoot} from 'react-unity';

function App() {
  return <>
    <div>
      Hello World
    </div>
  </>;
}
export default function Loader(id:string, name:string) {
  createRoot(UnityBridge.querySelector('Trail')).render(<App />);
}
```
JSX will be compiled to `React.createElement`,but `React.createElement` will generate `UnityEngine.GameObject` instead of `JSX.Element`

Of course,You can create GameObject directly:

```typescript
const go = GameObject.FindWithTag('t1');
```

