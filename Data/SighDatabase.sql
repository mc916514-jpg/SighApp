-- SIGH: Sistema Integral de Gestión Hospitalaria
-- Script de Creación de Base de Datos y Estructura de Tablas

USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'SighDb')
BEGIN
    ALTER DATABASE SighDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE SighDb;
END
GO

CREATE DATABASE SighDb;
GO

USE SighDb;
GO

-- 1. Tabla Especialidades
CREATE TABLE Especialidades (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE,
    Descripcion NVARCHAR(500) NULL
);
GO

-- 2. Tabla Pacientes
CREATE TABLE Pacientes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Apellido NVARCHAR(100) NOT NULL,
    Cedula NVARCHAR(20) NOT NULL UNIQUE,
    FechaNacimiento DATE NOT NULL,
    Genero NVARCHAR(10) NOT NULL, -- Masculino, Femenino, Otro
    Telefono NVARCHAR(20) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Direccion NVARCHAR(200) NOT NULL,
    FechaRegistro DATETIME DEFAULT GETDATE()
);
GO

-- 3. Tabla Medicos
CREATE TABLE Medicos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Apellido NVARCHAR(100) NOT NULL,
    Cedula NVARCHAR(20) NOT NULL UNIQUE,
    EspecialidadId INT NOT NULL,
    Telefono NVARCHAR(20) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Activo BIT DEFAULT 1,
    FOREIGN KEY (EspecialidadId) REFERENCES Especialidades(Id) ON DELETE CASCADE
);
GO

-- 4. Tabla Citas
CREATE TABLE Citas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PacienteId INT NOT NULL,
    MedicoId INT NOT NULL,
    FechaHora DATETIME NOT NULL,
    Motivo NVARCHAR(500) NOT NULL,
    Estado NVARCHAR(20) DEFAULT 'Pendiente', -- Pendiente, Realizada, Cancelada
    FOREIGN KEY (PacienteId) REFERENCES Pacientes(Id) ON DELETE CASCADE,
    FOREIGN KEY (MedicoId) REFERENCES Medicos(Id) ON DELETE NO ACTION
);
GO

-- 5. Tabla Diagnosticos
CREATE TABLE Diagnosticos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CitaId INT NOT NULL UNIQUE, -- Una cita tiene máximo un diagnóstico
    Descripcion NVARCHAR(MAX) NOT NULL,
    FechaDiagnostico DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CitaId) REFERENCES Citas(Id) ON DELETE CASCADE
);
GO

-- 6. Tabla Tratamientos
CREATE TABLE Tratamientos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    DiagnosticoId INT NOT NULL,
    Medicamento NVARCHAR(200) NOT NULL,
    Dosis NVARCHAR(100) NOT NULL,
    Frecuencia NVARCHAR(100) NOT NULL,
    Duracion NVARCHAR(100) NOT NULL,
    FOREIGN KEY (DiagnosticoId) REFERENCES Diagnosticos(Id) ON DELETE CASCADE
);
GO

-- INSERT DE DATOS INICIALES (MOCK DATA)

-- Especialidades
INSERT INTO Especialidades (Nombre, Descripcion) VALUES 
('Medicina General', 'Atención médica primaria y preventiva para toda la familia.'),
('Pediatría', 'Especialidad médica enfocada en el cuidado y tratamiento de niños y adolescentes.'),
('Cardiología', 'Diagnóstico y tratamiento de enfermedades del corazón y del sistema circulatorio.'),
('Dermatología', 'Tratamiento de afecciones relacionadas con la piel, el cabello y las uñas.'),
('Ginecología', 'Cuidado de la salud del sistema reproductor femenino y obstetricia.');
GO

-- Pacientes
INSERT INTO Pacientes (Nombre, Apellido, Cedula, FechaNacimiento, Genero, Telefono, Email, Direccion, FechaRegistro) VALUES
('Carlos', 'Mendoza', '1029384756', '1985-04-12', 'Masculino', '555-0192', 'carlos.mendoza@email.com', 'Av. Central #123, Sector Norte', GETDATE() - 30),
('Ana', 'Gómez', '2093847561', '1992-08-22', 'Femenino', '555-0143', 'ana.gomez@email.com', 'Calle Secundaria #45, Urbanización Real', GETDATE() - 25),
('Luis', 'Pérez', '3029485761', '1973-12-05', 'Masculino', '555-0188', 'luis.perez@email.com', 'Pasaje Las Flores #88, Centro Histórico', GETDATE() - 20),
('Sofía', 'Rodríguez', '4092837465', '2015-02-14', 'Femenino', '555-0177', 'sofia.rodriguez@email.com', 'Residencial Los Pinos, Bloque B', GETDATE() - 15),
('María', 'Fernández', '5029384716', '1998-07-30', 'Femenino', '555-0122', 'maria.fernandez@email.com', 'Colonia Miraflores, Calle 3', GETDATE() - 10);
GO

-- Médicos
INSERT INTO Medicos (Nombre, Apellido, Cedula, EspecialidadId, Telefono, Email, Activo) VALUES
('Dr. Eduardo', 'Sánchez', 'MED-1102', 1, '555-0201', 'eduardo.sanchez@saludtotal.com', 1), -- Medicina General
('Dra. Valentina', 'Herrera', 'MED-2204', 2, '555-0202', 'valentina.herrera@saludtotal.com', 1), -- Pediatría
('Dr. Roberto', 'Castillo', 'MED-3306', 3, '555-0203', 'roberto.castillo@saludtotal.com', 1), -- Cardiología
('Dra. Mónica', 'Reyes', 'MED-4408', 4, '555-0204', 'monica.reyes@saludtotal.com', 1); -- Dermatología
GO

-- Citas
INSERT INTO Citas (PacienteId, MedicoId, FechaHora, Motivo, Estado) VALUES
(1, 1, DATEADD(hour, 9, CAST(CONVERT(VARCHAR(10), GETDATE(), 120) AS DATETIME)), 'Chequeo general rutinario por dolores de cabeza.', 'Pendiente'),
(2, 3, DATEADD(hour, 10, CAST(CONVERT(VARCHAR(10), GETDATE(), 120) AS DATETIME)), 'Evaluación cardiológica por arritmia ocasional.', 'Pendiente'),
(4, 2, DATEADD(day, -2, DATEADD(hour, 15, CAST(CONVERT(VARCHAR(10), GETDATE(), 120) AS DATETIME))), 'Fiebre persistente y tos seca.', 'Realizada'),
(3, 4, DATEADD(day, -4, DATEADD(hour, 11, CAST(CONVERT(VARCHAR(10), GETDATE(), 120) AS DATETIME))), 'Consulta por erupción cutánea alérgica.', 'Realizada'),
(5, 1, DATEADD(day, 1, DATEADD(hour, 14, CAST(CONVERT(VARCHAR(10), GETDATE(), 120) AS DATETIME))), 'Dolor de garganta e inflamación amígdalas.', 'Pendiente');
GO

-- Diagnósticos
INSERT INTO Diagnosticos (CitaId, Descripcion, FechaDiagnostico) VALUES
(3, 'Faringoamigdalitis aguda bacteriana. Sin complicaciones respiratorias actuales.', DATEADD(day, -2, DATEADD(hour, 15, CAST(CONVERT(VARCHAR(10), GETDATE(), 120) AS DATETIME)))),
(4, 'Dermatitis de contacto de origen irritante. Se sugiere evitar jabones perfumados.', DATEADD(day, -4, DATEADD(hour, 11, CAST(CONVERT(VARCHAR(10), GETDATE(), 120) AS DATETIME))));
GO

-- Tratamientos
INSERT INTO Tratamientos (DiagnosticoId, Medicamento, Dosis, Frecuencia, Duracion) VALUES
(1, 'Amoxicilina 500mg (Jarabe)', '5 ml', 'Cada 8 horas', '7 días'),
(1, 'Paracetamol 250mg', '4 ml', 'Cada 6 horas en caso de fiebre', '3 días'),
(2, 'Cremol crema (Hidrocortisona 1%)', 'Aplicación fina', 'Cada 12 horas', '5 días'),
(2, 'Cetirizina 10mg', '1 tableta', 'Cada 24 horas por las noches', '10 días');
GO
