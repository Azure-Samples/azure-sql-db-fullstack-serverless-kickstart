/*
	GET
	Accepted Input: 
	Payload: '' or '[{"id":1}, {"id":2}]'
    Context: '{}' or '{"UserId": 123}
*/
create or alter procedure [web].[get_todo]
@payload nvarchar(max) = null,
@context nvarchar(max)
as

if (isjson(@context) <> 1) begin;
	throw 50000, 'Context is not a valid JSON document', 16;
end;

declare @ownerId varchar(128) = cast(json_value(@context, '$.UserId') as varchar(128))

-- return all
if (@payload = '' or @payload is null) begin
select 
	cast(
		(select
			id,
			todo as title,
			completed as completed
		from
			dbo.todos t
        where
            owner_id = isnull(@ownerId, 'anonymous')
		for json path)
	as nvarchar(max)) as result;
	return ;
end

-- return the specified todos
if (isjson(@payload) <> 1) begin;
	throw 50000, 'Payload is not a valid JSON document', 16;
end;

select 
	cast(
		(select
			id,
			todo as title,
			completed as completed
		from
			dbo.todos t
		where        
            owner_id = isnull(@ownerId, 'anonymous')
        and
			exists (select p.id from openjson(@payload) with (id int) as p where p.id = t.id)
		for json path)
	as nvarchar(max)) as result
go

/*
	POST
	Accepted Input: 
	Payload: '[{"id":1, "title":"todo title", "completed": 0}, {"id":2, "title": "another todo"}]'
    Context: '{}' or '{"UserId": 123}
*/
create or alter procedure [web].[post_todo]
@payload nvarchar(max),
@context nvarchar(max)
as
if (isjson(@context) <> 1) begin;
	throw 50000, 'Context is not a valid JSON document', 16;
end;

if (isjson(@payload) <> 1) begin;
	throw 50000, 'Payload is not a valid JSON document', 16;
end;

declare @ownerId varchar(128) = cast(json_value(@context, '$.UserId') as varchar(128))

declare @ids table (id int not null);

insert into dbo.todos ([todo], [completed], [owner_id])
output inserted.id into @ids
select [title], isnull([completed],0), isnull(@ownerId, 'anonymous') from openjson(@payload) with
(
	title nvarchar(100),
	completed bit    
)

declare @newPayload as nvarchar(max) = (select id from @ids for json auto);
exec [web].[get_todo] @newPayload, @context
go


/*
	DELETE
	Accepted Input: 
	Payload: '[{"id":1}, {"id":2}]'
    Context: '{}' or '{"UserId": 123}
*/
create or alter procedure [web].[delete_todo]
@payload nvarchar(max),
@context nvarchar(max)
as
if (isjson(@context) <> 1) begin;
	throw 50000, 'Context is not a valid JSON document', 16;
end;

if (isjson(@payload) <> 1) begin;
	throw 50000, 'Payload is not a valid JSON document', 16;
end;

declare @ownerId varchar(128) = cast(json_value(@context, '$.UserId') as varchar(128))

declare @ids table (id int not null);

delete t 
output deleted.id into @ids
from dbo.todos t 
where 
    owner_id = isnull(@ownerId, 'anonymous') 
and
    exists (select p.id from openjson(@payload) with (id int) as p where p.id = t.id)

select id from @ids for json auto;
go

/*
	PATCH
	Accepted Input: 
	Payload: '[{"id":1, "todo":{"id": 10, "title": "updated title", "completed": 1 },{...}]'
    Context: '{}' or '{"UserId": 123}
*/
create or alter procedure [web].[patch_todo]
@payload nvarchar(max),
@context nvarchar(max)
as
if (isjson(@context) <> 1) begin;
	throw 50000, 'Context is not a valid JSON document', 16;
end;

if (isjson(@payload) <> 1) begin;
	throw 50000, 'Payload is not a valid JSON document', 16;
end;

declare @ownerId varchar(128) = cast(json_value(@context, '$.UserId') as varchar(128))

declare @ids table (id int not null);

with cte as
(
	select 
		id,
		new_id,
		title,
		completed
	from 
		openjson(@payload) with
		(
			id int '$.id',
			todo nvarchar(max) as json
		) 
		cross apply openjson(todo) with 
		(
			new_id int '$.id',
			title nvarchar(100),
			completed bit
		)
)
update
	t
set
	id = coalesce(c.new_id, t.id),
	todo = coalesce(c.title, t.todo),
	completed = coalesce(c.completed, t.completed)
output 
	inserted.id into @ids
from
	dbo.[todos] t
inner join
	cte c on t.id = c.id
where
    t.owner_id = isnull(@ownerId, 'anonymous') 
;

declare @newPayload as nvarchar(max) = (select id from @ids for json auto);
if (@newPayload is null) return;
exec [web].[get_todo] @newPayload, @context
go
