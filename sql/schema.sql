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
DROP TABLE IF EXISTS RETROALIMENTACION;
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
    Contenido NVARCHAR(MAX),

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
-- Table: LLAVE_CURSO (RSA-2048 Key Management)
-- ============================================================
CREATE TABLE LLAVE_CURSO (
    IdLlave INT IDENTITY(1,1) PRIMARY KEY,
    IdCurso INT NOT NULL,
    LlavePublicaXml NVARCHAR(MAX) NOT NULL,
    Algoritmo VARCHAR(30) NOT NULL,
    Activa BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    INDEX IX_LLAVE_CURSO_CursoActiva (IdCurso, Activa)
);
GO

-- ============================================================
-- RETROALIMENTACION (Docente Feedback)
-- ============================================================
CREATE TABLE RETROALIMENTACION (
    IdRetroalimentacion INT IDENTITY(1,1) PRIMARY KEY,
    IdEntrega INT NOT NULL,
    IdDocente INT NOT NULL,
    Comentario NVARCHAR(MAX) NOT NULL,
    Calificacion DECIMAL(5,2),
    FechaCreacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_RETROALIMENTACION_ENTREGA
        FOREIGN KEY (IdEntrega) REFERENCES ENTREGA(IdEntrega),
    CONSTRAINT FK_RETROALIMENTACION_DOCENTE
        FOREIGN KEY (IdDocente) REFERENCES USUARIO(IdUsuario)
);
GO

-- ============================================================
-- Fin del script
-- ============================================================
-- ============================================================
-- Datos de prueba
-- ============================================================

-- Contrase?a de prueba: 123456
DECLARE @Hash VARCHAR(255) = '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92';

-- Usuarios (3 docentes, 6 estudiantes)
INSERT INTO USUARIO (IdRol, Nombre, Correo, PasswordHash)
VALUES
(1, 'Dr. Carlos Mendoza', 'carlos.mendoza@mail.com', @Hash),
(1, 'MSc. Laura Jim?nez', 'laura.jimenez@mail.com', @Hash),
(1, 'Ing. Roberto Vargas', 'roberto.vargas@mail.com', @Hash),
(2, 'Ana Sof?a Rojas', 'ana.rojas@mail.com', @Hash),
(2, 'Luis Fernando Mora', 'luis.mora@mail.com', @Hash),
(2, 'Mar?a Jos? Solano', 'maria.solano@mail.com', @Hash),
(2, 'Pedro Andr?s Chac?n', 'pedro.chacon@mail.com', @Hash),
(2, 'Diana Carolina Vega', 'diana.vega@mail.com', @Hash),
(2, 'Jos? David Quir?s', 'jose.quiros@mail.com', @Hash);
GO

-- Cursos
INSERT INTO CURSO (Codigo, Nombre, Periodo, Activo)
VALUES
('CE-2101', 'Programaci?n I', 'I-2026', 1),
('CE-2103', 'Estructuras de Datos', 'I-2026', 1),
('CE-3104', 'Bases de Datos', 'I-2026', 1),
('CE-4105', 'Ingenier?a de Software', 'I-2026', 1),
('MA-1001', 'C?lculo I', 'I-2026', 0);
GO

-- Matr?culas: docentes
INSERT INTO MATRICULA (IdUsuario, IdCurso, TipoParticipacion)
VALUES
(1, 1, 'DOCENTE'), (1, 2, 'DOCENTE'),
(2, 3, 'DOCENTE'), (2, 4, 'DOCENTE'),
(3, 5, 'DOCENTE');
GO

-- Matr?culas: estudiantes
INSERT INTO MATRICULA (IdUsuario, IdCurso, TipoParticipacion)
VALUES
(4, 1, 'ESTUDIANTE'), (4, 2, 'ESTUDIANTE'),
(5, 1, 'ESTUDIANTE'), (5, 3, 'ESTUDIANTE'),
(6, 1, 'ESTUDIANTE'), (6, 4, 'ESTUDIANTE'),
(7, 2, 'ESTUDIANTE'), (7, 3, 'ESTUDIANTE'),
(8, 2, 'ESTUDIANTE'), (8, 4, 'ESTUDIANTE'),
(9, 1, 'ESTUDIANTE'), (9, 3, 'ESTUDIANTE');
GO

-- Asignaciones
INSERT INTO ASIGNACION (IdCurso, IdDocente, Titulo, Descripcion, FechaLimite, Activa, AdmiteTrabajoGrupal, InicioPeriodoPrueba, FinPeriodoPrueba)
VALUES
(1, 1, 'Calculadora b?sica en Python', 'Implementar una calculadora con suma, resta, multiplicaci?n y divisi?n.', DATEADD(DAY, 14, SYSUTCDATETIME()), 1, 0, DATEADD(DAY, 7, SYSUTCDATETIME()), DATEADD(DAY, 14, SYSUTCDATETIME())),
(1, 1, 'Juego de adivinanza', 'Programa que genera un n?mero aleatorio y el usuario debe adivinarlo.', DATEADD(DAY, 30, SYSUTCDATETIME()), 1, 0, NULL, NULL),
(2, 1, 'Lista enlazada simple', 'Implementar una lista enlazada con inserci?n, eliminaci?n y b?squeda.', DATEADD(DAY, 21, SYSUTCDATETIME()), 1, 1, DATEADD(DAY, 14, SYSUTCDATETIME()), DATEADD(DAY, 21, SYSUTCDATETIME())),
(2, 1, 'Pila y cola con arreglos', 'Implementar una pila y una cola usando arreglos est?ticos.', DATEADD(DAY, 10, SYSUTCDATETIME()), 1, 0, NULL, NULL),
(3, 2, 'Modelo entidad-relaci?n', 'Dise?ar el MER para un sistema de biblioteca universitaria.', DATEADD(DAY, 7, SYSUTCDATETIME()), 1, 1, DATEADD(DAY, 3, SYSUTCDATETIME()), DATEADD(DAY, 7, SYSUTCDATETIME())),
(4, 2, 'Diagramas UML - Casos de uso', 'Elaborar diagramas de casos de uso para un sistema de matr?cula.', DATEADD(DAY, 5, SYSUTCDATETIME()), 1, 0, NULL, NULL),
(5, 3, 'Derivadas b?sicas', 'Resolver ejercicios de derivadas de primer orden.', DATEADD(DAY, 60, SYSUTCDATETIME()), 0, 0, NULL, NULL);
GO

-- Entregas
INSERT INTO ENTREGA (IdAsignacion, IdEstudiante, FechaEntrega, Estado, Calificacion, EsTardia, NumeroIntento)
VALUES (1, 4, DATEADD(DAY, 13, SYSUTCDATETIME()), 'CALIFICADA', 92.00, 0, 1);
INSERT INTO ENTREGA (IdAsignacion, IdEstudiante, FechaEntrega, Estado, Calificacion, EsTardia, NumeroIntento)
VALUES (1, 5, DATEADD(DAY, 10, SYSUTCDATETIME()), 'CALIFICADA', 78.50, 0, 1);
INSERT INTO ENTREGA (IdAsignacion, IdEstudiante, FechaEntrega, Estado, Calificacion, EsTardia, NumeroIntento)
VALUES (1, 6, DATEADD(DAY, 15, SYSUTCDATETIME()), 'CALIFICADA', 65.00, 1, 1);
INSERT INTO ENTREGA (IdAsignacion, IdEstudiante, FechaEntrega, Estado, EsTardia, NumeroIntento)
VALUES (1, 9, SYSUTCDATETIME(), 'RECIBIDA', 0, 1);
INSERT INTO ENTREGA (IdAsignacion, IdEstudiante, FechaEntrega, Estado, Calificacion, EsTardia, NumeroIntento)
VALUES (4, 5, DATEADD(DAY, -3, SYSUTCDATETIME()), 'CALIFICADA', 55.00, 0, 1);
INSERT INTO ENTREGA (IdAsignacion, IdEstudiante, FechaEntrega, Estado, Calificacion, EsTardia, NumeroIntento)
VALUES (4, 5, DATEADD(DAY, -1, SYSUTCDATETIME()), 'CALIFICADA', 83.00, 0, 2);
INSERT INTO ENTREGA (IdAsignacion, IdEstudiante, FechaEntrega, Estado, Calificacion, EsTardia, NumeroIntento)
VALUES (3, 7, DATEADD(DAY, 18, SYSUTCDATETIME()), 'CALIFICADA', 95.00, 0, 1);
INSERT INTO ENTREGA (IdAsignacion, IdEstudiante, FechaEntrega, Estado, EsTardia, NumeroIntento)
VALUES (4, 8, DATEADD(DAY, -2, SYSUTCDATETIME()), 'RECHAZADA', 0, 1);
INSERT INTO ENTREGA (IdAsignacion, IdEstudiante, FechaEntrega, Estado, EsTardia, NumeroIntento)
VALUES (6, 6, SYSUTCDATETIME(), 'RECIBIDA', 0, 1);
GO
-- Archivos (con Contenido de ejemplo en Base64)
INSERT INTO ARCHIVO (IdEntrega, NombreArchivo, RutaArchivo, TipoArchivo, TamanoBytes, Contenido)
VALUES (1, 'calculadora.py', '/entregas/1/calculadora.py', '.py', 1024, 'IyAtKi0gY29kaW5nOiB1dGYtOCAtKi0KIyBDYWxjdWxhZG9yYSBiw6FzaWNhCgpkZWYgc3VtYXIoYSwgYik6CiAgICByZXR1cm4gYSArIGIKCmRlZiByZXN0YXIoYSwgYik6CiAgICByZXR1cm4gYSAtIGIKCmRlZiBtdWx0aXBsaWNhcihhLCBiKToKICAgIHJldHVybiBhICogYgpkZWYgZGl2aWRpcihhLCBiKToKICAgIGlmIGIgPT0gMDoKICAgICAgICByYWlzZSBWYWx1ZUVycm9yKCJObyBzZSBwdWVkZSBkaXZpZGlyIHBvciBjZXJvIikKICAgIHJldHVybiBhIC8gYg==');
INSERT INTO ARCHIVO (IdEntrega, NombreArchivo, RutaArchivo, TipoArchivo, TamanoBytes, Contenido)
VALUES (2, 'calc.py', '/entregas/2/calc.py', '.py', 900, 'IyAtKi0gY29kaW5nOiB1dGYtOCAtKi0KZGVmIHN1bShhLCBiKToKICAgIHJldHVybiBhICsgYgpkZWYgcmVzdChhLCBiKToKICAgIHJldHVybiBhIC0gYgpkZWYgbXVsdChhLCBiKToKICAgIHJldHVybiBhICogYgpkZWYgZGl2KGEsIGIpOgogICAgaWYgYiA9PSAwOiByZXR1cm4gMAogICAgcmV0dXJuIGEgLyBi');
INSERT INTO ARCHIVO (IdEntrega, NombreArchivo, RutaArchivo, TipoArchivo, TamanoBytes, Contenido)
VALUES (3, 'calc_maria.py', '/entregas/3/calc_maria.py', '.py', 1100, 'cHJpbnQoIkNhbGN1bGFkb3JhIikKCmRlZiBzdW1hcihhLCBiKToKICAgIHJldHVybiBhICsgYgpkZWYgcmVzdGFyKGEsIGIpOgogICAgcmV0dXJuIGEgLSBiCmRlZiBtdWx0aXBsaWNhcihhLCBiKToKICAgIHJldHVybiBhICogYgpkZWYgZGl2aWRpcihhLCBiKToKICAgIGlmIGIgIT0gMDoKICAgICAgICByZXR1cm4gYSAvIGIKICAgIHJldHVybiAiRXJyb3Ii');
INSERT INTO ARCHIVO (IdEntrega, NombreArchivo, RutaArchivo, TipoArchivo, TamanoBytes, Contenido)
VALUES (4, 'calc_jose.py', '/entregas/4/calc_jose.py', '.py', 800, 'ZGVmIHN1bShhLCBiKTogcmV0dXJuIGEgKyBiCmRlZiByZXN0KGEsIGIpOiByZXR1cm4gYSAtIGIKZGVmIG11bChhLCBiKTogcmV0dXJuIGEgKiBiCmRlZiBkaXYoYSwgYik6CiAgICBpZiBiID09IDA6IHJldHVybiAiSW52w6FsaWRvIgogICAgcmV0dXJuIGEgLyBi');
INSERT INTO ARCHIVO (IdEntrega, NombreArchivo, RutaArchivo, TipoArchivo, TamanoBytes, Contenido)
VALUES (5, 'pila_cola.py', '/entregas/5/pila_cola.py', '.py', 2000, 'Y2xhc3MgUGlsYToKICAgIGRlZiBfX2luaXRfXyhzZWxmKToKICAgICAgICBzZWxmLml0ZW1zID0gW10KICAgIGRlZiBwdXNoKHNlbGYsIGl0ZW0pOgogICAgICAgIHNlbGYuaXRlbXMuYXBwZW5kKGl0ZW0pCiAgICBkZWYgcG9wKHNlbGYpOgogICAgICAgIHJldHVybiBzZWxmLml0ZW1zLnBvcCgpCgpjbGFzcyBDb2xhOgogICAgZGVmIF9faW5pdF9fKHNlbGYpOgogICAgICAgIHNlbGYuaXRlbXMgPSBbXQogICAgZGVmIGVucXVldWUoc2VsZiwgaXRlbSk6CiAgICAgICAgc2VsZi5pdGVtcy5hcHBlbmQoaXRlbSkKICAgIGRlZiBkZXF1ZXVlKHNlbGYpOgogICAgICAgIHJldHVybiBzZWxmLml0ZW1zLnBvcCgwKQ==');
INSERT INTO ARCHIVO (IdEntrega, NombreArchivo, RutaArchivo, TipoArchivo, TamanoBytes, Contenido)
VALUES (6, 'pila_cola_v2.py', '/entregas/6/pila_cola_v2.py', '.py', 2400, 'Y2xhc3MgU3RhY2s6CiAgICBkZWYgX19pbml0X18oc2VsZik6CiAgICAgICAgc2VsZi5kYXRhID0gW10KICAgIGRlZiBpc19lbXB0eShzZWxmKToKICAgICAgICByZXR1cm4gbGVuKHNlbGYuZGF0YSkgPT0gMAogICAgZGVmIHB1c2goc2VsZiwgaXRlbSk6CiAgICAgICAgc2VsZi5kYXRhLmFwcGVuZChpdGVtKQogICAgZGVmIHBvcChzZWxmKToKICAgICAgICBpZiBzZWxmLmlzX2VtcHR5KCk6IHJhaXNlIEluZGV4RXJyb3IoIkVtcHR5IikKICAgICAgICByZXR1cm4gc2VsZi5kYXRhLnBvcCgpCgpjbGFzcyBRdWV1ZToKICAgIGRlZiBfX2luaXRfXyhzZWxmKToKICAgICAgICBzZWxmLmRhdGEgPSBbXQogICAgZGVmIGVucXVldWUoc2VsZiwgaXRlbSk6CiAgICAgICAgc2VsZi5kYXRhLmFwcGVuZChpdGVtKQogICAgZGVmIGRlcXVldWUoc2VsZik6CiAgICAgICAgaWYgc2VsZi5pc19lbXB0eSgpOiByYWlzZSBJbmRleEVycm9yKCJFbXB0eSIpCiAgICAgICAgcmV0dXJuIHNlbGYuZGF0YS5wb3AoMCkKICAgIGRlZiBpc19lbXB0eShzZWxmKToKICAgICAgICByZXR1cm4gbGVuKHNlbGYuZGF0YSkgPT0gMA==');
INSERT INTO ARCHIVO (IdEntrega, NombreArchivo, RutaArchivo, TipoArchivo, TamanoBytes, Contenido)
VALUES (7, 'linked_list.py', '/entregas/7/linked_list.py', '.py', 3000, 'Y2xhc3MgTm9kZToKICAgIGRlZiBfX2luaXRfXyhzZWxmLCBkYXRhKToKICAgICAgICBzZWxmLmRhdGEgPSBkYXRhCiAgICAgICAgc2VsZi5uZXh0ID0gTm9uZQoKY2xhc3MgTGlua2VkTGlzdDoKICAgIGRlZiBfX2luaXRfXyhzZWxmKToKICAgICAgICBzZWxmLmhlYWQgPSBOb25lCiAgICBkZWYgaW5zZXJ0KHNlbGYsIGRhdGEpOgogICAgICAgIG5ld19ub2RlID0gTm9kZShkYXRhKQogICAgICAgIG5ld19ub2RlLm5leHQgPSBzZWxmLmhlYWQKICAgICAgICBzZWxmLmhlYWQgPSBuZXdfbm9kZQogICAgZGVmIHNlYXJjaChzZWxmLCBkYXRhKToKICAgICAgICBjdXJyID0gc2VsZi5oZWFkCiAgICAgICAgd2hpbGUgY3VycjoKICAgICAgICAgICAgaWYgY3Vyci5kYXRhID09IGRhdGE6CiAgICAgICAgICAgICAgICByZXR1cm4gVHJ1ZQogICAgICAgICAgICBjdXJyID0gY3Vyci5uZXh0CiAgICAgICAgcmV0dXJuIEZhbHNlCiAgICBkZWYgZGVsZXRlKHNlbGYsIGRhdGEpOgogICAgICAgIGN1cnIgPSBzZWxmLmhlYWQKICAgICAgICBwcmV2ID0gTm9uZQogICAgICAgIHdoaWxlIGN1cnI6CiAgICAgICAgICAgIGlmIGN1cnIuZGF0YSA9PSBkYXRhOgogICAgICAgICAgICAgICAgaWYgcHJldjoKICAgICAgICAgICAgICAgICAgICBwcmV2Lm5leHQgPSBjdXJyLm5leHQKICAgICAgICAgICAgICAgIGVsc2U6CiAgICAgICAgICAgICAgICAgICAgc2VsZi5oZWFkID0gY3Vyci5uZXh0CiAgICAgICAgICAgICAgICByZXR1cm4gVHJ1ZQogICAgICAgICAgICBwcmV2ID0gY3VycgogICAgICAgICAgICBjdXJyID0gY3Vyci5uZXh0CiAgICAgICAgcmV0dXJuIEZhbHNl');
INSERT INTO ARCHIVO (IdEntrega, NombreArchivo, RutaArchivo, TipoArchivo, TamanoBytes, Contenido)
VALUES (8, 'stack_queue.py', '/entregas/8/stack_queue.py', '.py', 500, 'I05PVCBXT1JLSU5HCmRlZiBwaWxhKCk6CiAgICBwYXNz');
INSERT INTO ARCHIVO (IdEntrega, NombreArchivo, RutaArchivo, TipoArchivo, TamanoBytes, Contenido)
VALUES (9, 'casos_de_uso.md', '/entregas/9/casos_de_uso.md', '.md', 1500, 'IyBEaWFncmFtYSBkZSBDYXNvcyBkZSBVc28KCiMjIEFjdG9yZXMKLSBBZG1pbmlzdHJhZG9yCi0gRXN0dWRpYW50ZQotIFByb2Zlc29yCgojIyBDYXNvcyBkZSBVc28KMS4gSW5pY2lhciBzZXNpw7NuCi0gQWN0b3I6IEFkbWluaXN0cmFkb3IKLSBQcmVjb25kaWNpw7NuOiBOaW5ndW5hCi0gRmx1am8gYsOhc2ljbwoKMi4gTWF0cmljdWxhcnNlIGVuIGN1cnNvCi0gQWN0b3I6IEVzdHVkaWFudGUKLSBQcmVjb25kaWNpw7NuOiBFc3RhciByZWdpc3RyYWRvCi0gRmx1am8gYWx0ZXJubyAy');
GO
-- Validaciones hash
INSERT INTO VALIDACION_HASH (IdArchivo, Algoritmo, HashCalculado, Valido)
VALUES (1, 'SHA-256', 'a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1', 1),
       (2, 'SHA-256', 'b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2', 1),
       (3, 'SHA-256', 'c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3', 1),
       (4, 'SHA-256', 'd4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3d4', 0),
       (5, 'SHA-256', 'e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3d4e5', 1),
       (6, 'SHA-256', 'f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3d4e5f6', 1),
       (7, 'SHA-256', 'a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3d4e5f6a7', 1),
       (8, 'SHA-256', 'b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3d4e5f6a7b8', 0),
       (9, 'SHA-256', 'c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3d4e5f6a7b8c9', 1);
GO

-- Commits Git
INSERT INTO COMMIT_GIT (IdEntrega, HashCommit, Mensaje, Rama, FechaCommit)
VALUES (1, 'abc123def456abc123def456abc123def456abc1', 'Calculadora b?sica completada', 'main', DATEADD(DAY, 13, SYSUTCDATETIME())),
       (7, 'def789abc012def789abc012def789abc012def7', 'Lista enlazada - implementaci?n completa', 'main', DATEADD(DAY, 18, SYSUTCDATETIME())),
       (6, '123abc456def123abc456def123abc456def123a', 'Refactor pila/cola con manejo de errores', 'develop', DATEADD(DAY, -1, SYSUTCDATETIME()));
GO

-- Casos de prueba
INSERT INTO CASO_PRUEBA (IdAsignacion, Nombre, Descripcion, Entrada, SalidaEsperada, Puntaje)
VALUES
(1, 'Suma', 'Probar suma de dos n?meros', 'sumar(2,3)', '5', 25),
(1, 'Resta', 'Probar resta de dos n?meros', 'restar(10,4)', '6', 25),
(1, 'Multiplicaci?n', 'Probar multiplicaci?n', 'multiplicar(3,7)', '21', 25),
(1, 'Divisi?n', 'Probar divisi?n exacta', 'dividir(15,3)', '5.0', 25),
(3, 'Insertar al inicio', 'Insertar un nodo al inicio de la lista', 'insertar(5)', 'True', 20),
(3, 'Buscar elemento', 'Buscar un elemento existente', 'buscar(5)', 'True', 20),
(3, 'Buscar inexistente', 'Buscar un elemento que no existe', 'buscar(99)', 'False', 20),
(3, 'Eliminar elemento', 'Eliminar un nodo existente', 'eliminar(5)', 'True', 20),
(3, 'Lista vac?a', 'Verificar que b?squeda en lista vac?a retorne False', 'buscar(1)', 'False', 20);
GO

-- Resultados de prueba
INSERT INTO RESULTADO_PRUEBA (IdEntrega, IdCasoPrueba, Aprobado, SalidaObtenida, TiempoEjecucion)
VALUES
(1, 1, 1, '5', 0.012), (1, 2, 1, '6', 0.008),
(1, 3, 1, '21', 0.010), (1, 4, 1, '5.0', 0.015),
(7, 5, 1, 'True', 0.020), (7, 6, 1, 'True', 0.018),
(7, 7, 1, 'False', 0.015), (7, 8, 1, 'True', 0.022),
(7, 9, 1, 'False', 0.011);
GO

-- Retroalimentaci?n
INSERT INTO RETROALIMENTACION (IdEntrega, IdDocente, Comentario, Calificacion)
VALUES
(1, 1, 'Excelente trabajo. La calculadora funciona correctamente y el c?digo est? bien estructurado.', 92.00),
(2, 1, 'Buen intento, pero faltan validaciones. La divisi?n entre cero debe lanzar error.', 78.50),
(3, 1, 'Entreg? tarde, pero el c?digo es funcional. Incluir m?s comentarios.', 65.00),
(7, 1, 'Implementaci?n impecable de lista enlazada. Todos los casos de prueba pasan.', 95.00),
(6, 1, 'Segundo intento mucho mejor. Manejo de errores agregado y c?digo m?s limpio.', 83.00);
GO

PRINT 'Datos de prueba insertados correctamente.';
GO
