Begin Transaction
	IF (NOT EXISTS(SELECT 1 FROM MoriyamaHosts WHERE HostId = @HostId)) 
	BEGIN 
		insert into MoriyamaHosts (HostId, AccessTime) values (@HostId, @AccessTime)
	END 
	ELSE 
	BEGIN 
	    update MoriyamaHosts set AccessTime = @AccessTime where HostId = @HostId
	END 
Commit Transaction
