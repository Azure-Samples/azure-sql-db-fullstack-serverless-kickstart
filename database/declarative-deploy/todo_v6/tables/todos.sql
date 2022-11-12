create table dbo.todos
(
	id int not null primary key default (next value for [global_sequence]),
	todo nvarchar(100) not null,
	completed tinyint not null default (0),
	[owner_id] varchar(128) collate Latin1_General_BIN2 not null
)
go