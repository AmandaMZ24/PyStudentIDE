-- ============================================================
-- PyStudentIDE Database Schema (4NF)
-- Based on Architecture Design Document
-- ============================================================

-- ROL
CREATE TABLE ROL (
    id_rol INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(50) NOT NULL UNIQUE,
    descripcion VARCHAR(255)
);

-- USUARIO
CREATE TABLE USUARIO (
    id_usuario INT PRIMARY KEY IDENTITY(1,1),
    id_rol INT NOT NULL FOREIGN KEY REFERENCES ROL(id_rol),
    nombre VARCHAR(100) NOT NULL,
    correo VARCHAR(150) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    activo BIT NOT NULL DEFAULT 1,
    fecha_creacion DATETIME NOT NULL DEFAULT GETUTCDATE()
);

-- CURSO
CREATE TABLE CURSO (
    id_curso INT PRIMARY KEY IDENTITY(1,1),
    codigo VARCHAR(20) NOT NULL UNIQUE,
    nombre VARCHAR(100) NOT NULL,
    periodo VARCHAR(20) NOT NULL,
    activo BIT NOT NULL DEFAULT 1
);

-- MATRICULA
CREATE TABLE MATRICULA (
    id_matricula INT PRIMARY KEY IDENTITY(1,1),
    id_usuario INT NOT NULL FOREIGN KEY REFERENCES USUARIO(id_usuario),
    id_curso INT NOT NULL FOREIGN KEY REFERENCES CURSO(id_curso),
    tipo_participacion VARCHAR(20) NOT NULL CHECK (tipo_participacion IN ('ESTUDIANTE', 'DOCENTE')),
    fecha_matricula DATETIME NOT NULL DEFAULT GETUTCDATE(),
    UNIQUE(id_usuario, id_curso)
);

-- ASIGNACION
CREATE TABLE ASIGNACION (
    id_asignacion INT PRIMARY KEY IDENTITY(1,1),
    id_curso INT NOT NULL FOREIGN KEY REFERENCES CURSO(id_curso),
    id_docente INT NOT NULL FOREIGN KEY REFERENCES USUARIO(id_usuario),
    titulo VARCHAR(150) NOT NULL,
    descripcion TEXT,
    fecha_publicacion DATETIME NOT NULL,
    fecha_limite DATETIME NOT NULL,
    activa BIT NOT NULL DEFAULT 1,
    admite_trabajo_grupal BIT NOT NULL DEFAULT 0,
    tamano_maximo_grupo INT,
    inicio_periodo_prueba DATETIME,
    fin_periodo_prueba DATETIME
);

-- ENTREGA
CREATE TABLE ENTREGA (
    id_entrega INT PRIMARY KEY IDENTITY(1,1),
    id_asignacion INT NOT NULL FOREIGN KEY REFERENCES ASIGNACION(id_asignacion),
    id_estudiante INT NOT NULL FOREIGN KEY REFERENCES USUARIO(id_usuario),
    fecha_entrega DATETIME NOT NULL DEFAULT GETUTCDATE(),
    estado VARCHAR(30) NOT NULL CHECK (estado IN ('RECIBIDA', 'VALIDADA', 'RECHAZADA', 'CALIFICADA')),
    calificacion DECIMAL(5,2) CHECK (calificacion >= 0 AND calificacion <= 100),
    es_tardia BIT NOT NULL DEFAULT 0,
    numero_intento INT NOT NULL DEFAULT 1,
    firma_digital TEXT,
    UNIQUE(id_asignacion, id_estudiante, numero_intento)
);

-- ARCHIVO
CREATE TABLE ARCHIVO (
    id_archivo INT PRIMARY KEY IDENTITY(1,1),
    id_entrega INT NOT NULL FOREIGN KEY REFERENCES ENTREGA(id_entrega),
    nombre_archivo VARCHAR(150) NOT NULL,
    ruta_archivo VARCHAR(255) NOT NULL,
    tipo_archivo VARCHAR(10) NOT NULL DEFAULT '.py',
    tamano_bytes INT NOT NULL CHECK (tamano_bytes > 0),
    fecha_carga DATETIME NOT NULL DEFAULT GETUTCDATE(),
    version_anterior VARCHAR(255)
);

-- VALIDACION_HASH
CREATE TABLE VALIDACION_HASH (
    id_validacion INT PRIMARY KEY IDENTITY(1,1),
    id_archivo INT NOT NULL UNIQUE FOREIGN KEY REFERENCES ARCHIVO(id_archivo),
    algoritmo VARCHAR(20) NOT NULL,
    hash_calculado CHAR(64) NOT NULL,
    valido BIT NOT NULL,
    fecha_validacion DATETIME NOT NULL DEFAULT GETUTCDATE()
);

-- COMMIT_GIT
CREATE TABLE COMMIT_GIT (
    id_commit INT PRIMARY KEY IDENTITY(1,1),
    id_entrega INT NOT NULL FOREIGN KEY REFERENCES ENTREGA(id_entrega),
    hash_commit VARCHAR(40) NOT NULL UNIQUE,
    mensaje VARCHAR(255),
    rama VARCHAR(100) NOT NULL DEFAULT 'main',
    fecha_commit DATETIME NOT NULL
);

-- CASO_PRUEBA
CREATE TABLE CASO_PRUEBA (
    id_caso_prueba INT PRIMARY KEY IDENTITY(1,1),
    id_asignacion INT NOT NULL FOREIGN KEY REFERENCES ASIGNACION(id_asignacion),
    nombre VARCHAR(100) NOT NULL,
    descripcion TEXT,
    entrada TEXT,
    salida_esperada TEXT NOT NULL,
    puntaje INT CHECK (puntaje >= 0),
    activo BIT NOT NULL DEFAULT 1
);

-- RESULTADO_PRUEBA
CREATE TABLE RESULTADO_PRUEBA (
    id_resultado INT PRIMARY KEY IDENTITY(1,1),
    id_entrega INT NOT NULL FOREIGN KEY REFERENCES ENTREGA(id_entrega),
    id_caso_prueba INT NOT NULL FOREIGN KEY REFERENCES CASO_PRUEBA(id_caso_prueba),
    aprobado BIT NOT NULL,
    salida_obtenida TEXT,
    mensaje_error TEXT,
    tiempo_ejecucion DECIMAL(8,3) CHECK (tiempo_ejecucion >= 0),
    fecha_ejecucion DATETIME NOT NULL DEFAULT GETUTCDATE(),
    UNIQUE(id_entrega, id_caso_prueba)
);
