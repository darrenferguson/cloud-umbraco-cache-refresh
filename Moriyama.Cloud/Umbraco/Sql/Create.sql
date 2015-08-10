Begin Transaction

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[MoriyamaHosts]') AND type in (N'U'))
	BEGIN
		CREATE TABLE MoriyamaHosts(
			[HostId] [nvarchar](50)  NOT NULL,
			[AccessTime] [datetime] NOT NULL,
			CONSTRAINT [PK_dbo.Hosts] PRIMARY KEY CLUSTERED 
			(
				[HostId] ASC
			))
	END;

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[MoriyamaPublishes]') AND type in (N'U'))
	BEGIN
		CREATE TABLE MoriyamaPublishes(
			[DocumentId] [int] NOT NULL,
			[PublishTime] [datetime] NOT NULL,
			[HostId] [nvarchar](50) NOT NULL,
			[PublishId] [UniqueIdentifier] NOT NULL,
			[RefreshType] [nvarchar](50) NOT NULL
		CONSTRAINT [PK_dbo.Publishes] PRIMARY KEY CLUSTERED 
		(
			HostId, DocumentId
		));

		
		ALTER TABLE MoriyamaPublishes
			ADD FOREIGN KEY (HostId)
		REFERENCES MoriyamaHosts(HostId) ON DELETE CASCADE;

	END;

IF NOT EXISTS ( SELECT 1 FROM information_schema.COLUMNS WHERE table_schema = 'dbo' AND TABLE_NAME = 'MoriyamaPublishes' AND column_Name = 'RefreshType' )
	BEGIN
		ALTER TABLE MoriyamaPublishes
		ADD [RefreshType] [nvarchar](50);	
		Exec('UPDATE MoriyamaPublishes Set [RefreshType] = '''' WHERE [RefreshType] IS NULL');
	END;
Commit Transaction


