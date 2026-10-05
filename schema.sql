IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Users] (
    [Id] bigint NOT NULL IDENTITY,
    [IpHash] binary(32) NOT NULL,
    [FingerprintHash] binary(32) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

CREATE TABLE [Comments] (
    [Id] bigint NOT NULL IDENTITY,
    [AuthorId] bigint NOT NULL,
    [ParentId] bigint NULL,
    [Email] nvarchar(254) NOT NULL,
    [Username] nvarchar(64) NOT NULL,
    [HomePage] nvarchar(2048) NULL,
    [Message] nvarchar(max) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_Comments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Comments_Comments_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Comments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Comments_Users_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Attachments] (
    [Id] uniqueidentifier NOT NULL,
    [CommentId] bigint NOT NULL,
    [Status] nvarchar(9) NOT NULL,
    [Filetype] nvarchar(255) NOT NULL,
    [Filepath] nvarchar(255) NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_Attachments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Attachments_Comments_CommentId] FOREIGN KEY ([CommentId]) REFERENCES [Comments] ([Id]) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX [IX_Attachments_CommentId] ON [Attachments] ([CommentId]);

CREATE INDEX [IX_Comments_AuthorId] ON [Comments] ([AuthorId]);

CREATE INDEX [IX_Comments_CreatedAt] ON [Comments] ([CreatedAt]);

CREATE INDEX [IX_Comments_Email] ON [Comments] ([Email]);

CREATE INDEX [IX_Comments_ParentId] ON [Comments] ([ParentId]);

CREATE INDEX [IX_Comments_Username] ON [Comments] ([Username]);

CREATE UNIQUE INDEX [IX_Users_IpHash_FingerprintHash] ON [Users] ([IpHash], [FingerprintHash]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261004202639_InitialMigration', N'10.0.12');

COMMIT;
GO

