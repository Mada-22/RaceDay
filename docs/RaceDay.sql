CREATE DATABASE RaceDay; 
GO 
USE RaceDay; GO

CREATE TABLE Account ( 
AccountID INT IDENTITY(1,1) , 
UserName VARCHAR(55) NOT NULL , 
PasswordHash VARCHAR(255) NOT NULL, 
UserEmail VARCHAR(200) NOT NULL , 
AccountType VARCHAR(20) NOT NULL, 


CONSTRAINT PK_Account_AccountID PRIMARY KEY(AccountID),

CONSTRAINT UQ_Account_UserName UNIQUE(UserName),

CONSTRAINT UQ_Account_UserEmail UNIQUE(UserEmail),

CONSTRAINT CK_Account_Type CHECK 
(AccountType IN ('Organizer', 'Participant')) ); 
GO

CREATE TABLE Organizer ( 
OrganizerID INT IDENTITY(1,1) , 
OrganizationName VARCHAR(100) NOT NULL, 
Address VARCHAR(200) NOT NULL, 
PhoneNumber VARCHAR(20) NOT NULL, 
Email VARCHAR(100) NOT NULL, 
AccountID INT NOT NULL ,

CONSTRAINT PK_Organizer_OrganizerID PRIMARY KEY (OrganizerID),

CONSTRAINT UQ_Organizer_AccountID UNIQUE(AccountID),

CONSTRAINT FK_Organizer_Account 
FOREIGN KEY (AccountID) 
REFERENCES Account(AccountID) 
ON DELETE NO ACTION
); 
GO


CREATE TABLE Participant ( 
ParticipantID INT IDENTITY(1,1) , 
Name VARCHAR(50) NOT NULL, 
Surname VARCHAR(50) NOT NULL, 
Age INT NOT NULL, 
PhoneNumber VARCHAR(20) NOT NULL, 
AccountID INT NOT NULL, 

CONSTRAINT PK_Participant_ParticipantID PRIMARY KEY(ParticipantID),

CONSTRAINT UQ_Participant_AccountID UNIQUE (AccountID),

CONSTRAINT CK_Participant_Age CHECK (Age >=18),

CONSTRAINT FK_Participant_Account 
FOREIGN KEY (AccountID) REFERENCES 
Account(AccountID)
ON DELETE NO ACTION)
; 
GO

CREATE TABLE Event
(
    EventID INT IDENTITY(1,1),
    Location VARCHAR(150) NOT NULL,
    OpenDate DATE NOT NULL,
    ClosingDate DATE NOT NULL,
    OrganizerID INT NOT NULL,

    CONSTRAINT PK_Event_EventID PRIMARY KEY (EventID),

    CONSTRAINT FK_Event_Organizer
        FOREIGN KEY (OrganizerID)
        REFERENCES Organizer(OrganizerID)
        ON DELETE NO ACTION
);
GO


CREATE TABLE Route
(
    RouteID INT IDENTITY(1,1) ,
    EventID INT NOT NULL,
    RouteName VARCHAR(100) NOT NULL,
    Difficulty VARCHAR(30) NOT NULL,
    Distance DECIMAL(6,2) NOT NULL,

    CONSTRAINT PK_RouteID PRIMARY KEY(RouteID),

    CONSTRAINT FK_Route_Event
        FOREIGN KEY (EventID)
        REFERENCES Event(EventID)
        ON DELETE NO ACTION
);
GO


CREATE TABLE Category
(
    CategoryID INT IDENTITY(1,1),
    CategoryName VARCHAR(50) NOT NULL,
    
    CONSTRAINT PK_Category_CategoryID PRIMARY KEY (CategoryID) 
);
GO

CREATE TABLE Event_Category
(
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,

    CONSTRAINT PK_Event_Category
        PRIMARY KEY (EventID, CategoryID),

    CONSTRAINT FK_EventCategory_Event
        FOREIGN KEY (EventID)
        REFERENCES Event(EventID)
        ON DELETE NO ACTION,

    CONSTRAINT FK_EventCategory_Category
        FOREIGN KEY (CategoryID)
        REFERENCES Category(CategoryID)
        ON DELETE NO ACTION
);
GO

CREATE TABLE Entry
(
    EntryID INT IDENTITY(1,1),
    EventID INT NOT NULL,
    ParticipantID INT NOT NULL,
    Price DECIMAL(10,2) NOT NULL,

    CONSTRAINT PK_Entry PRIMARY KEY(EntryID),

    CONSTRAINT FK_Entry_Event
        FOREIGN KEY (EventID)
        REFERENCES Event(EventID)
        ON DELETE NO ACTION,

    CONSTRAINT FK_Entry_Participant
        FOREIGN KEY (ParticipantID)
        REFERENCES Participant(ParticipantID)
        ON DELETE NO ACTION,

    CONSTRAINT UQ_Entry_Participant_Event
        UNIQUE (EventID, ParticipantID)
        
);
GO

CREATE TABLE Result
(
    ResultID INT IDENTITY(1,1),
    EntryID INT NOT NULL,
    TimeCompleted TIME NOT NULL,
    CategoryID INT NOT NULL,

    CONSTRAINT PK_Result_ResultID PRIMARY KEY(ResultID),

    CONSTRAINT FK_Result_Entry
        FOREIGN KEY (EntryID)
        REFERENCES Entry(EntryID)
        ON DELETE NO ACTION,

    CONSTRAINT FK_Result_Category
        FOREIGN KEY (CategoryID)
        REFERENCES Category(CategoryID)
        ON DELETE NO ACTION
);
GO


INSERT INTO Account
    (UserName, PasswordHash, UserEmail, AccountType)
VALUES
    ('organizer1', 'HASH_1', 'CTPR@ctroadrunners.co.za', 'Organizer'),
    ('organizer2', 'HASH_2', 'RSA@runsouthafrica.co.za', 'Organizer'),
    ('organizer3', 'HASH_3', 'WCCC@wccycling.co.za', 'Organizer'),

    ('matthew', 'HASH_4', 'matthew@email.com', 'Participant'),
    ('james', 'HASH_5', 'james@email.com', 'Participant'),
    ('sarah', 'HASH_6', 'sarah@email.com', 'Participant'),
    ('liam', 'HASH_7', 'liam@email.com', 'Participant'),
    ('thandi', 'HASH_8', 'thandi@email.com', 'Participant'),
    ('daniel', 'HASH_9', 'daniel@email.com', 'Participant'),
    ('amy', 'HASH_10', 'amy@email.com', 'Participant'),
    ('michael', 'HASH_11', 'michael@email.com', 'Participant');
GO

INSERT INTO Organizer
    (OrganizationName, Address, PhoneNumber, Email, AccountID)
VALUES
    ('Cape Town Road Runners',
     '12 Main Road, Cape Town',
     '0215551234',
     'CTPR@ctroadrunners.co.za',
     1),

    ('Run South Africa',
     '45 Long Street, Cape Town',
     '0215555678',
     'RSA@runsouthafrica.co.za',
     2),

    ('Western Cape Cycling Club',
     '78 Beach Road, Cape Town',
     '0215559876',
     'WCCC@wccycling.co.za',
     3);
GO

INSERT INTO Participant
    (Name, Surname, Age, PhoneNumber, AccountID)
VALUES
    ('Matthew', 'Davids', 20, '0821111111', 4),
    ('James', 'Smith', 25, '0822222222', 5),
    ('Sarah', 'Williams', 22, '0823333333', 6),
    ('Liam', 'Jones', 31, '0824444444', 7),
    ('Thandi', 'Mokoena', 28, '0825555555', 8),
    ('Daniel', 'Naidoo', 35, '0826666666', 9),
    ('Amy', 'Peters', 19, '0827777777', 10),
    ('Michael', 'Adams', 42, '0828888888', 11);
GO

INSERT INTO Event
    (Location, OpenDate, ClosingDate, OrganizerID)
VALUES
    ('Cape Town Stadium',
     '2026-09-20',
     '2026-09-15',
     1),

    ('Sea Point Promenade',
     '2026-10-10',
     '2026-10-05',
     2),

    ('Table Mountain',
     '2026-11-15',
     '2026-11-10',
     3);
GO


INSERT INTO Route
    (EventID, RouteName, Difficulty, Distance)
VALUES
    (1, 'Cape Town 10K Route', 'Easy', 10.00),
    (1, 'Cape Town Half Marathon Route', 'Hard', 21.10),

    (2, 'Sea Point 5K Route', 'Easy', 5.00),
    (2, 'Sea Point 10K Route', 'Moderate', 10.00),

    (3, 'Table Mountain Challenge', 'Hard', 25.00);
GO


INSERT INTO Category
    (CategoryName)
VALUES
    ('Junior Male'),
    ('Junior Female'),
    ('Senior Male'),
    ('Senior Female'),
    ('Veteran Male'),
    ('Veteran Female');
GO


INSERT INTO Event_Category
    (EventID, CategoryID)
VALUES
    (1, 1),
    (1, 2),
    (1, 3),
    (1, 4),
    (1, 5),
    (1, 6),

    (2, 1),
    (2, 2),
    (2, 3),
    (2, 4),
    (2, 5),
    (2, 6),

    (3, 3),
    (3, 4),
    (3, 5),
    (3, 6);
GO


INSERT INTO Entry
    (EventID, ParticipantID, Price)
VALUES
    (1, 1, 150.00),
    (1, 2, 150.00),
    (1, 3, 200.00),
    (1, 4, 200.00),

    (2, 1, 100.00),
    (2, 5, 100.00),
    (2, 6, 120.00),
    (2, 7, 100.00),

    (3, 8, 250.00),
    (3, 6, 250.00);
GO

INSERT INTO Result
    (EntryID, TimeCompleted, CategoryID)
VALUES
    (1, '00:52:34', 2),
    (2, '00:48:21', 2),
    (3, '01:45:12', 3),
    (4, '01:52:43', 3),

    (5, '00:25:18', 1),
    (6, '00:29:42', 1),
    (7, '00:55:31', 3),
    (8, '00:31:12', 1),

    (9, '02:15:43', 5),
    (10, '02:31:22', 5);
GO


