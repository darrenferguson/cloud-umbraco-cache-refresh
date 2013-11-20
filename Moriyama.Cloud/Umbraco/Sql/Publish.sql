Begin Transaction

  Declare @HostId nvarchar(50)

  Select distinct HostId into #hosts from MoriyamaHosts where HostId <> @PublishingHost
  
  While (Select Count(*) From #hosts) > 0
	Begin
		Select Top 1 @HostId = HostId From #hosts

	IF (NOT EXISTS(SELECT * FROM MoriyamaPublishes WHERE HostId = @HostId And DocumentId = @DocumentId)) 
	Begin

    	Insert Into MoriyamaPublishes 
    		(DocumentId, PublishTime, HostId, PublishId)
    	Values
    		(@DocumentId, GETDATE(), @HostId, NewId())
	End
		
		Delete #hosts Where HostId = @HostId
    
	End

   Update MoriyamaHosts set AccessTime = GetDate() where HostId = @PublishingHost
	
   Drop Table #hosts
   
Commit Transaction