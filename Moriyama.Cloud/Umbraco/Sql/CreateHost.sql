IF (NOT EXISTS(SELECT * FROM MoriyamaHosts WHERE HostId = @HostId)) 
BEGIN 
	insert into MoriyamaHosts (HostId, AccessTime) values (@HostId, @AccessTime)
END 
ELSE 
BEGIN 
    update MoriyamaHosts set AccessTime = @AccessTime where HostId = @HostId
END 