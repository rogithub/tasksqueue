USE [mylocaldb]
GO

/****** Object:  Table [dbo].[T_FEED_QUEUE]    Script Date: 2/26/2019 7:06:20 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[T_FEED_QUEUE](
	[F_GUID] [uniqueidentifier] NOT NULL,
	[F_INSTANCE_ID] [uniqueidentifier] NULL,
	[F_MACHINE_NAME] [varchar](500) NULL,
	[F_DATE_CREATED] [datetime] NOT NULL,
	[F_DATE_STARTED] [datetime] NULL,
	[F_DATE_COMPLETED] [datetime] NULL,
	[F_STATUS] [int] NOT NULL,
	[F_METHOD_NAME] [varchar](500) NOT NULL,
	[F_RESULT] [xml] NULL,
	[F_TIMEOUT_MILLISECONDS] [int] NOT NULL,
	[F_FULLY_QUALIFIED_CLASS_NAME] [varchar](500) NOT NULL,
	[F_CONSTRUCTOR_PARAMETERS] [xml] NOT NULL,
	[F_METHOD_PARAMETERS] [xml] NOT NULL,
	[F_EXCEPTION] [varchar](max) NULL,
	[F_ASSEMBLY] [varbinary](max) NOT NULL,
	[F_METHOD_PARAM_TYPES] [varbinary](max) NOT NULL,
 CONSTRAINT [PK_T_FEED_QUEUE] PRIMARY KEY CLUSTERED 
(
	[F_GUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


