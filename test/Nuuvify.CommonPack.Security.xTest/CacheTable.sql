﻿USE Audicon
GO

CREATE TABLE Audicon.CacheTest
(
    Id nvarchar(449) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
    Value varbinary(MAX) NOT NULL,
    ExpiresAtTime datetimeoffset NOT NULL,
    SlidingExpirationInSeconds bigint NULL,
    AbsoluteExpiration datetimeoffset NULL,
    CONSTRAINT Pk_CacheTest_Id PRIMARY KEY (Id)
)