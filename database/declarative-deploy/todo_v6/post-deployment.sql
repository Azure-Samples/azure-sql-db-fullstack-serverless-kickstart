-- This file contains SQL statements that will be executed after the build script.

insert into dbo.[todos] 
    (todo, owner_id) 
values 
    ('slides', 'anonymous'), 
    ('demos', 'anonymous')
go

if (user_id('webapp') is null) begin
    create user [webapp] with password = 'Super_Str0ng*P4ZZword!';
end

grant execute on schema::[web] to [webapp]
go

