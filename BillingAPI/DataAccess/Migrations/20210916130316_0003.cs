using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccess.Migrations
{
    public partial class _0003 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"CREATE TABLE [dbo].[Billing_APP_Logs] (
								[Id] [bigint] IDENTITY(1,1) NOT NULL,
								[ApplicationName] varchar(50) NOT NULL,
								[Message] [nvarchar](max) NULL,
								[Level] [tinyint] NULL,
								[TimeStamp] [datetime2](7) NULL,
								[Exception] [nvarchar](max) NULL,
								[RequestId] [uniqueidentifier] NULL,
								[IPAddress] [varchar](50) NULL,
								[UserName] [nvarchar](100) NULL,
								[Request] [nvarchar](max) NULL,
								[Response] [nvarchar](max) NULL,
								[Action] [nvarchar](500) NULL,
								[RequestURL] [nvarchar](1000) NULL,
								[ExecutionTime] [time](7) NULL,
								[SessionId] [varchar](600) NULL,
								[BrowserName] [nvarchar](1000) NULL,
								[BrowserVersion] [nvarchar](600) NULL,
								[OperatingSystem] [nvarchar](600) NULL,
								[CreatedDate] [datetime2](7) NOT NULL,
								[CreatedDay] [tinyint] NOT NULL,
								[CreatedMonth] [tinyint] NOT NULL,
								[CreatedYear] [smallint] NOT NULL,
							 CONSTRAINT [PK_Billing_APP_Logs] PRIMARY KEY CLUSTERED 
							(
								[Id] ASC
							)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
							) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
							GO

							ALTER TABLE [dbo].Billing_APP_Logs ADD  CONSTRAINT [DF_Billing_APP_Logs_CreateDate]  DEFAULT (getdate()) FOR [CreatedDate]
							GO

							ALTER TABLE [dbo].Billing_APP_Logs ADD  CONSTRAINT [DF_Billing_APP_Logs_CreatedDay]  DEFAULT (datepart(day,getdate())) FOR [CreatedDay]
							GO

							ALTER TABLE [dbo].Billing_APP_Logs ADD  CONSTRAINT [DF_Billing_APP_Logs_CreatedMonth]  DEFAULT (datepart(month,getdate())) FOR [CreatedMonth]
							GO

							ALTER TABLE [dbo].Billing_APP_Logs ADD  CONSTRAINT [DF_Billing_APP_Logs_CreatedYear]  DEFAULT (datepart(year,getdate())) FOR [CreatedYear]
							GO");
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql("DROP TABLE [dbo].[Billing_APP_Logs]");
		}
    }
}
