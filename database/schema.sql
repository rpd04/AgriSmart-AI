-- AgriSmart AI core schema (see docs/architecture.md for the ER diagram)
CREATE TABLE Users (
    UserId INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Role NVARCHAR(20) NOT NULL,   -- Farmer, Agronomist, Admin
    Phone NVARCHAR(15),
    Email NVARCHAR(100) UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Region NVARCHAR(100)
);

CREATE TABLE Farms (
    FarmId INT IDENTITY PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    Location NVARCHAR(200),
    AreaInAcres DECIMAL(10,2),
    SoilType NVARCHAR(50)
);

CREATE TABLE CropRecords (
    CropRecordId INT IDENTITY PRIMARY KEY,
    FarmId INT NOT NULL FOREIGN KEY REFERENCES Farms(FarmId),
    CropType NVARCHAR(50),
    SowingDate DATE,
    GrowthStage NVARCHAR(30)
);

CREATE TABLE SoilData (
    SoilDataId INT IDENTITY PRIMARY KEY,
    FarmId INT NOT NULL FOREIGN KEY REFERENCES Farms(FarmId),
    NitrogenPPM DECIMAL(6,2),
    PhosphorusPPM DECIMAL(6,2),
    PotassiumPPM DECIMAL(6,2),
    pH DECIMAL(4,2),
    MoisturePercent DECIMAL(5,2),
    RecordedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE DiseaseReports (
    ReportId INT IDENTITY PRIMARY KEY,
    CropRecordId INT NOT NULL FOREIGN KEY REFERENCES CropRecords(CropRecordId),
    ImageUrl NVARCHAR(300),
    PredictedDisease NVARCHAR(100),
    ConfidenceScore DECIMAL(5,4),
    TreatmentAdvice NVARCHAR(500),
    ReportedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE YieldPredictions (
    PredictionId INT IDENTITY PRIMARY KEY,
    CropRecordId INT NOT NULL FOREIGN KEY REFERENCES CropRecords(CropRecordId),
    PredictedYieldKg DECIMAL(10,2),
    ModelVersion NVARCHAR(20),
    PredictedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE MarketplaceListings (
    ListingId INT IDENTITY PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    CropType NVARCHAR(50),
    QuantityKg DECIMAL(10,2),
    SuggestedPrice DECIMAL(10,2),
    Status NVARCHAR(20) DEFAULT 'Active'
);

CREATE TABLE Orders (
    OrderId INT IDENTITY PRIMARY KEY,
    ListingId INT NOT NULL FOREIGN KEY REFERENCES MarketplaceListings(ListingId),
    BuyerId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    Quantity DECIMAL(10,2),
    AgreedPrice DECIMAL(10,2),
    OrderStatus NVARCHAR(20) DEFAULT 'Pending'
);

CREATE TABLE Notifications (
    NotificationId INT IDENTITY PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    Type NVARCHAR(30),
    Message NVARCHAR(300),
    IsRead BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);
