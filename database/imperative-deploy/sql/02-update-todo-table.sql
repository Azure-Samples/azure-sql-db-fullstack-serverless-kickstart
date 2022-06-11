alter table dbo.todos
add [owner_id] int 
go

update dbo.todos set [owner_id] = 0 where [owner_id] is null;
go

alter table dbo.todos
alter column [owner_id] int not null
go
