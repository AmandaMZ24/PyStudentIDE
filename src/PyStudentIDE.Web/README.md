# PyStudentIDE Web

Cliente web mínimo para consumir la API de PyStudentIDE.

## Requisitos
- Node.js 18+

## Ejecutar
1. Enciende la API (`PyStudentIDE.Api`).
2. Inicia el cliente web:

```sh
npm install
npm run dev
```

Luego abre `http://localhost:5173`.

## Funcionalidades cubiertas
- Registro y autenticación.
- Crear cursos y unirse a cursos.
- Crear y modificar tareas.
- Ver tareas por curso.

## Notas
- Puedes cambiar la URL base de la API desde la sección **Configuración API**.
- El campo `Docente ID` se envía como header `docenteId` para crear cursos y tareas.
