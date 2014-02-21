Begin Transaction
IF OBJECT_ID('tempdb..#hosts') IS NOT NULL
    DROP TABLE #hosts

CREATE TABLE #hosts ([HostId] nvarchar(50))

Declare @HostId nvarchar(50)
/*Declare @DocumentId int*/

--INSERT INTO #hosts (HostId) values (@HostId)
INSERT INTO #hosts (HostId)
  Select distinct HostId from MoriyamaHosts where HostId <> @PublishingHost
  
  While (Select Count(*) From #hosts) > 0
	Begin
		Select Top 1 @HostId = HostId From #hosts

		IF (NOT EXISTS(SELECT * FROM MoriyamaPublishes WHERE HostId = @HostId And DocumentId = @DocumentId)) 
			Begin

    		INSERT INTO MoriyamaPublishes 
    			(DocumentId, PublishTime, HostId, PublishId)
    		Values
    			(@DocumentId, GETDATE(), @HostId, NewId())
		End
		
		DELETE FROM #hosts Where HostId = @HostId
    
	End

   Update MoriyamaHosts set AccessTime = GetDate() where HostId = @PublishingHost
	
   Drop Table #hosts
   
Commit Transaction