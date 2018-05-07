CREATE TABLE [dbo].[Games] (
    [GameID]    INT          IDENTITY (1, 1) NOT NULL,
    [Player1]   VARCHAR (36) NOT NULL,
    [Player2]   VARCHAR (36) NULL,
    [Board]     NCHAR (16)   NULL,
    [TimeLimit] INT          NULL,
    [StartTime] DATETIME     NULL,
    [GameState] NCHAR(10) NOT NULL , 
    PRIMARY KEY CLUSTERED ([GameID] ASC),
    CONSTRAINT [FK_Games_Users] FOREIGN KEY ([Player1]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_Games_Users2] FOREIGN KEY ([Player2]) REFERENCES [dbo].[Users] ([UserID])
);

