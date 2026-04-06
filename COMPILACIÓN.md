# GUÍA DE COMPILACIÓN - GHOST SIGNAL

## 🎯 Opciones de Compilación

### OPCIÓN 1: Compilación desde PowerShell (Automática con Unity)

**Requisitos:**
- Unity 2022.3 LTS instalado en tu sistema

**Pasos:**

1. **Abre PowerShell** como Administrador
2. **Ve a la carpeta del proyecto:**
```powershell
cd "C:\Andres\UCC INGENIERIA DE SOFTWARE\CUARTO SEMESTRE\DiseñoDeInterfaces\ProyectoVideoJuego\GhostSignal"
```

3. **Ejecuta el script de compilación:**
```powershell
.\build.ps1
```

**Resultado esperado:**
- ✓ Compilación exitosa de todos los scripts C#
- ✓ Log de compilación guardado en `Build\Logs\Build.log`
- ✓ Mensaje: "COMPILACIÓN EXITOSA"

---

### OPCIÓN 2: Compilación Manual en Unity Editor

**Requisitos:**
- Unity Hub instalado
- Unity 2022.3 LTS instalado

**Pasos:**

1. **Abre Unity Hub**
2. **Click en "New Project"**
3. **Selecciona:**
   - Version: "2022.3 LTS" (o superior)
   - Template: "3D (Built-in Render Pipeline)"
   - Project Name: "GhostSignal"
   - Click en "Create"

4. **Espera a que Unity abra el proyecto**
5. **En la carpeta Assets** del nuevo proyecto, copia los archivos:
   ```
   Assets/
   ├── Scripts/
   │   ├── PlayerController.cs
   │   ├── GameManager.cs
   │   ├── DialogueSystem.cs
   │   ├── TrackGenerator.cs
   │   ├── Guardians.cs
   │   └── Systems.cs
   ```

6. **Unity compilará automáticamente**
7. **Verifica la consola:** Windows → General → Console
   - Si no hay errores en rojo, compilación exitosa ✓

---

### OPCIÓN 3: Build del Ejecutable

**Una vez que la compilación sea exitosa:**

```powershell
.\build.ps1 -Build
```

Esto generará:
- `Builds/GhostSignal.exe` - Ejecutable del juego compilado
- Listo para jugar en Windows

---

## ❌ Errores Comunes y Soluciones

### Error: "Unity no encontrado"
**Solución:**
```powershell
# Verifica que Unity 2022.3 LTS esté instalado
ls "C:\Program Files\Unity\Hub\Editor"
```

Si no aparece nada, **instala Unity 2022.3 LTS desde Unity Hub**.

### Error: "Proyecto no existe"
**Solución:**
Verifica que la ruta sea correcta:
```powershell
ls "C:\Andres\UCC INGENIERIA DE SOFTWARE\CUARTO SEMESTRE\DiseñoDeInterfaces\ProyectoVideoJuego\GhostSignal"
```

### Error: "Faltan archivos de script"
**Solución:**
Verifica que los 6 scripts estén en:
```
Assets/Scripts/
```

Todos estos archivos deben estar presentes:
- [ ] PlayerController.cs
- [ ] GameManager.cs
- [ ] DialogueSystem.cs
- [ ] TrackGenerator.cs
- [ ] Guardians.cs
- [ ] Systems.cs

---

## 📊 Verificar Compilación

Después de compilar, busca el log:
```
GhostSignal\Build\Logs\Build.log
```

**Contenido esperado:**
```
Assets/Scripts/PlayerController.cs(1,1): Started importing asset
Assets/Scripts/GameManager.cs(1,1): Started importing asset
...
All Assemblies loaded
[Assembly Reloading] 'Assets/Scripts/...': Loaded successfully
```

**SIN errores como:**
```
error CS1234: ...
```

---

## 🚀 Próximos Pasos Después de Compilar

1. **Abre el proyecto en Unity Editor**
2. **Sigue la guía SETUP.md** para configurar las escenas
3. **Presiona Play** para probar el juego
4. **Build final:** File → Build and Run (o usa `build.ps1 -Build`)

---

## 📝 Información del Proyecto

- **Engine:** Unity 2022.3 LTS
- **Lenguaje:** C# (.NET Standard 2.1)
- **Plataforma:** PC (Windows/Mac)
- **Scripts:** 6 archivos (aproximadamente 1500 líneas de código)
- **Características:** 5 niveles, 5 guardianes, 3 finales

---

**¿Necesitas ayuda? Abre una carpeta de Terminal en el proyecto y ejecuta:**
```powershell
.\build.ps1 -Verbose
```
