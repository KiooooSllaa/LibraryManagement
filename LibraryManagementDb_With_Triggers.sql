
-- ================================
-- Database: LibraryManagementDb
-- Updated: Includes triggers for AvailableCopies management
-- ================================

-- Drop tables if they exist
DROP TABLE IF EXISTS PasswordResetTokens;
DROP TABLE IF EXISTS Notifications;
DROP TABLE IF EXISTS BorrowRecords;
DROP TABLE IF EXISTS BookInventory;
DROP TABLE IF EXISTS Books;
DROP TABLE IF EXISTS Authors;
DROP TABLE IF EXISTS Categories;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS Roles;

-- Create Roles table
CREATE TABLE Roles (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL -- Admin, Librarian, User
);

-- Create Users table
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY,
    FullName NVARCHAR(100),
    Email NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(MAX),
    RoleId INT NOT NULL,
    IsGoogleAccount BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);

-- Create Categories table
CREATE TABLE Categories (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100)
);

-- Create Authors table
CREATE TABLE Authors (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100)
);

-- Create Books table
CREATE TABLE Books (
    Id INT PRIMARY KEY IDENTITY,
    Title NVARCHAR(200),
    Description NVARCHAR(MAX),
    CategoryId INT,
    AuthorId INT,
    PublishedYear INT,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    FOREIGN KEY (AuthorId) REFERENCES Authors(Id)
);

-- Create BookInventory table
CREATE TABLE BookInventory (
    BookId INT PRIMARY KEY,
    TotalCopies INT,
    AvailableCopies INT,
    FOREIGN KEY (BookId) REFERENCES Books(Id)
);

-- Create BorrowRecords table
CREATE TABLE BorrowRecords (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT,
    BookId INT,
    BorrowDate DATETIME,
    DueDate DATETIME,
    ReturnDate DATETIME NULL,
    LibrarianId INT,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (BookId) REFERENCES Books(Id),
    FOREIGN KEY (LibrarianId) REFERENCES Users(Id)
);

-- Create Notifications table
CREATE TABLE Notifications (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT,
    Message NVARCHAR(MAX),
    IsRead BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Create PasswordResetTokens table
CREATE TABLE PasswordResetTokens (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT,
    Token NVARCHAR(100),
    Expiration DATETIME,
    IsUsed BIT DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Trigger: Decrease AvailableCopies when a book is borrowed
CREATE TRIGGER trg_DecreaseAvailableCopies
ON BorrowRecords
AFTER INSERT
AS
BEGIN
    UPDATE BookInventory
    SET AvailableCopies = AvailableCopies - 1
    FROM BookInventory bi
    INNER JOIN inserted i ON bi.BookId = i.BookId
END;

-- Trigger: Increase AvailableCopies when a book is returned
CREATE TRIGGER trg_IncreaseAvailableCopies
ON BorrowRecords
AFTER UPDATE
AS
BEGIN
    -- Only apply if ReturnDate was just updated (i.e., not NULL before and now has a value)
    UPDATE BookInventory
    SET AvailableCopies = AvailableCopies + 1
    FROM BookInventory bi
    INNER JOIN inserted i ON bi.BookId = i.BookId
    INNER JOIN deleted d ON i.Id = d.Id
    WHERE d.ReturnDate IS NULL AND i.ReturnDate IS NOT NULL;
END;
