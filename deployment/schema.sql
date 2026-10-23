CREATE TABLE [Admins] (
    [Id] int NOT NULL IDENTITY,
    [PasswordHash] nvarchar(max) NOT NULL,
    [MustChangePassword] bit NOT NULL,
    [SecurityStamp] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Admins] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Contracts] (
    [Id] int NOT NULL IDENTITY,
    [Number] nvarchar(100) NOT NULL,
    [Title] nvarchar(250) NOT NULL,
    [Party] nvarchar(250) NOT NULL,
    [Amount] decimal(18,0) NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [Notes] nvarchar(4000) NULL,
    CONSTRAINT [PK_Contracts] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Attachments] (
    [Id] int NOT NULL IDENTITY,
    [ContractId] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Content] varbinary(max) NOT NULL,
    CONSTRAINT [PK_Attachments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Attachments_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_Attachments_ContractId] ON [Attachments] ([ContractId]);
GO


CREATE UNIQUE INDEX [IX_Contracts_Number] ON [Contracts] ([Number]);
GO


