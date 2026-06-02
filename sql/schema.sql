-- ============================================================
-- PyStudentIDE Database Schema (4NF)
-- Based on Architecture Design Document
-- ============================================================


IF DB_ID('PyStudentIDE') IS NULL
BEGIN
    CREATE DATABASE PyStudentIDE;
END
GO

USE PyStudentIDE;
GO

-- ============================================================
-- Eliminar tablas si ya existen
-- Se eliminan en orden inverso por las llaves foráneas
-- ============================================================

DROP TABLE IF EXISTS RESULTADO_PRUEBA;
DROP TABLE IF EXISTS CASO_PRUEBA;
DROP TABLE IF EXISTS COMMIT_GIT;
DROP TABLE IF EXISTS VALIDACION_HASH;
DROP TABLE IF EXISTS ARCHIVO;
DROP TABLE IF EXISTS ENTREGA;
DROP TABLE IF EXISTS ASIGNACION;
DROP TABLE IF EXISTS MATRICULA;
DROP TABLE IF EXISTS CURSO;
DROP TABLE IF EXISTS USUARIO;
DROP TABLE IF EXISTS ROL;
GO

-- ============================================================
-- ROL
-- ============================================================

CREATE TABLE ROL (
    IdRol INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL UNIQUE,
    Descripcion VARCHAR(255)
);
GO

-- ============================================================
-- USUARIO
-- ============================================================

CREATE TABLE USUARIO (
    IdUsuario INT IDENTITY(1,1) PRIMARY KEY,
    IdRol INT NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Correo VARCHAR(150) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_USUARIO_ROL
        FOREIGN KEY (IdRol) REFERENCES ROL(IdRol)
);
GO

-- ============================================================
-- CURSO
-- ============================================================

CREATE TABLE CURSO (
    IdCurso INT IDENTITY(1,1) PRIMARY KEY,
    Codigo VARCHAR(20) NOT NULL UNIQUE,
    Nombre VARCHAR(100) NOT NULL,
    Periodo VARCHAR(50) NOT NULL,
    Activo BIT NOT NULL DEFAULT 1
);
GO

-- ============================================================
-- MATRICULA
-- ============================================================

CREATE TABLE MATRICULA (
    IdMatricula INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuario INT NOT NULL,
    IdCurso INT NOT NULL,
    TipoParticipacion VARCHAR(20) NOT NULL,
    FechaMatricula DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_MATRICULA_USUARIO
        FOREIGN KEY (IdUsuario) REFERENCES USUARIO(IdUsuario),

    CONSTRAINT FK_MATRICULA_CURSO
        FOREIGN KEY (IdCurso) REFERENCES CURSO(IdCurso),

    CONSTRAINT UQ_MATRICULA_USUARIO_CURSO
        UNIQUE (IdUsuario, IdCurso),

    CONSTRAINT CK_MATRICULA_TipoParticipacion
        CHECK (TipoParticipacion IN ('ESTUDIANTE', 'DOCENTE'))
);
GO

-- ============================================================
-- ASIGNACION
-- ============================================================

CREATE TABLE ASIGNACION (
    IdAsignacion INT IDENTITY(1,1) PRIMARY KEY,
    IdCurso INT NOT NULL,
    IdDocente INT NOT NULL,
    Titulo VARCHAR(150) NOT NULL,
    Descripcion VARCHAR(MAX),
    FechaPublicacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaLimite DATETIME2 NOT NULL,
    Activa BIT NOT NULL DEFAULT 1,
    AdmiteTrabajoGrupal BIT NOT NULL DEFAULT 0,
    TamanoMaximoGrupo INT,
    InicioPeriodoPrueba DATETIME2,
    FinPeriodoPrueba DATETIME2,

    CONSTRAINT FK_ASIGNACION_CURSO
        FOREIGN KEY (IdCurso) REFERENCES CURSO(IdCurso),

    CONSTRAINT FK_ASIGNACION_DOCENTE
        FOREIGN KEY (IdDocente) REFERENCES USUARIO(IdUsuario)
);
GO

-- ============================================================
-- ENTREGA
-- ============================================================

CREATE TABLE ENTREGA (
    IdEntrega INT IDENTITY(1,1) PRIMARY KEY,
    IdAsignacion INT NOT NULL,
    IdEstudiante INT NOT NULL,
    FechaEntrega DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Estado VARCHAR(30) NOT NULL,
    Calificacion DECIMAL(5,2),
    EsTardia BIT NOT NULL DEFAULT 0,
    NumeroIntento INT NOT NULL DEFAULT 1,
    FirmaDigital VARCHAR(MAX),

    CONSTRAINT FK_ENTREGA_ASIGNACION
        FOREIGN KEY (IdAsignacion) REFERENCES ASIGNACION(IdAsignacion),

    CONSTRAINT FK_ENTREGA_ESTUDIANTE
        FOREIGN KEY (IdEstudiante) REFERENCES USUARIO(IdUsuario),

    CONSTRAINT UQ_ENTREGA_ASIGNACION_ESTUDIANTE_INTENTO
        UNIQUE (IdAsignacion, IdEstudiante, NumeroIntento),

    CONSTRAINT CK_ENTREGA_Estado
        CHECK (Estado IN ('RECIBIDA', 'VALIDADA', 'RECHAZADA', 'CALIFICADA')),

    CONSTRAINT CK_ENTREGA_Calificacion
        CHECK (Calificacion IS NULL OR (Calificacion >= 0 AND Calificacion <= 100))
);
GO

-- ============================================================
-- ARCHIVO
-- ============================================================

CREATE TABLE ARCHIVO (
    IdArchivo INT IDENTITY(1,1) PRIMARY KEY,
    IdEntrega INT NOT NULL,
    NombreArchivo VARCHAR(150) NOT NULL,
    RutaArchivo VARCHAR(255) NOT NULL,
    TipoArchivo VARCHAR(10) NOT NULL DEFAULT '.py',
    TamanoBytes INT NOT NULL,
    FechaCarga DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    VersionAnterior VARCHAR(255),

    CONSTRAINT FK_ARCHIVO_ENTREGA
        FOREIGN KEY (IdEntrega) REFERENCES ENTREGA(IdEntrega),

    CONSTRAINT CK_ARCHIVO_TamanoBytes
        CHECK (TamanoBytes > 0)
);
GO

-- ============================================================
-- VALIDACION_HASH
-- ============================================================

CREATE TABLE VALIDACION_HASH (
    IdValidacion INT IDENTITY(1,1) PRIMARY KEY,
    IdArchivo INT NOT NULL UNIQUE,
    Algoritmo VARCHAR(20) NOT NULL,
    HashCalculado CHAR(64) NOT NULL,
    Valido BIT NOT NULL,
    FechaValidacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_VALIDACION_HASH_ARCHIVO
        FOREIGN KEY (IdArchivo) REFERENCES ARCHIVO(IdArchivo)
);
GO

-- ============================================================
-- COMMIT_GIT
-- ============================================================

CREATE TABLE COMMIT_GIT (
    IdCommit INT IDENTITY(1,1) PRIMARY KEY,
    IdEntrega INT NOT NULL,
    HashCommit VARCHAR(40) NOT NULL UNIQUE,
    Mensaje VARCHAR(255),
    Rama VARCHAR(100) NOT NULL DEFAULT 'main',
    FechaCommit DATETIME2 NOT NULL,

    CONSTRAINT FK_COMMIT_GIT_ENTREGA
        FOREIGN KEY (IdEntrega) REFERENCES ENTREGA(IdEntrega)
);
GO

-- ============================================================
-- CASO_PRUEBA
-- ============================================================

CREATE TABLE CASO_PRUEBA (
    IdCasoPrueba INT IDENTITY(1,1) PRIMARY KEY,
    IdAsignacion INT NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion VARCHAR(MAX),
    Entrada VARCHAR(MAX),
    SalidaEsperada VARCHAR(MAX) NOT NULL,
    Puntaje INT,
    Activo BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_CASO_PRUEBA_ASIGNACION
        FOREIGN KEY (IdAsignacion) REFERENCES ASIGNACION(IdAsignacion),

    CONSTRAINT CK_CASO_PRUEBA_Puntaje
        CHECK (Puntaje IS NULL OR Puntaje >= 0)
);
GO

-- ============================================================
-- RESULTADO_PRUEBA
-- ============================================================

CREATE TABLE RESULTADO_PRUEBA (
    IdResultado INT IDENTITY(1,1) PRIMARY KEY,
    IdEntrega INT NOT NULL,
    IdCasoPrueba INT NOT NULL,
    Aprobado BIT NOT NULL,
    SalidaObtenida VARCHAR(MAX),
    MensajeError VARCHAR(MAX),
    TiempoEjecucion DECIMAL(8,3),
    FechaEjecucion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_RESULTADO_PRUEBA_ENTREGA
        FOREIGN KEY (IdEntrega) REFERENCES ENTREGA(IdEntrega),

    CONSTRAINT FK_RESULTADO_PRUEBA_CASO_PRUEBA
        FOREIGN KEY (IdCasoPrueba) REFERENCES CASO_PRUEBA(IdCasoPrueba),

    CONSTRAINT UQ_RESULTADO_PRUEBA_ENTREGA_CASO
        UNIQUE (IdEntrega, IdCasoPrueba),

    CONSTRAINT CK_RESULTADO_PRUEBA_TiempoEjecucion
        CHECK (TiempoEjecucion IS NULL OR TiempoEjecucion >= 0)
);
GO

-- ============================================================
-- Datos iniciales
-- ============================================================

INSERT INTO ROL (Nombre, Descripcion)
VALUES 
('DOCENTE', 'Usuario docente del sistema'),
('ESTUDIANTE', 'Usuario estudiante del sistema');
GO

-- ============================================================
-- Fin del script
-- ============================================================

IF DB_ID('PyStudentIDE') IS NULL
BEGIN
    CREATE DATABASE PyStudentIDE;
END
GO

USE PyStudentIDE;
GO

-- ============================================================
-- Eliminar tablas si ya existen
-- Se eliminan en orden inverso por las llaves foráneas
-- ============================================================

DROP TABLE IF EXISTS RESULTADO_PRUEBA;
DROP TABLE IF EXISTS CASO_PRUEBA;
DROP TABLE IF EXISTS COMMIT_GIT;
DROP TABLE IF EXISTS VALIDACION_HASH;
DROP TABLE IF EXISTS ARCHIVO;
DROP TABLE IF EXISTS ENTREGA;
DROP TABLE IF EXISTS ASIGNACION;
DROP TABLE IF EXISTS MATRICULA;
DROP TABLE IF EXISTS CURSO;
DROP TABLE IF EXISTS USUARIO;
DROP TABLE IF EXISTS ROL;
GO

-- ============================================================
-- ROL
-- ============================================================

CREATE TABLE ROL (
    IdRol INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL UNIQUE,
    Descripcion VARCHAR(255)
);
GO

-- ============================================================
-- USUARIO
-- ============================================================

CREATE TABLE USUARIO (
    IdUsuario INT IDENTITY(1,1) PRIMARY KEY,
    IdRol INT NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Correo VARCHAR(150) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_USUARIO_ROL
        FOREIGN KEY (IdRol) REFERENCES ROL(IdRol)
);
GO

-- ============================================================
-- CURSO
-- ============================================================

CREATE TABLE CURSO (
    IdCurso INT IDENTITY(1,1) PRIMARY KEY,
    Codigo VARCHAR(20) NOT NULL UNIQUE,
    Nombre VARCHAR(100) NOT NULL,
    Periodo VARCHAR(50) NOT NULL,
    Activo BIT NOT NULL DEFAULT 1
);
GO

-- ============================================================
-- MATRICULA
-- ============================================================

CREATE TABLE MATRICULA (
    IdMatricula INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuario INT NOT NULL,
    IdCurso INT NOT NULL,
    TipoParticipacion VARCHAR(20) NOT NULL,
    FechaMatricula DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_MATRICULA_USUARIO
        FOREIGN KEY (IdUsuario) REFERENCES USUARIO(IdUsuario),

    CONSTRAINT FK_MATRICULA_CURSO
        FOREIGN KEY (IdCurso) REFERENCES CURSO(IdCurso),

    CONSTRAINT UQ_MATRICULA_USUARIO_CURSO
        UNIQUE (IdUsuario, IdCurso),

    CONSTRAINT CK_MATRICULA_TipoParticipacion
        CHECK (TipoParticipacion IN ('ESTUDIANTE', 'DOCENTE'))
);
GO

-- ============================================================
-- ASIGNACION
-- ============================================================

CREATE TABLE ASIGNACION (
    IdAsignacion INT IDENTITY(1,1) PRIMARY KEY,
    IdCurso INT NOT NULL,
    IdDocente INT NOT NULL,
    Titulo VARCHAR(150) NOT NULL,
    Descripcion VARCHAR(MAX),
    FechaPublicacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaLimite DATETIME2 NOT NULL,
    Activa BIT NOT NULL DEFAULT 1,
    AdmiteTrabajoGrupal BIT NOT NULL DEFAULT 0,
    TamanoMaximoGrupo INT,
    InicioPeriodoPrueba DATETIME2,
    FinPeriodoPrueba DATETIME2,

    CONSTRAINT FK_ASIGNACION_CURSO
        FOREIGN KEY (IdCurso) REFERENCES CURSO(IdCurso),

    CONSTRAINT FK_ASIGNACION_DOCENTE
        FOREIGN KEY (IdDocente) REFERENCES USUARIO(IdUsuario)
);
GO

-- ============================================================
-- ENTREGA
-- ============================================================

CREATE TABLE ENTREGA (
    IdEntrega INT IDENTITY(1,1) PRIMARY KEY,
    IdAsignacion INT NOT NULL,
    IdEstudiante INT NOT NULL,
    FechaEntrega DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Estado VARCHAR(30) NOT NULL,
    Calificacion DECIMAL(5,2),
    EsTardia BIT NOT NULL DEFAULT 0,
    NumeroIntento INT NOT NULL DEFAULT 1,
    FirmaDigital VARCHAR(MAX),

    CONSTRAINT FK_ENTREGA_ASIGNACION
        FOREIGN KEY (IdAsignacion) REFERENCES ASIGNACION(IdAsignacion),

    CONSTRAINT FK_ENTREGA_ESTUDIANTE
        FOREIGN KEY (IdEstudiante) REFERENCES USUARIO(IdUsuario),

    CONSTRAINT UQ_ENTREGA_ASIGNACION_ESTUDIANTE_INTENTO
        UNIQUE (IdAsignacion, IdEstudiante, NumeroIntento),

    CONSTRAINT CK_ENTREGA_Estado
        CHECK (Estado IN ('RECIBIDA', 'VALIDADA', 'RECHAZADA', 'CALIFICADA')),

    CONSTRAINT CK_ENTREGA_Calificacion
        CHECK (Calificacion IS NULL OR (Calificacion >= 0 AND Calificacion <= 100))
);
GO

-- ============================================================
-- ARCHIVO
-- ============================================================

CREATE TABLE ARCHIVO (
    IdArchivo INT IDENTITY(1,1) PRIMARY KEY,
    IdEntrega INT NOT NULL,
    NombreArchivo VARCHAR(150) NOT NULL,
    RutaArchivo VARCHAR(255) NOT NULL,
    TipoArchivo VARCHAR(10) NOT NULL DEFAULT '.py',
    TamanoBytes INT NOT NULL,
    FechaCarga DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    VersionAnterior VARCHAR(255),

    CONSTRAINT FK_ARCHIVO_ENTREGA
        FOREIGN KEY (IdEntrega) REFERENCES ENTREGA(IdEntrega),

    CONSTRAINT CK_ARCHIVO_TamanoBytes
        CHECK (TamanoBytes > 0)
);
GO

-- ============================================================
-- VALIDACION_HASH
-- ============================================================

CREATE TABLE VALIDACION_HASH (
    IdValidacion INT IDENTITY(1,1) PRIMARY KEY,
    IdArchivo INT NOT NULL UNIQUE,
    Algoritmo VARCHAR(20) NOT NULL,
    HashCalculado CHAR(64) NOT NULL,
    Valido BIT NOT NULL,
    FechaValidacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_VALIDACION_HASH_ARCHIVO
        FOREIGN KEY (IdArchivo) REFERENCES ARCHIVO(IdArchivo)
);
GO

-- ============================================================
-- COMMIT_GIT
-- ============================================================

CREATE TABLE COMMIT_GIT (
    IdCommit INT IDENTITY(1,1) PRIMARY KEY,
    IdEntrega INT NOT NULL,
    HashCommit VARCHAR(40) NOT NULL UNIQUE,
    Mensaje VARCHAR(255),
    Rama VARCHAR(100) NOT NULL DEFAULT 'main',
    FechaCommit DATETIME2 NOT NULL,

    CONSTRAINT FK_COMMIT_GIT_ENTREGA
        FOREIGN KEY (IdEntrega) REFERENCES ENTREGA(IdEntrega)
);
GO

-- ============================================================
-- CASO_PRUEBA
-- ============================================================

CREATE TABLE CASO_PRUEBA (
    IdCasoPrueba INT IDENTITY(1,1) PRIMARY KEY,
    IdAsignacion INT NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion VARCHAR(MAX),
    Entrada VARCHAR(MAX),
    SalidaEsperada VARCHAR(MAX) NOT NULL,
    Puntaje INT,
    Activo BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_CASO_PRUEBA_ASIGNACION
        FOREIGN KEY (IdAsignacion) REFERENCES ASIGNACION(IdAsignacion),

    CONSTRAINT CK_CASO_PRUEBA_Puntaje
        CHECK (Puntaje IS NULL OR Puntaje >= 0)
);
GO

-- ============================================================
-- RESULTADO_PRUEBA
-- ============================================================

CREATE TABLE RESULTADO_PRUEBA (
    IdResultado INT IDENTITY(1,1) PRIMARY KEY,
    IdEntrega INT NOT NULL,
    IdCasoPrueba INT NOT NULL,
    Aprobado BIT NOT NULL,
    SalidaObtenida VARCHAR(MAX),
    MensajeError VARCHAR(MAX),
    TiempoEjecucion DECIMAL(8,3),
    FechaEjecucion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_RESULTADO_PRUEBA_ENTREGA
        FOREIGN KEY (IdEntrega) REFERENCES ENTREGA(IdEntrega),

    CONSTRAINT FK_RESULTADO_PRUEBA_CASO_PRUEBA
        FOREIGN KEY (IdCasoPrueba) REFERENCES CASO_PRUEBA(IdCasoPrueba),

    CONSTRAINT UQ_RESULTADO_PRUEBA_ENTREGA_CASO
        UNIQUE (IdEntrega, IdCasoPrueba),

    CONSTRAINT CK_RESULTADO_PRUEBA_TiempoEjecucion
        CHECK (TiempoEjecucion IS NULL OR TiempoEjecucion >= 0)
);
GO

-- ============================================================
-- Datos iniciales
-- ============================================================

INSERT INTO ROL (Nombre, Descripcion)
VALUES 
('DOCENTE', 'Usuario docente del sistema'),
('ESTUDIANTE', 'Usuario estudiante del sistema');
GO

-- ============================================================
-- Fin del script
-- ============================================================
