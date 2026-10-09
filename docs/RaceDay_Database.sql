CREATE DATABASE RaceDayTest;

USE RaceDayTest;

IF OBJECT_ID('dbo.Results', 'U') IS NOT NULL DROP TABLE dbo.Results;
IF OBJECT_ID('dbo.Enrolments', 'U') IS NOT NULL DROP TABLE dbo.Enrolments;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID('dbo.Venues', 'U') IS NOT NULL DROP TABLE dbo.Venues;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

-- ============================================================
-- CREATE TABLES
-- ============================================================

-- ------------------------------------------------------------
-- 1. USERS Table
-- Stores all user accounts (Organisers and Participants)
-- ------------------------------------------------------------
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    [Role] NVARCHAR(20) NOT NULL CHECK ([Role] IN ('Organiser', 'Participant')),
    DateRegistered DATETIME DEFAULT GETDATE(),
    DateOfBirth DATE,
    Gender NVARCHAR(10) CHECK (Gender IN ('Male', 'Female', 'Other')),
    PhoneNumber NVARCHAR(20)
);
GO

-- ------------------------------------------------------------
-- 2. VENUES Table
-- Stores venue/location information for events
-- ------------------------------------------------------------
CREATE TABLE Venues (
    VenueID INT IDENTITY(1,1) PRIMARY KEY,
    VenueName NVARCHAR(100) NOT NULL,
    AddressLine1 NVARCHAR(200) NOT NULL,
    AddressLine2 NVARCHAR(100),
    City NVARCHAR(50) NOT NULL,
    Province NVARCHAR(50) NOT NULL,
    PostalCode NVARCHAR(10),
    Capacity INT CHECK (Capacity >= 0),
    ParkingAvailable BIT DEFAULT 0,
    WheelchairAccessible BIT DEFAULT 0,
    ContactPerson NVARCHAR(100),
    ContactPhone NVARCHAR(20)
);
GO

-- ------------------------------------------------------------
-- 3. EVENTS Table
-- Stores event details linked to Organiser and Venue
-- ------------------------------------------------------------
CREATE TABLE Events (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    EventName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    EventDate DATETIME NOT NULL,
    StartTime TIME,
    EndTime TIME,
    Location NVARCHAR(200) NOT NULL,
    Distance DECIMAL(5,2) NOT NULL CHECK (Distance > 0),
    EventType NVARCHAR(50) NOT NULL CHECK (EventType IN ('Running', 'Walking', 'Cycling', 'Multi-Sport')),
    OrganiserID INT NOT NULL,
    VenueID INT,
    MaxParticipants INT CHECK (MaxParticipants >= 0),
    EntryFee DECIMAL(10,2) DEFAULT 0.00 CHECK (EntryFee >= 0),
    Status NVARCHAR(20) DEFAULT 'Open' CHECK (Status IN ('Open', 'Closed', 'Cancelled', 'Completed')),
    CreatedDate DATETIME DEFAULT GETDATE(),
    LastModified DATETIME DEFAULT GETDATE(),
    IsPublic BIT DEFAULT 1,
    
    -- Foreign Keys
    CONSTRAINT FK_Events_Organiser 
        FOREIGN KEY (OrganiserID) REFERENCES Users(UserID),
    CONSTRAINT FK_Events_Venue 
        FOREIGN KEY (VenueID) REFERENCES Venues(VenueID)
);
GO

-- Trigger to auto-update LastModified
CREATE TRIGGER trg_Events_LastModified
ON Events
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Events 
    SET LastModified = GETDATE() 
    WHERE EventID IN (SELECT DISTINCT EventID FROM inserted);
END
GO

-- ------------------------------------------------------------
-- 4. CATEGORIES Table
-- Event categories (Elite, Open, Veteran, etc.)
-- ------------------------------------------------------------
CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryName NVARCHAR(50) NOT NULL,
    AgeGroup NVARCHAR(20),
    AgeMin INT CHECK (AgeMin >= 0),
    AgeMax INT CHECK (AgeMax >= AgeMin),
    GenderRestriction NVARCHAR(10) CHECK (GenderRestriction IN ('Male', 'Female', 'None')),
    StartTime TIME,
    EntryFeeMultiplier DECIMAL(3,2) DEFAULT 1.00 CHECK (EntryFeeMultiplier > 0),
    MaxParticipantsPerCategory INT CHECK (MaxParticipantsPerCategory >= 0),
    
    CONSTRAINT FK_Categories_Event 
        FOREIGN KEY (EventID) REFERENCES Events(EventID)
);
GO

-- ------------------------------------------------------------
-- 5. ENROLMENTS Table
-- Links Participants to Events and Categories (Many-to-Many resolver)
-- ------------------------------------------------------------
CREATE TABLE Enrolments (
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) DEFAULT 'Confirmed' CHECK (Status IN ('Confirmed', 'Cancelled', 'Waitlisted', 'Pending')),
    PaymentStatus NVARCHAR(20) DEFAULT 'Pending' CHECK (PaymentStatus IN ('Pending', 'Paid', 'Refunded', 'Failed')),
    PaymentDate DATETIME,
    AmountPaid DECIMAL(10,2),
    BibNumber INT UNIQUE,
    StartTime TIME,
    WaveNumber INT,
    SpecialRequests NVARCHAR(200),
    
    -- Foreign Keys
    CONSTRAINT FK_Enrolments_Participant 
        FOREIGN KEY (ParticipantID) REFERENCES Users(UserID),
    CONSTRAINT FK_Enrolments_Event 
        FOREIGN KEY (EventID) REFERENCES Events(EventID),
    CONSTRAINT FK_Enrolments_Category 
        FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    
    -- Enforce unique enrolment (participant cannot enrol twice in same event)
    CONSTRAINT UQ_Enrolment_Participant_Event 
        UNIQUE (ParticipantID, EventID)
);
GO

-- ------------------------------------------------------------
-- 6. RESULTS Table
-- Stores race results linked to Enrolment (One-to-One)
-- ------------------------------------------------------------
CREATE TABLE Results (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL UNIQUE,  -- One-to-One relationship
    GunTime TIME,                      -- Official time from start gun
    ChipTime TIME,                     -- Net time from crossing start line
    FinishTime TIME,                   -- Overall finish time
    FinishPosition INT CHECK (FinishPosition >= 0),
    OverallPosition INT CHECK (OverallPosition >= 0),
    AgeGroupPosition INT CHECK (AgeGroupPosition >= 0),
    GenderPosition INT CHECK (GenderPosition >= 0),
    WavePosition INT CHECK (WavePosition >= 0),
    PacePerKm DECIMAL(5,2),            -- Minutes per kilometer
    Disqualified BIT DEFAULT 0,
    DisqualificationReason NVARCHAR(200),
    IsVerified BIT DEFAULT 0,
    VerifiedBy INT,                    -- Organiser ID who verified
    VerifiedDate DATETIME,
    Notes NVARCHAR(500),
    
    CONSTRAINT FK_Results_Enrolment 
        FOREIGN KEY (EnrolmentID) REFERENCES Enrolments(EnrolmentID),
    CONSTRAINT FK_Results_VerifiedBy 
        FOREIGN KEY (VerifiedBy) REFERENCES Users(UserID)
);
GO

-- Indexes for performance
CREATE INDEX IX_Enrolments_ParticipantID ON Enrolments(ParticipantID);
CREATE INDEX IX_Enrolments_EventID ON Enrolments(EventID);
CREATE INDEX IX_Enrolments_Status ON Enrolments(Status);
CREATE INDEX IX_Events_OrganiserID ON Events(OrganiserID);
CREATE INDEX IX_Events_EventDate ON Events(EventDate);
CREATE INDEX IX_Results_EnrolmentID ON Results(EnrolmentID);
GO

-- ============================================================
-- SEED DATA
-- ============================================================

-- ------------------------------------------------------------
-- 1. Insert Organisers (2)
-- ------------------------------------------------------------
INSERT INTO Users (FirstName, LastName, Email, PasswordHash, [Role], DateOfBirth, Gender, PhoneNumber)
VALUES 
    ('John', 'Smith', 'john.smith@raceday.co.za', 'hash_8a7f9d3e2c1b', 'Organiser', '1980-05-15', 'Male', '+27 82 123 4567'),
    ('Sarah', 'Johnson', 'sarah.johnson@raceday.co.za', 'hash_5b2c9f8e1d4a', 'Organiser', '1985-08-22', 'Female', '+27 83 987 6543');
GO

-- ------------------------------------------------------------
-- 2. Insert Participants (2+)
-- ------------------------------------------------------------
INSERT INTO Users (FirstName, LastName, Email, PasswordHash, [Role], DateOfBirth, Gender, PhoneNumber)
VALUES 
    ('Michael', 'Brown', 'michael.brown@email.com', 'hash_3a6f2b8d1e9c', 'Participant', '1990-03-10', 'Male', '+27 71 234 5678'),
    ('Emma', 'Davis', 'emma.davis@email.com', 'hash_4c7g1e9f2d5b', 'Participant', '1995-07-19', 'Female', '+27 72 345 6789'),
    ('Thabo', 'Nkosi', 'thabo.nkosi@email.com', 'hash_9h8g7f6e5d4c', 'Participant', '1988-11-02', 'Male', '+27 73 456 7890'),
    ('Priya', 'Naidoo', 'priya.naidoo@email.com', 'hash_1a2b3c4d5e6f', 'Participant', '1992-04-25', 'Female', '+27 74 567 8901');
GO

-- ------------------------------------------------------------
-- 3. Insert Venues (3)
-- ------------------------------------------------------------
INSERT INTO Venues (VenueName, AddressLine1, City, Province, PostalCode, Capacity, ParkingAvailable, WheelchairAccessible)
VALUES 
    ('Cape Town Stadium', 'Green Point, Stadium Drive', 'Cape Town', 'Western Cape', '8001', 55000, 1, 1),
    ('Durban Beachfront', 'Snell Parade, Durban Beach', 'Durban', 'KwaZulu-Natal', '4001', 20000, 1, 1),
    ('Johannesburg CBD', 'Jan Smuts Avenue, Braamfontein', 'Johannesburg', 'Gauteng', '2001', 15000, 1, 0);
GO

-- ------------------------------------------------------------
-- 4. Insert Events (3)
-- ------------------------------------------------------------
INSERT INTO Events (EventName, Description, EventDate, StartTime, EndTime, Location, Distance, EventType, OrganiserID, VenueID, MaxParticipants, EntryFee, Status, IsPublic)
VALUES 
    ('Cape Town Marathon', 'Scenic marathon through Cape Town with breathtaking ocean and mountain views. The event includes a full marathon, half marathon, and 10km races.', 
     '2026-10-15', '06:00:00', '14:00:00', 'Cape Town Stadium', 42.20, 'Running', 1, 1, 5000, 350.00, 'Open', 1),
    
    ('Durban Summer Walk', 'Beautiful coastal walk along the Durban beachfront. A family-friendly event with 5km, 10km, and 15km options.', 
     '2026-12-01', '07:00:00', '12:00:00', 'Durban Beachfront', 15.00, 'Walking', 2, 2, 1000, 150.00, 'Open', 1),
    
    ('Johannesburg Cycle Classic', 'Urban cycling event through the streets of Johannesburg. Categories for Elite, Open, and Veteran cyclists.', 
     '2026-11-20', '05:30:00', '11:00:00', 'Johannesburg CBD', 60.00, 'Cycling', 1, 3, 2000, 250.00, 'Open', 1);
GO

-- ------------------------------------------------------------
-- 5. Insert Categories for Events
-- ------------------------------------------------------------

-- Cape Town Marathon Categories (Event 1)
INSERT INTO Categories (EventID, CategoryName, AgeGroup, AgeMin, AgeMax, GenderRestriction, StartTime, EntryFeeMultiplier)
VALUES 
    (1, 'Elite Men', '18-39', 18, 39, 'Male', '06:00:00', 1.00),
    (1, 'Elite Women', '18-39', 18, 39, 'Female', '06:00:00', 1.00),
    (1, 'Veteran Men', '40-49', 40, 49, 'Male', '06:10:00', 0.90),
    (1, 'Veteran Women', '40-49', 40, 49, 'Female', '06:10:00', 0.90),
    (1, 'Masters Men', '50+', 50, 99, 'Male', '06:20:00', 0.80),
    (1, 'Masters Women', '50+', 50, 99, 'Female', '06:20:00', 0.80),
    (1, 'Open Men', '18+', 18, 99, 'Male', '06:30:00', 0.85),
    (1, 'Open Women', '18+', 18, 99, 'Female', '06:30:00', 0.85);

-- Durban Summer Walk Categories (Event 2)
INSERT INTO Categories (EventID, CategoryName, AgeGroup, AgeMin, AgeMax, GenderRestriction, StartTime, EntryFeeMultiplier)
VALUES 
    (2, 'Open', '18-39', 18, 39, 'None', '07:00:00', 1.00),
    (2, 'Veteran', '40+', 40, 99, 'None', '07:15:00', 0.90),
    (2, 'Junior', '12-17', 12, 17, 'None', '07:30:00', 0.60),
    (2, 'Family', 'All Ages', 0, 99, 'None', '08:00:00', 0.50);

-- Johannesburg Cycle Classic Categories (Event 3)
INSERT INTO Categories (EventID, CategoryName, AgeGroup, AgeMin, AgeMax, GenderRestriction, StartTime, EntryFeeMultiplier)
VALUES 
    (3, 'Elite', '18-39', 18, 39, 'None', '05:30:00', 1.00),
    (3, 'Veteran', '40+', 40, 99, 'None', '05:45:00', 0.90),
    (3, 'Open', '18+', 18, 99, 'None', '06:00:00', 0.85);
GO

-- ------------------------------------------------------------
-- 6. Insert Enrolments
-- ------------------------------------------------------------
INSERT INTO Enrolments (ParticipantID, EventID, CategoryID, Status, PaymentStatus, PaymentDate, AmountPaid, BibNumber, StartTime, WaveNumber)
VALUES 
    -- Michael Brown's Enrolments
    (3, 1, 1, 'Confirmed', 'Paid', '2026-09-01 10:30:00', 350.00, 101, '06:00:00', 1),   -- Elite Men - Cape Town Marathon
    (3, 2, 9, 'Confirmed', 'Paid', '2026-09-15 14:20:00', 150.00, 201, '07:00:00', 1),   -- Open - Durban Summer Walk
    (3, 3, 12, 'Confirmed', 'Paid', '2026-09-20 09:15:00', 250.00, 301, '05:30:00', 1),  -- Elite - Joburg Cycle Classic
    
    -- Emma Davis's Enrolments
    (4, 1, 2, 'Confirmed', 'Paid', '2026-09-02 11:45:00', 350.00, 102, '06:00:00', 2),   -- Elite Women - Cape Town Marathon
    (4, 3, 12, 'Confirmed', 'Pending', NULL, 250.00, 302, '05:30:00', 1),               -- Elite - Joburg Cycle Classic
    
    -- Thabo Nkosi's Enrolments
    (5, 1, 3, 'Confirmed', 'Paid', '2026-09-10 08:00:00', 315.00, 103, '06:10:00', 1),   -- Veteran Men - Cape Town Marathon
    (5, 2, 9, 'Confirmed', 'Paid', '2026-09-18 12:30:00', 150.00, 202, '07:00:00', 2),   -- Open - Durban Summer Walk
    
    -- Priya Naidoo's Enrolments
    (6, 1, 4, 'Confirmed', 'Paid', '2026-09-12 16:20:00', 315.00, 104, '06:10:00', 1),   -- Veteran Women - Cape Town Marathon
    (6, 2, 10, 'Confirmed', 'Paid', '2026-09-22 10:10:00', 135.00, 203, '07:15:00', 1),  -- Veteran - Durban Summer Walk
    (6, 3, 12, 'Confirmed', 'Paid', '2026-09-25 14:45:00', 250.00, 303, '05:30:00', 2);  -- Elite - Joburg Cycle Classic
GO

-- ------------------------------------------------------------
-- 7. Insert Results
-- ------------------------------------------------------------
INSERT INTO Results (EnrolmentID, GunTime, ChipTime, FinishTime, FinishPosition, OverallPosition, AgeGroupPosition, GenderPosition, WavePosition, PacePerKm, Disqualified, IsVerified, VerifiedBy, VerifiedDate, Notes)
VALUES 
    -- Cape Town Marathon Results
    (1, '02:45:30', '02:45:15', '02:45:30', 1, 1, 1, 1, 1, 3.92, 0, 1, 1, '2026-10-16 08:00:00', 'Course record!'),
    (2, '03:15:45', '03:15:20', '03:15:45', 2, 2, 1, 1, 2, 4.63, 0, 1, 1, '2026-10-16 08:05:00', 'Personal best'),
    (4, '02:50:10', '02:49:55', '02:50:10', 1, 3, 2, 1, 1, 4.03, 0, 0, NULL, NULL, NULL),
    (5, '03:45:20', '03:45:00', '03:45:20', 3, 8, 2, 2, 1, 5.34, 0, 0, NULL, NULL, NULL),
    
    -- Durban Summer Walk Results
    (3, '01:45:20', '01:45:10', '01:45:20', 1, 1, 1, 1, 1, 7.02, 0, 1, 1, '2026-12-02 09:00:00', 'First place overall'),
    (6, '01:55:45', '01:55:30', '01:55:45', 2, 2, 2, 2, 1, 7.72, 0, 0, NULL, NULL, NULL),
    (7, '01:50:30', '01:50:15', '01:50:30', 1, 1, 1, 1, 1, 7.37, 0, 0, NULL, NULL, NULL),
    
    -- Johannesburg Cycle Classic Results
    (8, '02:30:15', '02:30:00', '02:30:15', 1, 1, 1, 1, 1, 2.50, 0, 0, NULL, NULL, NULL),
    (9, '02:45:30', '02:45:10', '02:45:30', 2, 2, 1, 1, 1, 2.76, 0, 0, NULL, NULL, NULL),
    (10, '02:55:00', '02:54:40', '02:55:00', 3, 3, 2, 2, 2, 2.92, 0, 0, NULL, NULL, NULL);
GO

-- ============================================================
-- VERIFY DATA
-- ============================================================

-- Display counts
SELECT 'Users' AS TableName, COUNT(*) AS RecordCount FROM Users
UNION ALL
SELECT 'Venues', COUNT(*) FROM Venues
UNION ALL
SELECT 'Events', COUNT(*) FROM Events
UNION ALL
SELECT 'Categories', COUNT(*) FROM Categories
UNION ALL
SELECT 'Enrolments', COUNT(*) FROM Enrolments
UNION ALL
SELECT 'Results', COUNT(*) FROM Results;
GO

-- Display sample data
SELECT * FROM Users;
SELECT * FROM Venues;
SELECT * FROM Events;
SELECT * FROM Categories;
SELECT * FROM Enrolments;
SELECT * FROM Results;
GO

-- Check counts
SELECT 'Users' AS Table, COUNT(*) AS Count FROM Users
UNION ALL SELECT 'Venues', COUNT(*) FROM Venues
UNION ALL SELECT 'Events', COUNT(*) FROM Events
UNION ALL SELECT 'Categories', COUNT(*) FROM Categories
UNION ALL SELECT 'Enrolments', COUNT(*) FROM Enrolments
UNION ALL SELECT 'Results', COUNT(*) FROM Results;

-- View sample data
SELECT * FROM Users;
SELECT * FROM Events;
SELECT * FROM Enrolments;
SELECT * FROM Results;