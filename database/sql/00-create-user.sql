if (serverproperty('Edition') = 'SQL Azure') begin

    if not exists (select * from sys.database_principals where [type] in ('E', 'S') and [name] = 'webapp')
    begin 
        create user [webapp] with password = 'Super_Str0ng*P4ZZword!'        
    end

    alter role db_owner add member [webapp]
    
end else begin

    if not exists (select * from sys.server_principals where [type] in ('E', 'S') and [name] = 'webapp')
    begin 
        create login [webapp] with password = 'Super_Str0ng*P4ZZword!'
    end    

    if not exists (select * from sys.database_principals where [type] in ('E', 'S') and [name] = 'webapp')
    begin
       create user [webapp] from login [webapp]   
    end

    alter role db_owner add member [webapp]
end
go