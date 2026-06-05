/****** Object:  Table [dbo].[AppSettings]    Script Date: 5/6/2026 11:14:40 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AppSettings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](100) NOT NULL,
	[Value] [nvarchar](500) NOT NULL,
	[CreatedAtUtc] [datetime2](7) NOT NULL,
	[UpdatedAtUtc] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_AppSettings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GroupInvitations]    Script Date: 5/6/2026 11:14:40 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GroupInvitations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GroupId] [int] NOT NULL,
	[InvitedUserId] [int] NULL,
	[CreatedByUserId] [int] NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
	[CreatedAtUtc] [datetime2](7) NOT NULL,
	[RespondedAtUtc] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
	[UpdatedAtUtc] [datetime2](7) NOT NULL,
	[UpdatedByUserId] [int] NOT NULL,
	[AcceptedByUserId] [int] NULL,
	[DeclinedByUserId] [int] NULL,
	[ExpiresAtUtc] [datetime2](7) NOT NULL,
	[InvitedEmail] [nvarchar](256) NOT NULL,
	[InvitedEmailNormalized] [nvarchar](256) NOT NULL,
	[TokenHash] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_GroupInvitations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GroupMembers]    Script Date: 5/6/2026 11:14:40 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GroupMembers](
	[GroupId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[JoinedAtUtc] [datetime2](7) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedAtUtc] [datetime2](7) NOT NULL,
	[CreatedByUserId] [int] NOT NULL,
	[UpdatedAtUtc] [datetime2](7) NOT NULL,
	[UpdatedByUserId] [int] NOT NULL,
	[IsEnabled] [bit] NOT NULL,
 CONSTRAINT [PK_GroupMembers] PRIMARY KEY CLUSTERED 
(
	[GroupId] ASC,
	[UserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Groups]    Script Date: 5/6/2026 11:14:40 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Groups](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[OwnerUserId] [int] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedAtUtc] [datetime2](7) NOT NULL,
	[CreatedByUserId] [int] NOT NULL,
	[UpdatedAtUtc] [datetime2](7) NOT NULL,
	[UpdatedByUserId] [int] NOT NULL,
	[ImageContentType] [nvarchar](max) NULL,
	[ImageKey] [nvarchar](max) NULL,
	[ImageUpdatedAtUtc] [datetime2](7) NULL,
	[TimeZoneId] [nvarchar](100) NULL,
 CONSTRAINT [PK_Groups] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GroupScoringRules]    Script Date: 5/6/2026 11:14:40 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GroupScoringRules](
	[GroupId] [int] NOT NULL,
	[EnableOutcomeRule] [bit] NOT NULL,
	[OutcomePoints] [int] NULL,
	[EnableExactScoreRule] [bit] NOT NULL,
	[ExactHomeGoalsPoints] [int] NULL,
	[ExactAwayGoalsPoints] [int] NULL,
	[RequireBothExactScores] [bit] NOT NULL,
	[EnableGoalDifferenceRule] [bit] NOT NULL,
	[ClosedMatchPoints] [int] NULL,
	[ComfortableWinPoints] [int] NULL,
	[BlowoutPoints] [int] NULL,
	[CreatedAtUtc] [datetime2](7) NOT NULL,
	[CreatedByUserId] [int] NOT NULL,
	[UpdatedAtUtc] [datetime2](7) NOT NULL,
	[UpdatedByUserId] [int] NOT NULL,
 CONSTRAINT [PK_GroupScoringRules] PRIMARY KEY CLUSTERED 
(
	[GroupId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GroupUserJourneyStandingSnapshots]    Script Date: 5/6/2026 11:14:40 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GroupUserJourneyStandingSnapshots](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GroupId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[JourneyNumber] [int] NOT NULL,
	[JourneyDate] [date] NOT NULL,
	[PointsOfDay] [int] NOT NULL,
	[CumulativePoints] [int] NOT NULL,
	[PositionInJourney] [int] NOT NULL,
	[LastMatchId] [int] NULL,
	[UpdatedAtUtc] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_GroupUserJourneyStandingSnapshots] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GroupUserMatchScores]    Script Date: 5/6/2026 11:14:40 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GroupUserMatchScores](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GroupId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[MatchId] [int] NOT NULL,
	[Points] [int] NOT NULL,
	[OutcomePoints] [int] NOT NULL,
	[ExactHomeGoalsPoints] [int] NOT NULL,
	[ExactAwayGoalsPoints] [int] NOT NULL,
	[CategoryPoints] [int] NOT NULL,
	[PredictedHomeGoals] [int] NULL,
	[PredictedAwayGoals] [int] NULL,
	[OfficialHomeGoals] [int] NOT NULL,
	[OfficialAwayGoals] [int] NOT NULL,
	[CalculatedAtUtc] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Matches]    Script Date: 5/6/2026 11:14:40 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Matches](
	[Id] [int] NOT NULL,
	[GroupCode] [nvarchar](10) NOT NULL,
	[MatchNumber] [int] NOT NULL,
	[HomeTeam] [nvarchar](100) NOT NULL,
	[AwayTeam] [nvarchar](100) NOT NULL,
	[HomeFlag] [nvarchar](50) NOT NULL,
	[AwayFlag] [nvarchar](50) NOT NULL,
	[Stadium] [nvarchar](150) NOT NULL,
	[City] [nvarchar](100) NOT NULL,
	[MatchDateUtc] [datetime2](0) NOT NULL,
	[StageCode] [nvarchar](30) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsFinished] [bit] NOT NULL,
	[CreatedAtUtc] [datetime2](0) NOT NULL,
	[UpdatedAtUtc] [datetime2](0) NOT NULL,
	[HomeTeamGoals] [int] NULL,
	[AwayTeamGoals] [int] NULL,
 CONSTRAINT [PK_Matches] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notification]    Script Date: 5/6/2026 11:14:40 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notification](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[EventKey] [nvarchar](100) NOT NULL,
	[EntityType] [nvarchar](100) NULL,
	[EntityId] [nvarchar](100) NULL,
	[DeduplicationKey] [nvarchar](300) NOT NULL,
	[UserId] [int] NOT NULL,
	[Title] [nvarchar](250) NULL,
	[Body] [nvarchar](max) NULL,
	[DataJson] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NotificationDelivery]    Script Date: 5/6/2026 11:14:40 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NotificationDelivery](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[NotificationId] [bigint] NOT NULL,
	[Channel] [nvarchar](20) NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
	[RetryCount] [int] NOT NULL,
	[MaxRetries] [int] NOT NULL,
	[ScheduledAt] [datetime2](7) NULL,
	[LockedAt] [datetime2](7) NULL,
	[LockedBy] [nvarchar](100) NULL,
	[SentAt] [datetime2](7) NULL,
	[FailedAt] [datetime2](7) NULL,
	[LastError] [nvarchar](max) NULL,
	[ProviderResponse] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UserId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserDeviceToken]    Script Date: 5/6/2026 11:14:40 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserDeviceToken](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[Channel] [nvarchar](20) NOT NULL,
	[Platform] [nvarchar](20) NOT NULL,
	[Token] [nvarchar](1000) NOT NULL,
	[DeviceName] [nvarchar](200) NULL,
	[AppVersion] [nvarchar](50) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[LastSeenAt] [datetime2](7) NULL,
	[LastSentAt] [datetime2](7) NULL,
	[InvalidatedAt] [datetime2](7) NULL,
	[InvalidReason] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserMatchPredictions]    Script Date: 5/6/2026 11:14:40 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserMatchPredictions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[MatchId] [int] NOT NULL,
	[PredictedHomeGoals] [int] NULL,
	[PredictedAwayGoals] [int] NULL,
	[HasPrediction] [bit] NOT NULL,
	[CreatedAtUtc] [datetime2](0) NOT NULL,
	[UpdatedAtUtc] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_UserMatchPredictions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserMatchSimulations]    Script Date: 5/6/2026 11:14:40 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserMatchSimulations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[MatchId] [int] NOT NULL,
	[SimulatedHomeGoals] [int] NULL,
	[SimulatedAwayGoals] [int] NULL,
	[HasSimulation] [bit] NOT NULL,
	[CreatedAtUtc] [datetime2](7) NOT NULL,
	[UpdatedAtUtc] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 5/6/2026 11:14:40 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FirebaseUid] [nvarchar](128) NOT NULL,
	[Email] [nvarchar](256) NOT NULL,
	[Nickname] [nvarchar](256) NULL,
	[CreatedAtUtc] [datetime2](7) NOT NULL,
	[PhotoKey] [nvarchar](500) NULL,
	[PhotoContentType] [nvarchar](100) NULL,
	[PreferredLanguage] [varchar](10) NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AppSettings_Key]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppSettings_Key] ON [dbo].[AppSettings]
(
	[Key] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_GroupInvitations_AcceptedByUserId]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_GroupInvitations_AcceptedByUserId] ON [dbo].[GroupInvitations]
(
	[AcceptedByUserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_GroupInvitations_CreatedByUserId]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_GroupInvitations_CreatedByUserId] ON [dbo].[GroupInvitations]
(
	[CreatedByUserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_GroupInvitations_DeclinedByUserId]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_GroupInvitations_DeclinedByUserId] ON [dbo].[GroupInvitations]
(
	[DeclinedByUserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_GroupInvitations_GroupId]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_GroupInvitations_GroupId] ON [dbo].[GroupInvitations]
(
	[GroupId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_GroupInvitations_GroupId_InvitedEmailNormalized_IsDeleted]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_GroupInvitations_GroupId_InvitedEmailNormalized_IsDeleted] ON [dbo].[GroupInvitations]
(
	[GroupId] ASC,
	[InvitedEmailNormalized] ASC,
	[IsDeleted] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_GroupInvitations_InvitedEmailNormalized]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_GroupInvitations_InvitedEmailNormalized] ON [dbo].[GroupInvitations]
(
	[InvitedEmailNormalized] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_GroupInvitations_InvitedUserId]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_GroupInvitations_InvitedUserId] ON [dbo].[GroupInvitations]
(
	[InvitedUserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_GroupInvitations_TokenHash]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_GroupInvitations_TokenHash] ON [dbo].[GroupInvitations]
(
	[TokenHash] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_GroupInvitations_UpdatedByUserId]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_GroupInvitations_UpdatedByUserId] ON [dbo].[GroupInvitations]
(
	[UpdatedByUserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_GroupMembers_CreatedByUserId]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_GroupMembers_CreatedByUserId] ON [dbo].[GroupMembers]
(
	[CreatedByUserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_GroupMembers_GroupId_IsDeleted]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_GroupMembers_GroupId_IsDeleted] ON [dbo].[GroupMembers]
(
	[GroupId] ASC,
	[IsDeleted] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_GroupMembers_UpdatedByUserId]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_GroupMembers_UpdatedByUserId] ON [dbo].[GroupMembers]
(
	[UpdatedByUserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_GroupMembers_UserId]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_GroupMembers_UserId] ON [dbo].[GroupMembers]
(
	[UserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_GroupMembers_UserId_IsDeleted]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_GroupMembers_UserId_IsDeleted] ON [dbo].[GroupMembers]
(
	[UserId] ASC,
	[IsDeleted] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Groups_CreatedByUserId]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_Groups_CreatedByUserId] ON [dbo].[Groups]
(
	[CreatedByUserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Groups_IsDeleted_OwnerUserId]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_Groups_IsDeleted_OwnerUserId] ON [dbo].[Groups]
(
	[IsDeleted] ASC,
	[OwnerUserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Groups_OwnerUserId]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_Groups_OwnerUserId] ON [dbo].[Groups]
(
	[OwnerUserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Groups_UpdatedByUserId]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_Groups_UpdatedByUserId] ON [dbo].[Groups]
(
	[UpdatedByUserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_GroupScoringRules_CreatedByUserId]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_GroupScoringRules_CreatedByUserId] ON [dbo].[GroupScoringRules]
(
	[CreatedByUserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_GroupScoringRules_UpdatedByUserId]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_GroupScoringRules_UpdatedByUserId] ON [dbo].[GroupScoringRules]
(
	[UpdatedByUserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_GroupUserJourneyStandingSnapshots_Group_Journey]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_GroupUserJourneyStandingSnapshots_Group_Journey] ON [dbo].[GroupUserJourneyStandingSnapshots]
(
	[GroupId] ASC,
	[JourneyNumber] ASC,
	[PositionInJourney] ASC
)
INCLUDE([UserId],[PointsOfDay],[CumulativePoints],[JourneyDate],[LastMatchId],[UpdatedAtUtc]) WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_GroupUserJourneyStandingSnapshots_Group_User]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_GroupUserJourneyStandingSnapshots_Group_User] ON [dbo].[GroupUserJourneyStandingSnapshots]
(
	[GroupId] ASC,
	[UserId] ASC
)
INCLUDE([JourneyNumber],[JourneyDate],[PointsOfDay],[CumulativePoints],[PositionInJourney]) WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_GroupUserJourneyStandingSnapshots_Group_User_JourneyDate]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_GroupUserJourneyStandingSnapshots_Group_User_JourneyDate] ON [dbo].[GroupUserJourneyStandingSnapshots]
(
	[GroupId] ASC,
	[UserId] ASC,
	[JourneyDate] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_GroupUserMatchScores_Group_Match]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_GroupUserMatchScores_Group_Match] ON [dbo].[GroupUserMatchScores]
(
	[GroupId] ASC,
	[MatchId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_GroupUserMatchScores_Group_User]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_GroupUserMatchScores_Group_User] ON [dbo].[GroupUserMatchScores]
(
	[GroupId] ASC,
	[UserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_GroupUserMatchScores_Group_User_Match]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_GroupUserMatchScores_Group_User_Match] ON [dbo].[GroupUserMatchScores]
(
	[GroupId] ASC,
	[UserId] ASC,
	[MatchId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Notification_DeduplicationKey]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Notification_DeduplicationKey] ON [dbo].[Notification]
(
	[DeduplicationKey] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_NotificationDelivery_PendingQueue]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_NotificationDelivery_PendingQueue] ON [dbo].[NotificationDelivery]
(
	[Status] ASC,
	[ScheduledAt] ASC,
	[RetryCount] ASC,
	[CreatedAt] ASC,
	[Id] ASC
)
INCLUDE([NotificationId],[Channel],[UserId],[MaxRetries]) 
WHERE ([Status]='Pending')
WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_NotificationDelivery_Status_ScheduledAt]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_NotificationDelivery_Status_ScheduledAt] ON [dbo].[NotificationDelivery]
(
	[Status] ASC,
	[ScheduledAt] ASC,
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_UserDeviceToken_UserId_IsActive]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_UserDeviceToken_UserId_IsActive] ON [dbo].[UserDeviceToken]
(
	[UserId] ASC,
	[IsActive] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_UserDeviceToken_Token]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_UserDeviceToken_Token] ON [dbo].[UserDeviceToken]
(
	[Token] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_UserMatchSimulation]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_UserMatchSimulation] ON [dbo].[UserMatchSimulations]
(
	[UserId] ASC,
	[MatchId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Users_Email]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users]
(
	[Email] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Users_FirebaseUid]    Script Date: 5/6/2026 11:14:40 a. m. ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_FirebaseUid] ON [dbo].[Users]
(
	[FirebaseUid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[GroupInvitations] ADD  DEFAULT ('0001-01-01T00:00:00.0000000') FOR [ExpiresAtUtc]
GO
ALTER TABLE [dbo].[GroupInvitations] ADD  DEFAULT (N'') FOR [InvitedEmail]
GO
ALTER TABLE [dbo].[GroupInvitations] ADD  DEFAULT (N'') FOR [InvitedEmailNormalized]
GO
ALTER TABLE [dbo].[GroupInvitations] ADD  DEFAULT (N'') FOR [TokenHash]
GO
ALTER TABLE [dbo].[GroupMembers] ADD  CONSTRAINT [DF_GroupMembers_IsEnabled]  DEFAULT ((1)) FOR [IsEnabled]
GO
ALTER TABLE [dbo].[Groups] ADD  CONSTRAINT [DF_Groups_TimeZoneId]  DEFAULT ('America/New_York') FOR [TimeZoneId]
GO
ALTER TABLE [dbo].[Notification] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[NotificationDelivery] ADD  DEFAULT ('Pending') FOR [Status]
GO
ALTER TABLE [dbo].[NotificationDelivery] ADD  DEFAULT ((0)) FOR [RetryCount]
GO
ALTER TABLE [dbo].[NotificationDelivery] ADD  DEFAULT ((3)) FOR [MaxRetries]
GO
ALTER TABLE [dbo].[NotificationDelivery] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[UserDeviceToken] ADD  DEFAULT ('Push') FOR [Channel]
GO
ALTER TABLE [dbo].[UserDeviceToken] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[UserDeviceToken] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ('en') FOR [PreferredLanguage]
GO
ALTER TABLE [dbo].[GroupInvitations]  WITH CHECK ADD  CONSTRAINT [FK_GroupInvitations_Groups_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Groups] ([Id])
GO
ALTER TABLE [dbo].[GroupInvitations] CHECK CONSTRAINT [FK_GroupInvitations_Groups_GroupId]
GO
ALTER TABLE [dbo].[GroupInvitations]  WITH CHECK ADD  CONSTRAINT [FK_GroupInvitations_Users_AcceptedByUserId] FOREIGN KEY([AcceptedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[GroupInvitations] CHECK CONSTRAINT [FK_GroupInvitations_Users_AcceptedByUserId]
GO
ALTER TABLE [dbo].[GroupInvitations]  WITH CHECK ADD  CONSTRAINT [FK_GroupInvitations_Users_CreatedByUserId] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[GroupInvitations] CHECK CONSTRAINT [FK_GroupInvitations_Users_CreatedByUserId]
GO
ALTER TABLE [dbo].[GroupInvitations]  WITH CHECK ADD  CONSTRAINT [FK_GroupInvitations_Users_DeclinedByUserId] FOREIGN KEY([DeclinedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[GroupInvitations] CHECK CONSTRAINT [FK_GroupInvitations_Users_DeclinedByUserId]
GO
ALTER TABLE [dbo].[GroupInvitations]  WITH CHECK ADD  CONSTRAINT [FK_GroupInvitations_Users_InvitedUserId] FOREIGN KEY([InvitedUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[GroupInvitations] CHECK CONSTRAINT [FK_GroupInvitations_Users_InvitedUserId]
GO
ALTER TABLE [dbo].[GroupInvitations]  WITH CHECK ADD  CONSTRAINT [FK_GroupInvitations_Users_UpdatedByUserId] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[GroupInvitations] CHECK CONSTRAINT [FK_GroupInvitations_Users_UpdatedByUserId]
GO
ALTER TABLE [dbo].[GroupMembers]  WITH CHECK ADD  CONSTRAINT [FK_GroupMembers_Groups_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Groups] ([Id])
GO
ALTER TABLE [dbo].[GroupMembers] CHECK CONSTRAINT [FK_GroupMembers_Groups_GroupId]
GO
ALTER TABLE [dbo].[GroupMembers]  WITH CHECK ADD  CONSTRAINT [FK_GroupMembers_Users_CreatedByUserId] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[GroupMembers] CHECK CONSTRAINT [FK_GroupMembers_Users_CreatedByUserId]
GO
ALTER TABLE [dbo].[GroupMembers]  WITH CHECK ADD  CONSTRAINT [FK_GroupMembers_Users_UpdatedByUserId] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[GroupMembers] CHECK CONSTRAINT [FK_GroupMembers_Users_UpdatedByUserId]
GO
ALTER TABLE [dbo].[GroupMembers]  WITH CHECK ADD  CONSTRAINT [FK_GroupMembers_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[GroupMembers] CHECK CONSTRAINT [FK_GroupMembers_Users_UserId]
GO
ALTER TABLE [dbo].[Groups]  WITH CHECK ADD  CONSTRAINT [FK_Groups_Users_CreatedByUserId] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Groups] CHECK CONSTRAINT [FK_Groups_Users_CreatedByUserId]
GO
ALTER TABLE [dbo].[Groups]  WITH CHECK ADD  CONSTRAINT [FK_Groups_Users_OwnerUserId] FOREIGN KEY([OwnerUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Groups] CHECK CONSTRAINT [FK_Groups_Users_OwnerUserId]
GO
ALTER TABLE [dbo].[Groups]  WITH CHECK ADD  CONSTRAINT [FK_Groups_Users_UpdatedByUserId] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Groups] CHECK CONSTRAINT [FK_Groups_Users_UpdatedByUserId]
GO
ALTER TABLE [dbo].[GroupScoringRules]  WITH CHECK ADD  CONSTRAINT [FK_GroupScoringRules_Groups_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Groups] ([Id])
GO
ALTER TABLE [dbo].[GroupScoringRules] CHECK CONSTRAINT [FK_GroupScoringRules_Groups_GroupId]
GO
ALTER TABLE [dbo].[GroupScoringRules]  WITH CHECK ADD  CONSTRAINT [FK_GroupScoringRules_Users_CreatedByUserId] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[GroupScoringRules] CHECK CONSTRAINT [FK_GroupScoringRules_Users_CreatedByUserId]
GO
ALTER TABLE [dbo].[GroupScoringRules]  WITH CHECK ADD  CONSTRAINT [FK_GroupScoringRules_Users_UpdatedByUserId] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[GroupScoringRules] CHECK CONSTRAINT [FK_GroupScoringRules_Users_UpdatedByUserId]
GO
ALTER TABLE [dbo].[Notification]  WITH CHECK ADD  CONSTRAINT [FK_Notification_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Notification] CHECK CONSTRAINT [FK_Notification_Users]
GO
ALTER TABLE [dbo].[NotificationDelivery]  WITH CHECK ADD  CONSTRAINT [FK_NotificationDelivery_Notification] FOREIGN KEY([NotificationId])
REFERENCES [dbo].[Notification] ([Id])
GO
ALTER TABLE [dbo].[NotificationDelivery] CHECK CONSTRAINT [FK_NotificationDelivery_Notification]
GO
/****** Object:  StoredProcedure [dbo].[SP_NotificationDelivery_TakePendingBatch]    Script Date: 5/6/2026 11:14:40 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[SP_NotificationDelivery_TakePendingBatch]
(
    @WorkerId NVARCHAR(100),
    @BatchSize INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Picked TABLE
    (
        Id BIGINT NOT NULL
    );

    ;WITH Pending AS
    (
        SELECT TOP (@BatchSize)
            nd.Id
        FROM dbo.NotificationDelivery nd WITH (UPDLOCK, READPAST, ROWLOCK)
        WHERE
            nd.Status = 'Pending'
            AND nd.RetryCount < nd.MaxRetries
            AND
            (
                nd.ScheduledAt IS NULL
                OR nd.ScheduledAt <= SYSUTCDATETIME()
            )
        ORDER BY
            ISNULL(nd.ScheduledAt, nd.CreatedAt),
            nd.Id
    )
    UPDATE nd
        SET
            nd.Status = 'Processing',
            nd.LockedAt = SYSUTCDATETIME(),
            nd.LockedBy = @WorkerId
    OUTPUT inserted.Id INTO @Picked(Id)
    FROM dbo.NotificationDelivery nd
    INNER JOIN Pending p ON p.Id = nd.Id;

    SELECT Id
    FROM @Picked
    ORDER BY Id;
END
GO
