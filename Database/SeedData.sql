-- ============================================
-- Seed Data for BookingService API
-- ============================================

USE BookingServiceDb;
GO

-- ============================================
-- 1. Create Admin User
-- Password: Admin123! (hashed with BCrypt)
-- ============================================
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@bookingservice.com')
BEGIN
    INSERT INTO Users (Name, Email, PasswordHash, Role, CreatedAt, IsActive)
VALUES (
        'System Administrator',
        'admin@bookingservice.com',
        '$2a$11$YourBCryptHashHere', -- You need to generate this hash
        1, -- Admin role
      GETUTCDATE(),
        1
    );
    PRINT 'Admin user created';
END
GO

-- ============================================
-- 2. Create Test Users
-- Password for all: User123!
-- ============================================
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'user1@test.com')
BEGIN
    INSERT INTO Users (Name, Email, PasswordHash, Role, CreatedAt, IsActive)
    VALUES 
        ('John Doe', 'user1@test.com', '$2a$11$YourBCryptHashHere', 0, GETUTCDATE(), 1),
    ('Jane Smith', 'user2@test.com', '$2a$11$YourBCryptHashHere', 0, GETUTCDATE(), 1),
      ('Mike Johnson', 'user3@test.com', '$2a$11$YourBCryptHashHere', 0, GETUTCDATE(), 1);
    PRINT 'Test users created';
END
GO

-- ============================================
-- 3. Create Sample Resources
-- ============================================
IF NOT EXISTS (SELECT 1 FROM Resources)
BEGIN
    INSERT INTO Resources (Name, Description, IsActive, CreatedAt)
    VALUES 
        ('Sala de Conferencias A', 'Sala grande con capacidad para 20 personas, incluye proyector y pizarra', 1, GETUTCDATE()),
        ('Sala de Reuniones B', 'Sala mediana para 10 personas con videoconferencia', 1, GETUTCDATE()),
        ('Vehículo Corporativo 1', 'Toyota Corolla 2023 - Placa ABC123', 1, GETUTCDATE()),
        ('Vehículo Corporativo 2', 'Honda CR-V 2024 - Placa XYZ789', 1, GETUTCDATE()),
  ('Laptop HP ProBook', 'Laptop empresarial con Windows 11 Pro', 1, GETUTCDATE()),
        ('Proyector Epson', 'Proyector Full HD para presentaciones', 1, GETUTCDATE()),
        ('Cancha de Tenis', 'Cancha de tenis al aire libre', 1, GETUTCDATE()),
        ('Estudio de Grabación', 'Estudio profesional con equipamiento completo', 1, GETUTCDATE());
    PRINT 'Resources created';
END
GO

-- ============================================
-- 4. Create Sample Blocked Times
-- (Maintenance periods for next 30 days)
-- ============================================
DECLARE @Tomorrow DATETIME = DATEADD(DAY, 1, CAST(GETUTCDATE() AS DATE));
DECLARE @ResourceId INT = (SELECT TOP 1 Id FROM Resources WHERE Name LIKE '%Sala de Conferencias A%');

IF @ResourceId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM BlockedTimes WHERE ResourceId = @ResourceId)
BEGIN
    -- Block tomorrow morning for maintenance
    INSERT INTO BlockedTimes (ResourceId, StartTime, EndTime, Reason, CreatedAt)
    VALUES 
   (
      @ResourceId,
            DATEADD(HOUR, 8, @Tomorrow),
DATEADD(HOUR, 10, @Tomorrow),
       'Mantenimiento preventivo de equipos',
            GETUTCDATE()
        );
    PRINT 'Blocked times created';
END
GO

-- ============================================
-- 5. Create Sample Reservations
-- (Active reservations for testing)
-- ============================================
DECLARE @NextWeek DATETIME = DATEADD(DAY, 7, CAST(GETUTCDATE() AS DATE));
DECLARE @UserId INT = (SELECT TOP 1 Id FROM Users WHERE Role = 0); -- Get first regular user
DECLARE @ResId INT = (SELECT TOP 1 Id FROM Resources WHERE Name LIKE '%Sala de Reuniones B%');

IF @UserId IS NOT NULL AND @ResId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Reservations)
BEGIN
    INSERT INTO Reservations (UserId, ResourceId, StartTime, EndTime, Status, CreatedAt)
    VALUES 
        (
     @UserId,
            @ResId,
            DATEADD(HOUR, 14, @NextWeek), -- 2 PM next week
DATEADD(HOUR, 16, @NextWeek), -- 4 PM next week
   0, -- Active
GETUTCDATE()
        );
    PRINT 'Sample reservations created';
END
GO

-- ============================================
-- Verification Queries
-- ============================================
PRINT '============================================';
PRINT 'Database seed completed!';
PRINT '============================================';
PRINT '';

PRINT 'Users Summary:';
SELECT 
    Role,
    COUNT(*) as Count,
    CASE Role WHEN 0 THEN 'User' WHEN 1 THEN 'Admin' END as RoleName
FROM Users
GROUP BY Role;

PRINT '';
PRINT 'Resources Summary:';
SELECT COUNT(*) as TotalResources, SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) as ActiveResources
FROM Resources;

PRINT '';
PRINT 'Reservations Summary:';
SELECT 
    Status,
  COUNT(*) as Count,
    CASE Status 
        WHEN 0 THEN 'Active' 
 WHEN 1 THEN 'Cancelled' 
     WHEN 2 THEN 'Completed' 
 END as StatusName
FROM Reservations
GROUP BY Status;

PRINT '';
PRINT 'Blocked Times Summary:';
SELECT COUNT(*) as TotalBlocked FROM BlockedTimes;

GO
