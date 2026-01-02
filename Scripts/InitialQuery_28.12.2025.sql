CREATE DATABASE KnowHowTraining

USE KnowHowTraining;
-- Complete SQL Server database schema for bilingual coaching CMS
-- Supports all required pages, sections, programs, trainers, testimonials, etc.
-- Multilingual (MK/EN) with structured content delivery [web:21][web:26][web:29]

-- =============================================
-- CORE TABLES
-- =============================================

-- Languages (en, mk)
CREATE TABLE [Languages] (
    [Id] TINYINT IDENTITY(1,1) PRIMARY KEY,
    [Code] NVARCHAR(5) NOT NULL UNIQUE, -- 'en', 'mk'
    [Name] NVARCHAR(50) NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDefault] BIT NOT NULL DEFAULT 0,
    [Direction] NVARCHAR(10) DEFAULT 'ltr' -- ltr/rtl
);

-- Pages (home, about, programs, etc.)
CREATE TABLE [Pages] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Slug] NVARCHAR(100) NOT NULL UNIQUE,
    [SortOrder] INT NOT NULL DEFAULT 0,
    [IsPublished] BIT NOT NULL DEFAULT 0,
    [IsMenuItem] BIT NOT NULL DEFAULT 1,
    [MenuSortOrder] INT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Global settings
CREATE TABLE [Settings] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Key] NVARCHAR(100) NOT NULL UNIQUE,
    [Value] NVARCHAR(MAX) NULL,
    [Description] NVARCHAR(500) NULL
);

-- =============================================
-- MULTILINGUAL CONTENT
-- =============================================

-- Page content per language (structured JSON sections)
CREATE TABLE [PageContents] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [PageId] INT NOT NULL FOREIGN KEY REFERENCES [Pages]([Id]) ON DELETE CASCADE,
    [LanguageId] TINYINT NOT NULL FOREIGN KEY REFERENCES [Languages]([Id]),
    [SeoTitle] NVARCHAR(200) NULL,
    [SeoDescription] NVARCHAR(500) NULL,
    [SeoKeywords] NVARCHAR(500) NULL,
    [HeroTitle] NVARCHAR(200) NULL,
    [HeroSubtitle] NVARCHAR(500) NULL,
    [HeroCtaText] NVARCHAR(100) NULL,
    [HeroCtaUrl] NVARCHAR(500) NULL,
    [ContentSections] NVARCHAR(MAX) NOT NULL, -- JSON: [{"type":"programs","title":"...","items":[...]}]
    [OpenGraphTitle] NVARCHAR(200) NULL,
    [OpenGraphDescription] NVARCHAR(500) NULL,
    [OpenGraphImage] NVARCHAR(500) NULL,
    [IsPublished] BIT NOT NULL DEFAULT 0,
    [PublishedAt] DATETIME2 NULL,
    UNIQUE([PageId], [LanguageId])
);

-- =============================================
-- PROGRAMS & TRAINING
-- =============================================

-- Program categories (Leadership, Team Building, etc.)
CREATE TABLE [ProgramCategories] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [SortOrder] INT NOT NULL DEFAULT 0
);

-- Category translations
CREATE TABLE [ProgramCategoryTranslations] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [CategoryId] INT NOT NULL FOREIGN KEY REFERENCES [ProgramCategories]([Id]) ON DELETE CASCADE,
    [LanguageId] TINYINT NOT NULL FOREIGN KEY REFERENCES [Languages]([Id]),
    [Name] NVARCHAR(150) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    UNIQUE([CategoryId], [LanguageId])
);

-- Training programs
CREATE TABLE [Programs] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [CategoryId] INT NULL FOREIGN KEY REFERENCES [ProgramCategories]([Id]),
    [Slug] NVARCHAR(150) NOT NULL,
    [DurationHours] DECIMAL(4,1) NULL,
    [MaxParticipants] INT NULL,
    [PriceRangeMin] DECIMAL(10,2) NULL,
    [PriceRangeMax] DECIMAL(10,2) NULL,
    [ImageUrl] NVARCHAR(500) NULL,
    [PdfOutlineUrl] NVARCHAR(500) NULL,
    [SortOrder] INT NOT NULL DEFAULT 0,
    [IsPublished] BIT NOT NULL DEFAULT 0
);

-- Program translations
CREATE TABLE [ProgramTranslations] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [ProgramId] INT NOT NULL FOREIGN KEY REFERENCES [Programs]([Id]) ON DELETE CASCADE,
    [LanguageId] TINYINT NOT NULL FOREIGN KEY REFERENCES [Languages]([Id]),
    [Title] NVARCHAR(200) NOT NULL,
    [ShortDescription] NVARCHAR(500) NULL,
    [FullDescription] NVARCHAR(MAX) NULL,
    [TargetAudience] NVARCHAR(MAX) NULL,
    [LearningOutcomes] NVARCHAR(MAX) NULL,
    UNIQUE([ProgramId], [LanguageId])
);

-- =============================================
-- TRAINERS
-- =============================================

CREATE TABLE [Trainers] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [ImageUrl] NVARCHAR(500) NULL,
    [LinkedinUrl] NVARCHAR(500) NULL,
    [SortOrder] INT NOT NULL DEFAULT 0,
    [IsPublished] BIT NOT NULL DEFAULT 0
);

CREATE TABLE [TrainerTranslations] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [TrainerId] INT NOT NULL FOREIGN KEY REFERENCES [Trainers]([Id]) ON DELETE CASCADE,
    [LanguageId] TINYINT NOT NULL FOREIGN KEY REFERENCES [Languages]([Id]),
    [FullName] NVARCHAR(150) NOT NULL,
    [Title] NVARCHAR(150) NULL,
    [Bio] NVARCHAR(MAX) NULL,
    [ExperienceYears] INT NULL,
    UNIQUE([TrainerId], [LanguageId])
);

-- =============================================
-- TESTIMONIALS & REFERENCES
-- =============================================

CREATE TABLE [Testimonials] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [AuthorImageUrl] NVARCHAR(500) NULL,
    [AuthorPosition] NVARCHAR(200) NULL,
    [CompanyLogoUrl] NVARCHAR(500) NULL,
    [SortOrder] INT NOT NULL DEFAULT 0,
    [IsPublished] BIT NOT NULL DEFAULT 0
);

CREATE TABLE [TestimonialTranslations] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [TestimonialId] INT NOT NULL FOREIGN KEY REFERENCES [Testimonials]([Id]) ON DELETE CASCADE,
    [LanguageId] TINYINT NOT NULL FOREIGN KEY REFERENCES [Languages]([Id]),
    [Quote] NVARCHAR(MAX) NOT NULL,
    [AuthorName] NVARCHAR(150) NOT NULL,
    UNIQUE([TestimonialId], [LanguageId])
);

-- Company references (logos)
CREATE TABLE [CompanyReferences] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [LogoUrl] NVARCHAR(500) NOT NULL,
    [WebsiteUrl] NVARCHAR(500) NULL,
    [SortOrder] INT NOT NULL DEFAULT 0,
    [IsPublished] BIT NOT NULL DEFAULT 0
);

CREATE TABLE [CompanyReferenceTranslations] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [CompanyId] INT NOT NULL FOREIGN KEY REFERENCES [CompanyReferences]([Id]) ON DELETE CASCADE,
    [LanguageId] TINYINT NOT NULL FOREIGN KEY REFERENCES [Languages]([Id]),
    [CompanyName] NVARCHAR(200) NOT NULL,
    UNIQUE([CompanyId], [LanguageId])
);

-- =============================================
-- GALLERY & DOWNLOADS
-- =============================================

CREATE TABLE [GalleryItems] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [ImageUrl] NVARCHAR(500) NOT NULL,
    [ThumbnailUrl] NVARCHAR(500) NULL,
    [SortOrder] INT NOT NULL DEFAULT 0
);

CREATE TABLE [GalleryItemTranslations] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [GalleryItemId] INT NOT NULL FOREIGN KEY REFERENCES [GalleryItems]([Id]) ON DELETE CASCADE,
    [LanguageId] TINYINT NOT NULL FOREIGN KEY REFERENCES [Languages]([Id]),
    [Caption] NVARCHAR(500) NULL,
    UNIQUE([GalleryItemId], [LanguageId])
);

CREATE TABLE [DownloadItems] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [FileUrl] NVARCHAR(500) NOT NULL,
    [FileSizeKb] INT NULL,
    [SortOrder] INT NOT NULL DEFAULT 0,
    [IsPublished] BIT NOT NULL DEFAULT 0
);

CREATE TABLE [DownloadItemTranslations] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [DownloadId] INT NOT NULL FOREIGN KEY REFERENCES [DownloadItems]([Id]) ON DELETE CASCADE,
    [LanguageId] TINYINT NOT NULL FOREIGN KEY REFERENCES [Languages]([Id]),
    [Title] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    UNIQUE([DownloadId], [LanguageId])
);

-- =============================================
-- CONTACT FORM SUBMISSIONS
-- =============================================

CREATE TABLE [ContactSubmissions] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [FullName] NVARCHAR(150) NOT NULL,
    [Email] NVARCHAR(200) NOT NULL,
    [Phone] NVARCHAR(50) NULL,
    [CompanyName] NVARCHAR(200) NULL,
    [CompanySize] NVARCHAR(50) NULL, -- '1-10', '11-50', etc.
    [ProgramInterest] NVARCHAR(200) NULL,
    [Message] NVARCHAR(MAX) NULL,
    [Language] NVARCHAR(5) NULL,
    [IpAddress] NVARCHAR(45) NULL,
    [UserAgent] NVARCHAR(500) NULL,
    [SubmittedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [IsRead] BIT NOT NULL DEFAULT 0
);

-- =============================================
-- RELATIONSHIPS (Many-to-Many)
-- =============================================

-- Page-specific programs (for home page programs overview)
CREATE TABLE [PagePrograms] (
    [PageId] INT NOT NULL FOREIGN KEY REFERENCES [Pages]([Id]) ON DELETE CASCADE,
    [ProgramId] INT NOT NULL FOREIGN KEY REFERENCES [Programs]([Id]) ON DELETE CASCADE,
    [SortOrder] INT NOT NULL DEFAULT 0,
    PRIMARY KEY ([PageId], [ProgramId])
);

-- Program trainers
CREATE TABLE [ProgramTrainers] (
    [ProgramId] INT NOT NULL FOREIGN KEY REFERENCES [Programs]([Id]) ON DELETE CASCADE,
    [TrainerId] INT NOT NULL FOREIGN KEY REFERENCES [Trainers]([Id]) ON DELETE CASCADE,
    PRIMARY KEY ([ProgramId], [TrainerId])
);

-- =============================================
-- INDEXES FOR PERFORMANCE
-- =============================================

CREATE INDEX IX_PageContents_PageId_LanguageId ON [PageContents]([PageId], [LanguageId]);
CREATE INDEX IX_PageContents_IsPublished ON [PageContents]([IsPublished]) INCLUDE ([PageId], [LanguageId]);
CREATE INDEX IX_Programs_Slug_IsPublished ON [Programs]([Slug], [IsPublished]);
CREATE INDEX IX_ContactSubmissions_SubmittedAt ON [ContactSubmissions]([SubmittedAt] DESC);
CREATE INDEX IX_ContactSubmissions_IsRead ON [ContactSubmissions]([IsRead]);

-- =============================================
-- SEED DATA
-- =============================================

-- Languages
INSERT INTO [Languages] ([Code], [Name], [IsDefault]) VALUES 
('en', 'English', 1),
('mk', 'Macedonian', 0);

-- Pages (slugs match React routes)
INSERT INTO [Pages] ([Slug], [SortOrder], [IsPublished], [IsMenuItem], [MenuSortOrder]) VALUES
('home', 10, 1, 1, 1),
('about', 20, 1, 1, 2),
('programs', 30, 1, 1, 3),
('corporate-programs', 35, 1, 1, 4),
('coaching', 40, 1, 1, 5),
('trainers', 50, 1, 1, 6),
('gallery', 60, 1, 1, 7),
('references', 70, 1, 1, 8),
('testimonials', 80, 1, 1, 9),
('downloads', 90, 1, 1, 10),
('contact', 100, 1, 1, 11);