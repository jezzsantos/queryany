﻿USE [TestDatabase]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[testentities]') AND type in (N'U'))
DROP TABLE [dbo].[testentities]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[firstjoiningtestentities]') AND type in (N'U'))
DROP TABLE [dbo].[firstjoiningtestentities]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[secondjoiningtestentities]') AND type in (N'U'))
DROP TABLE [dbo].[secondjoiningtestentities]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[testentities](
	[Id] [nvarchar](100) NOT NULL,
	[AStringValue] [nvarchar](max) NULL,
	[ABooleanValue] [bit] NULL,
	[ADateTimeUtcValue] [datetime] NULL,
	[ADateTimeOffsetUtcValue] [datetimeoffset](7) NULL,
	[ADoubleValue] [float] NULL,
	[AGuidValue] [nvarchar](36) NULL,
	[AIntValue] [int] NULL,
	[ALongValue] [bigint] NULL,
	[ABinaryValue] [varbinary](max) NULL,
	[AComplexNonValueTypeValue] [nvarchar](max) NULL,
	[AComplexValueTypeValue] [nvarchar](max) NULL,
	[CreatedAtUtc] [datetime] NULL,
	[LastModifiedAtUtc] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[firstjoiningtestentities](
	[Id] [nvarchar](100) NOT NULL,
	[AStringValue] [nvarchar](1000) NULL,
	[AIntValue] [int] NULL,
	[CreatedAtUtc] [datetime] NULL,
	[LastModifiedAtUtc] [datetime] NULL
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[secondjoiningtestentities](
	[Id] [nvarchar](100) NOT NULL,
	[AStringValue] [nvarchar](1000) NULL,
	[AIntValue] [int] NULL,
	[ALongValue] [bigint] NULL,
	[CreatedAtUtc] [datetime] NULL,
	[LastModifiedAtUtc] [datetime] NULL
) ON [PRIMARY]
GO