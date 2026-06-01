const logEl = document.getElementById("log");
const cursosList = document.getElementById("cursosList");
const asignacionesList = document.getElementById("asignacionesList");
const currentUserEl = document.getElementById("currentUser");
const apiBaseInput = document.getElementById("apiBase");

let apiBase = apiBaseInput.value;
let authState = {
  token: null,
  usuarioId: null,
  nombre: null,
  rol: null
};

const saveBaseBtn = document.getElementById("saveBase");

saveBaseBtn.addEventListener("click", () => {
  apiBase = apiBaseInput.value.trim();
  writeLog(`Base API actualizada a ${apiBase}`);
});

const btnRegister = document.getElementById("btnRegister");
const btnLogin = document.getElementById("btnLogin");
const btnCreateCurso = document.getElementById("btnCreateCurso");
const btnJoinCurso = document.getElementById("btnJoinCurso");
const btnListCursos = document.getElementById("btnListCursos");
const btnCreateAsignacion = document.getElementById("btnCreateAsignacion");
const btnUpdateAsignacion = document.getElementById("btnUpdateAsignacion");
const btnListAsignaciones = document.getElementById("btnListAsignaciones");

btnRegister.addEventListener("click", async () => {
  const payload = {
    nombre: getValue("regNombre"),
    email: getValue("regEmail"),
    password: getValue("regPassword"),
    idRol: Number(getValue("regRol"))
  };

  const result = await apiRequest("/api/auth/register", "POST", payload);
  if (result) {
    writeLog(`Registro exitoso: ${result.nombre ?? payload.nombre}`);
  }
});

btnLogin.addEventListener("click", async () => {
  const payload = {
    email: getValue("loginEmail"),
    password: getValue("loginPassword")
  };

  const result = await apiRequest("/api/auth/login", "POST", payload);
  if (result) {
    authState = {
      token: result.token,
      usuarioId: result.usuarioId,
      nombre: result.nombre,
      rol: result.rol
    };
    currentUserEl.textContent = `${authState.nombre} (ID ${authState.usuarioId}) - ${authState.rol}`;
    writeLog(`Login exitoso. Token: ${result.token?.slice(0, 20)}...`);
  }
});

btnCreateCurso.addEventListener("click", async () => {
  const docenteId = Number(getValue("cursoDocenteId"));
  const payload = {
    codigo: getValue("cursoCodigo"),
    nombre: getValue("cursoNombre"),
    periodo: getValue("cursoPeriodo")
  };

  const result = await apiRequest("/api/cursos", "POST", payload, {
    docenteId
  });

  if (result) {
    writeLog(`Curso creado: ${result.nombre} (#${result.idCurso})`);
  }
});

btnJoinCurso.addEventListener("click", async () => {
  const cursoId = Number(getValue("joinCursoId"));
  const payload = {
    idUsuario: Number(getValue("joinUsuarioId")),
    tipoParticipacion: getValue("joinTipo")
  };

  const result = await apiRequest(`/api/cursos/${cursoId}/join`, "POST", payload);
  if (result) {
    writeLog("Matrícula exitosa");
  }
});

btnListCursos.addEventListener("click", async () => {
  const userId = Number(getValue("listCursosUser"));
  const result = await apiRequest(`/api/cursos/usuario/${userId}`, "GET");
  if (result) {
    cursosList.innerHTML = result
      .map((curso) => `<li>${curso.codigo} - ${curso.nombre} (${curso.periodo})</li>`)
      .join("");
    writeLog(`Cursos cargados: ${result.length}`);
  }
});

btnCreateAsignacion.addEventListener("click", async () => {
  const docenteId = Number(getValue("asigDocenteId"));
  const payload = {
    idCurso: Number(getValue("asigCursoId")),
    titulo: getValue("asigTitulo"),
    descripcion: getValue("asigDescripcion"),
    fechaLimite: toIso(getValue("asigFecha"))
  };

  const result = await apiRequest("/api/asignaciones", "POST", payload, {
    docenteId
  });
  if (result) {
    writeLog(`Asignación creada con ID ${result.id}`);
  }
});

btnUpdateAsignacion.addEventListener("click", async () => {
  const asignacionId = Number(getValue("updAsigId"));
  const payload = {
    titulo: getValue("updTitulo") || null,
    descripcion: getValue("updDescripcion") || null,
    fechaLimite: toIso(getValue("updFecha")) || null
  };

  const result = await apiRequest(`/api/asignaciones/${asignacionId}`, "PUT", payload);
  if (result) {
    writeLog("Asignación actualizada");
  }
});

btnListAsignaciones.addEventListener("click", async () => {
  const cursoId = Number(getValue("listAsigCursoId"));
  const result = await apiRequest(`/api/asignaciones/curso/${cursoId}`, "GET");
  if (result) {
    asignacionesList.innerHTML = result
      .map((item) => `<li>${item.titulo} - vence ${formatDate(item.fechaLimite)}</li>`)
      .join("");
    writeLog(`Asignaciones cargadas: ${result.length}`);
  }
});

function getValue(id) {
  return document.getElementById(id).value.trim();
}

function toIso(value) {
  if (!value) return "";
  return new Date(value).toISOString();
}

function formatDate(value) {
  if (!value) return "";
  return new Date(value).toLocaleString();
}

async function apiRequest(path, method, body, extraHeaders = {}) {
  try {
    const response = await fetch(`${apiBase}${path}`, {
      method,
      headers: {
        "Content-Type": "application/json",
        ...(authState.token ? { Authorization: `Bearer ${authState.token}` } : {}),
        ...normalizeHeaders(extraHeaders)
      },
      body: method === "GET" ? undefined : JSON.stringify(body ?? {})
    });

    const text = await response.text();
    if (!response.ok) {
      writeLog(`Error ${response.status}: ${text}`);
      return null;
    }

    if (!text) return {};
    return JSON.parse(text);
  } catch (error) {
    writeLog(`Error: ${error.message}`);
    return null;
  }
}

function normalizeHeaders(headers) {
  const normalized = {};
  Object.entries(headers).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== "") {
      normalized[key] = String(value);
    }
  });
  return normalized;
}

function writeLog(message) {
  const timestamp = new Date().toLocaleTimeString();
  logEl.textContent = `[${timestamp}] ${message}\n` + logEl.textContent;
}
