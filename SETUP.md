# GHOST SIGNAL - GUÍA DE CONFIGURACIÓN EN UNITY 2022.3 LTS

## Requisitos Previos
- Unity 2022.3 LTS o superior
- TextMesh Pro (TMP) - se descargará automáticamente al importar los scripts
- Paquete de Entrada Antigua (si deseas usar Input Manager tradicional)

---

## PASO 1: Crear Proyecto
1. Abre Unity Hub
2. Click en "New Project"
3. Selecciona "3D (Built-in Render Pipeline)"
4. Nombre: "GhostSignal"
5. Click en "Create project"

---

## PASO 2: Preparar la Carpeta de Scripts
1. En la carpeta `Assets`, crea carpeta `Scripts`
2. Copia los 6 archivos C# en `Assets/Scripts/`:
   - `PlayerController.cs`
   - `GameManager.cs`
   - `DialogueSystem.cs`
   - `TrackGenerator.cs`
   - `Guardians.cs`
   - `Systems.cs`

3. Espera a que Unity compile (visible en la esquina inferior derecha)

---

## PASO 3: Configurar Carpetas de Recursos
Crea estas carpetas en `Assets/`:
```
Assets/
├── Scripts/
├── Prefabs/
├── Materials/
├── Scenes/
├── Audio/
│   ├── Music/
│   └── Sfx/
└── Sprites/
    ├── Portraits/
    └── UI/
```

---

## PASO 4: Crear la Escena Principal
1. Click derecho en `Assets/Scenes` → Create → Scene
2. Nombre: "MainLevel"
3. Abre la scena haciendo doble click

---

## PASO 5: Crear GameObjects Principales

### 5.1 Crear Canvas (para UI)
1. Right-click en Hierarchy → UI → Canvas
2. Rename a "UICanvas"
3. En Canvas → Canvas Scaler
   - UI Scale Mode: "Scale With Screen Size"
   - Reference Resolution: (1920, 1080)

### 5.2 Crear GameManager
1. Right-click en Hierarchy → Create Empty
2. Rename a "GameManager"
3. Drag `GameManager.cs` al GameObject
4. Inspector → Add Component → Audio Source (x2)
   - Una para Music
   - Una para Ambiance

### 5.3 Crear UIManager
1. Right-click en Hierarchy → Create Empty
2. Rename a "UIManager"
3. Drag `UIManager.cs` al GameObject
4. Asigna referencias en el inspector:
   - Main Canvas: selecciona UICanvas
   - Otros campos quedan para después (se configuran dinámicamente)

### 5.4 Crear CameraController
1. Selecciona Main Camera en Hierarchy
2. Drag `CameraController.cs` al GameObject
3. En Inspector:
   - Main Camera: Auto-asignada
   - Smooth Speed: 10

### 5.5 Crear AudioManager
1. Right-click en Hierarchy → Create Empty
2. Rename a "AudioManager"
3. Drag `AudioManager.cs` al GameObject
4. Add Component → Audio Source (x2)
   - Uno para música
   - Uno para ambiance

### 5.6 Crear TrackGenerator
1. Right-click en Hierarchy → Create Empty
2. Rename a "TrackGenerator"
3. Drag `TrackGenerator.cs` al GameObject

---

## PASO 6: Crear Player

### 6.1 Crear GameObject del Jugador
1. Right-click en Hierarchy → 3D Object → Cube
2. Rename a "KAI"
3. Scale: (0.5, 2, 1) para forma de personaje
4. Position: (0, 1, 0)

### 6.2 Configurar Componentes
1. Delete el Mesh Collider por defecto
2. Add Component → Character Controller
   - Center: (0, 0.5, 0)
   - Radius: 0.25
   - Height: 2

3. Add Component → Collider (Sphere o Capsule, set como Trigger)
   - Para detectar colisiones con nodos

4. Add Component → Audio Source
   - Play On Awake: OFF

5. Drag `PlayerController.cs` al GameObject

### 6.3 Configurar Material de Circuitos
1. En `Assets/Materials`, Create → Material
2. Rename a "KAI_Circuits"
3. En Inspector:
   - Shader: "Standard"
   - Metallic: 0.8
   - Smoothness: 0.9
   - Emission: Habilitado
   - Emission Color: Teal (RGB: 0, 0.8, 1)

4. Asigna este material a KAI

---

## PASO 7: Crear Obstáculos (Prefabs)

### 7.1 Obstáculo Estático
1. Create Empty → Rename a "ObstacleStatic"
2. Add child Cube (scale 1, 1, 1)
3. Add Component → Box Collider → Is Trigger: ON
4. Drag `MemoryObstacle.cs`
5. En MemoryObstacle: Obstacle Type: Static
6. Drag a `Assets/Prefabs/ObstacleStatic.prefab`
7. Delete de Hierarchy

### 7.2 Obstáculo Móvil
1. Create Empty → Rename a "ObstacleMoving"
2. Add child Cube
3. Add Component → Box Collider → Is Trigger: ON
4. Drag `MemoryObstacle.cs`
5. En MemoryObstacle: Obstacle Type: Moving
6. Drag a `Assets/Prefabs/ObstacleMoving.prefab`
7. Delete de Hierarchy

---

## PASO 8: Crear Coleccionables (Prefabs)

### 8.1 Nodo Estándar
1. Create Empty → Rename a "NodeStandard"
2. Add child Sphere (scale 0.3, 0.3, 0.3)
3. Material teal
4. Add Component → Sphere Collider → Is Trigger: ON
5. Drag `IdentityNode.cs`
6. En IdentityNode: Node Type: Standard
7. Drag a `Assets/Prefabs/NodeStandard.prefab`
8. Delete de Hierarchy

### 8.2 Nodo Empatía
1. Duplicate NodeStandard prefab
2. Rename a "NodeEmpathy"
3. Open prefab → Change color a dorado
4. En IdentityNode: Node Type: Empathy
5. Save as `Assets/Prefabs/NodeEmpathy.prefab`

### 8.3 Nodo Velocidad
1. Duplicate NodeStandard
2. Rename a "NodeSpeed"
3. Change color a azul
4. En IdentityNode: Node Type: Speed
5. Save as `Assets/Prefabs/NodeSpeed.prefab`

### 8.4 Nodo Trauma
1. Duplicate NodeStandard
2. Rename a "NodeTrauma"
3. Change color a rojo
4. En IdentityNode: Node Type: Trauma
5. Save as `Assets/Prefabs/NodeTrauma.prefab`

---

## PASO 9: Crear Tags
1. Selecciona cualquier GameObject
2. Inspector → Tag dropdown → Add Tag
3. Crea estos tags:
   - `MemoryObstacle`
   - `IdentityNode`
   - `Guardian`

---

## PASO 10: Crear Guardianes (Opcional pero Recomendado)

### 10.1 Guardián LUMEN
1. Create Empty → Rename a "LUMEN"
2. Add child Sphere (escala 2)
3. Material blanco/dorado
4. Position: (0, 2, 100)
5. Add Component → Sphere Collider (NO trigger)
6. Drag `GuardianLUMEN.cs`
7. Add Component → Particle System (luciérnagas opcionales)
8. Tag: `Guardian`

### 10.2 Guardianes VOID, ECHO, ARIA
1. Repite proceso similar para cada uno en sus posiciones específicas:
   - VOID: Agregar componente con `GuardianVOID.cs`
   - ECHO: Agregar componentes con `GuardianECHO.cs`
   - ARIA: Agregar componentes con `GuardianARIA.cs`

---

## PASO 11: Configurar Canvas UI (HUD)

### 11.1 Crear HUD Panel
1. En UICanvas → Create → Panel
2. Rename a "HUD"
3. Anchor Preset: Stretch (todo el canvas)
4. Color Background: Transparent

### 11.2 Agregar Elementos
Dentro de HUD, crea:
1. Text - Distance
   - Anchor: Top Left
   - Text: "DISTANCIA: 0m"
   - Font Size: 36

2. Text - Nodes
   - Anchor: Top Center
   - Text: "NODOS: 0"
   - Font Size: 36

3. Image - Resonance Meter
   - Anchor: Bottom Center
   - Size: (400, 50)
   - Image Type: Filled (Horizontal)

4. Text - Level Name
   - Anchor: Center
   - Font Size: 48
   - Color: Cyan
   - Text: "[Nombre del Nivel]"

5. Text - QuantumFall Prompt
   - Anchor: Center
   - Color: Red
   - Text: "¡PRESIONA CUALQUIER TECLA!"

6. Text - ARIA Countdown
   - Anchor: Top Right
   - Color: Red (when < 10s)
   - Text: "TIEMPO: 40s"

### 11.3 Asignar Referencias
1. Selecciona UIManager en Hierarchy
2. En Inspector, arrastra cada elemento de texto/imagen a los campos correspondientes

---

## PASO 12: Configurar Entradas de Teclado

### 12.1 Input Manager
1. Edit → Project Settings → Input Manager
2. Verifica que existan:
   - Horizontal (A/D o flechas izq/der)
   - Vertical (W/S o flechas arriba/abajo)
3. Si no existen, créalas:
   - Positive Button: D (Horizontal) / W (Vertical)
   - Negative Button: A (Horizontal) / S (Vertical)
   - Alt Positive/Negative: Right Arrow / Key Up

---

## PASO 13: Configurar Físicas

### 13.1 Project Settings
1. Edit → Project Settings → Physics
2. Gravity: (0, -25, 0) [Para coincidir con playerController gravity]
3. Default Drag: 0
4. Default Angular Drag: 0.05

---

## PASO 14: Test Play
1. Configura KAI en posición (0, 1, 0)
2. Press Play
3. Verifica:
   - ✓ KAI puede saltar (Espacio/W/Flecha Arriba)
   - ✓ KAI puede cambiar de carril (A/D o Flechas)
   - ✓ KAI se desliza (S/Flecha Abajo)
   - ✓ HUD muestra distancia y nodos
   - ✓ Si toca obstáculo, activa Quantum Fall

---

## PASO 15: Agregar Música y Audio

### 15.1 Importar Audio
1. Descarga archivos MP3/WAV
2. Coloca en `Assets/Audio/Music/` y `Assets/Audio/Sfx/`
3. En Inspector, configura:
   - Compression Format: Vorbis
   - Quality: 50%

### 15.2 Asignar a AudioManager
1. Selecciona AudioManager
2. En Inspector, arrastra clips a:
   - Level Music Clips[0-4]
   - Level Ambiance Clips[0-4]
   - Good Ending Clip
   - Poor Ending Clip

---

## PASO 16: Diálogos y Sistema de Resonancia

### 16.1 Canvas de Diálogo
1. En UICanvas → Create → Panel
2. Rename a "DialoguePanel"
3. Add estos elementos:
   - Image: Portrait (lado izquierdo)
   - Text: Speaker Name
   - Text: Dialogue Text
   - Button: Option1
   - Button: Option2
   - Button: Option3 (opcional)

### 16.2 Asignar a DialogueSystem
1. Selecciona DialogueSystem (o créalo si no existe)
2. Drag a PlayerController u otro GameObject importante
3. Asigna referencias en Inspector

---

## PASO 17: Configuración Final

### 17.1 Scene Manager
1. File → Save scene como "MainLevel"
2. File → Build Settings
3. Añade "MainLevel" a Scenes In Build

### 17.2 Player Settings
1. Edit → Project Settings → Player
2. Company Name: "Your Team"
3. Product Name: "GHOST SIGNAL"
4. Resolution: 1920x1080

---

## COMPONENTES POR GAMEOBJECT (Referencia Rápida)

### KAI (Jugador)
- CharacterController ✓
- Collider (Trigger) para nodos ✓
- PlayerController.cs ✓
- SkinnedMeshRenderer (opcional) para efectos de circuitos

### GameManager
- GameManager.cs ✓
- AudioSource x2 ✓

### UIManager
- UIManager.cs ✓

### CameraController (Main Camera)
- CameraController.cs ✓

### AudioManager
- AudioManager.cs ✓
- AudioSource x2 ✓

### TrackGenerator
- TrackGenerator.cs ✓

### Guardianes (cada uno)
- GuardianLUMEN.cs / GuardianVOID.cs / etc. ✓
- Collider ✓
- Renderer ✓

---

## AHORA ESTÁS LISTO

¡El juego básico debería estar funcional! Prueba:

1. Presiona Play
2. Mueve a KAI con A/D y Espacio
3. Recolecta nodos
4. Evita obstáculos
5. Verifica que los finales cambian según resonancia emocional

## SIGUIENTES PASOS (Opcionales pero Recomendados)

- [ ] Agregar modelos 3D de personajes personalizados
- [ ] Crear animaciones para KAI (correr, saltar, deslizar)
- [ ] Agregar efectos de partículas para coleccionables
- [ ] Implementar cinemáticas de guardianes
- [ ] Crear HUD mejorado con fuentes custom
- [ ] Agregar sonidos de SFX para saltos, colisiones, etc.
- [ ] Crear skins alternativos para los mundos
- [ ] Implementar leaderboard local

---

**¡A DISFRUTAR CREANDO GHOST SIGNAL!** 👻⚡
