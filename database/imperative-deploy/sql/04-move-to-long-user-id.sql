alter table dbo.todos
drop column [owner_id]
go

alter table dbo.todos
add [owner_id] varchar(128) collate Latin1_General_BIN2
go

update dbo.todos set owner_id = 'anonymous' where owner_id is null
go

alter table dbo.todos
alter column [owner_id] varchar(128) not null
go
