/*
    RaceDay - SQL Server Database Script
    Section C - SQL Database Script

    AI USE DISCLOSURE:
    AI assistance was used during the planning/review stage.
    The database structure, constraints, relationships, and sample
    data were reviewed and adapted for the RaceDay requirements.
*/

IF DB_ID('RaceDayDB') IS NULL
BEGIN
    CREATE DATABASE RaceDayDB;
END;
GO

USE RaceDayDB;
GO

/* 1. DROP EXISTING TABLES */

IF OBJECT_ID('dbo.Results', 'U') IS NOT NULL
    DROP TABLE dbo.Results;

IF OBJECT_ID('dbo.Enrolments', 'U') IS NOT NULL
    DROP TABLE dbo.Enrolments;

IF OBJECT_ID('dbo.Routes', 'U') IS NOT NULL
    DROP TABLE dbo.Routes;

IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL
    DROP TABLE dbo.Categories;

IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL
    DROP TABLE dbo.Events;

IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    DROP TABLE dbo.Users;
GO


/* 2. USERS TABLE */

CREATE TABLE dbo.Users
(
    UserId INT IDENTITY(1,1) NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,
    Role NVARCHAR(20) NOT NULL,

    CONSTRAINT PK_Users
        PRIMARY KEY (UserId),

    CONSTRAINT UQ_Users_Email
        UNIQUE (Email),

    CONSTRAINT CK_Users_Role
        CHECK (Role IN ('Organiser', 'Participant'))
);
GO


/* 3. EVENTS TABLE */

CREATE TABLE dbo.Events
(
    EventId INT IDENTITY(1,1) NOT NULL,
    OrganiserId INT NOT NULL,
    Name NVARCHAR(150) NOT NULL,
    Description NVARCHAR(500) NULL,
    EventDate DATE NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    Distance DECIMAL(6,2) NOT NULL,
    EventType NVARCHAR(50) NOT NULL,

    CONSTRAINT PK_Events
        PRIMARY KEY (EventId),

    CONSTRAINT FK_Events_Users
        FOREIGN KEY (OrganiserId)
        REFERENCES dbo.Users(UserId),

    CONSTRAINT CK_Events_Distance
        CHECK (Distance > 0),

    CONSTRAINT CK_Events_EventType
        CHECK (EventType IN ('Running', 'Walking', 'Cycling'))
);
GO


/* 4. CATEGORIES TABLE */

CREATE TABLE dbo.Categories
(
    CategoryId INT IDENTITY(1,1) NOT NULL,
    EventId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(300) NULL,
    EntryFee DECIMAL(10,2) NOT NULL,
    MaximumParticipants INT NOT NULL,

    CONSTRAINT PK_Categories
        PRIMARY KEY (CategoryId),

    CONSTRAINT FK_Categories_Events
        FOREIGN KEY (EventId)
        REFERENCES dbo.Events(EventId),

    CONSTRAINT CK_Categories_EntryFee
        CHECK (EntryFee >= 0),

    CONSTRAINT CK_Categories_MaximumParticipants
        CHECK (MaximumParticipants > 0),

    CONSTRAINT UQ_Categories_Event_Name
        UNIQUE (EventId, Name)
);
GO


/* 5. ENROLMENTS TABLE */

CREATE TABLE dbo.Enrolments
(
    EnrolmentId INT IDENTITY(1,1) NOT NULL,
    ParticipantId INT NOT NULL,
    CategoryId INT NOT NULL,

    EnrolmentDate DATETIME2 NOT NULL
        CONSTRAINT DF_Enrolments_EnrolmentDate
        DEFAULT SYSDATETIME(),

    Status NVARCHAR(30) NOT NULL,

    CONSTRAINT PK_Enrolments
        PRIMARY KEY (EnrolmentId),

    CONSTRAINT FK_Enrolments_Participant
        FOREIGN KEY (ParticipantId)
        REFERENCES dbo.Users(UserId),

    CONSTRAINT FK_Enrolments_Category
        FOREIGN KEY (CategoryId)
        REFERENCES dbo.Categories(CategoryId),

    CONSTRAINT CK_Enrolments_Status
        CHECK (Status IN
        ('Registered', 'Confirmed', 'Cancelled', 'Completed')),

    CONSTRAINT UQ_Enrolments_Participant_Category
        UNIQUE (ParticipantId, CategoryId)
);
GO


/* 6. RESULTS TABLE */

CREATE TABLE dbo.Results
(
    ResultId INT IDENTITY(1,1) NOT NULL,
    EnrolmentId INT NOT NULL,
    FinishTime TIME NULL,
    Position INT NULL,
    Status NVARCHAR(30) NOT NULL,

    CONSTRAINT PK_Results
        PRIMARY KEY (ResultId),

    CONSTRAINT FK_Results_Enrolments
        FOREIGN KEY (EnrolmentId)
        REFERENCES dbo.Enrolments(EnrolmentId),

    CONSTRAINT UQ_Results_Enrolment
        UNIQUE (EnrolmentId),

    CONSTRAINT CK_Results_Position
        CHECK (Position IS NULL OR Position > 0),

    CONSTRAINT CK_Results_Status
        CHECK (Status IN
        ('Finished', 'Did Not Finish', 'Disqualified'))
);
GO


/* 7. ROUTES TABLE */

CREATE TABLE dbo.Routes
(
    RouteId INT IDENTITY(1,1) NOT NULL,
    EventId INT NOT NULL,
    RouteName NVARCHAR(150) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    Description NVARCHAR(500) NULL,

    CONSTRAINT PK_Routes
        PRIMARY KEY (RouteId),

    CONSTRAINT FK_Routes_Events
        FOREIGN KEY (EventId)
        REFERENCES dbo.Events(EventId),

    CONSTRAINT CK_Routes_Distance
        CHECK (DistanceKm > 0)
);
GO


/* 8. SEED USERS */

INSERT INTO dbo.Users
(
    FirstName,
    LastName,
    Email,
    PasswordHash,
    PhoneNumber,
    Role
)
VALUES
(
    'Thabo',
    'Mokoena',
    'thabo.mokoena@raceday.co.za',
    'HASHED_PASSWORD_ORGANISER_001',
    '0825551001',
    'Organiser'
),
(
    'Lerato',
    'Dlamini',
    'lerato.dlamini@raceday.co.za',
    'HASHED_PASSWORD_ORGANISER_002',
    '0825551002',
    'Organiser'
),
(
    'Sipho',
    'Nkosi',
    'sipho.nkosi@example.com',
    'HASHED_PASSWORD_PARTICIPANT_001',
    '0825552001',
    'Participant'
),
(
    'Nomsa',
    'Khumalo',
    'nomsa.khumalo@example.com',
    'HASHED_PASSWORD_PARTICIPANT_002',
    '0825552002',
    'Participant'
);
GO


/* 9. SEED EVENTS */

INSERT INTO dbo.Events
(
    OrganiserId,
    Name,
    Description,
    EventDate,
    Location,
    Distance,
    EventType
)
VALUES
(
    1,
    'Johannesburg City Run',
    'A road running event through selected areas of Johannesburg.',
    '2027-03-14',
    'Johannesburg, Gauteng',
    21.10,
    'Running'
),
(
    1,
    'Cape Town Community Cycle',
    'A community cycling event featuring a scenic Cape Town route.',
    '2027-04-18',
    'Cape Town, Western Cape',
    50.00,
    'Cycling'
),
(
    2,
    'Durban Charity Walk',
    'A community walking event supporting local charity initiatives.',
    '2027-05-09',
    'Durban, KwaZulu-Natal',
    10.00,
    'Walking'
);
GO


/* 10. SEED CATEGORIES */

INSERT INTO dbo.Categories
(
    EventId,
    Name,
    Description,
    EntryFee,
    MaximumParticipants
)
VALUES
(
    1,
    '10 KM Run',
    '10 kilometre road running category.',
    150.00,
    1000
),
(
    1,
    '21 KM Half Marathon',
    '21.1 kilometre half marathon category.',
    250.00,
    1500
),
(
    2,
    '25 KM Cycle',
    '25 kilometre community cycling category.',
    180.00,
    500
),
(
    2,
    '50 KM Cycle',
    '50 kilometre cycling category.',
    300.00,
    800
),
(
    3,
    '5 KM Walk',
    '5 kilometre community walking category.',
    80.00,
    700
),
(
    3,
    '10 KM Walk',
    '10 kilometre charity walking category.',
    120.00,
    700
);
GO


/* 11. SEED ROUTES */

INSERT INTO dbo.Routes
(
    EventId,
    RouteName,
    DistanceKm,
    Description
)
VALUES
(
    1,
    'Johannesburg Central Route',
    10.00,
    'Road route passing through central Johannesburg and surrounding areas.'
),
(
    1,
    'Johannesburg Half Marathon Route',
    21.10,
    'Long-distance road route designed for the half marathon category.'
),
(
    2,
    'Cape Town Coastal Route',
    25.00,
    'Scenic cycling route along selected coastal roads.'
),
(
    2,
    'Cape Town Extended Cycle Route',
    50.00,
    'Extended cycling route covering approximately 50 kilometres.'
),
(
    3,
    'Durban Charity Route',
    5.00,
    'Community walking route through selected areas of Durban.'
),
(
    3,
    'Durban 10 KM Route',
    10.00,
    'Longer charity walking route covering approximately 10 kilometres.'
);
GO


/* 12. SEED ENROLMENTS */

INSERT INTO dbo.Enrolments
(
    ParticipantId,
    CategoryId,
    EnrolmentDate,
    Status
)
VALUES
(
    3,
    1,
    '2027-01-15 09:30:00',
    'Confirmed'
),
(
    3,
    4,
    '2027-02-01 10:15:00',
    'Confirmed'
),
(
    4,
    2,
    '2027-01-20 14:00:00',
    'Confirmed'
),
(
    4,
    5,
    '2027-02-05 11:45:00',
    'Registered'
);
GO


/* 13. SEED RESULTS */

INSERT INTO dbo.Results
(
    EnrolmentId,
    FinishTime,
    Position,
    Status
)
VALUES
(
    1,
    '00:52:35',
    15,
    'Finished'
),
(
    2,
    '01:48:20',
    42,
    'Finished'
);
GO


/* 14. VERIFY TABLE DATA */

SELECT * FROM dbo.Users;

SELECT * FROM dbo.Events;

SELECT * FROM dbo.Categories;

SELECT * FROM dbo.Routes;

SELECT * FROM dbo.Enrolments;

SELECT * FROM dbo.Results;


/* 15. VERIFY RELATIONSHIPS */

SELECT
    e.EnrolmentId,
    u.FirstName + ' ' + u.LastName AS Participant,
    ev.Name AS EventName,
    c.Name AS Category,
    e.EnrolmentDate,
    e.Status AS EnrolmentStatus,
    r.FinishTime,
    r.Position,
    r.Status AS ResultStatus
FROM dbo.Enrolments e
INNER JOIN dbo.Users u
    ON e.ParticipantId = u.UserId
INNER JOIN dbo.Categories c
    ON e.CategoryId = c.CategoryId
INNER JOIN dbo.Events ev
    ON c.EventId = ev.EventId
LEFT JOIN dbo.Results r
    ON e.EnrolmentId = r.EnrolmentId
ORDER BY ev.EventDate, u.LastName;
GO