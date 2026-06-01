# Decorator Assignment (Script)

Solución independiente del patrón Decorator para scripts Python.

## Requisitos
- .NET 9 SDK
- Windows (WPF)

## Ejecutar

```powershell
cd "Decorator"
dotnet run
```

## Funcionalidades
- Cargar un script `.py`
- Mostrar syntax highlight
- Firmar, verificar y regenerar firma (hash SHA-256 en CSV local)

## Archivos importantes
- `Decorators/` contiene la implementación del patrón.
- `Views/ScriptDecoratorView.xaml` es la UI principal.
- `docs/Informe-Decorator.md` es la plantilla del informe.
