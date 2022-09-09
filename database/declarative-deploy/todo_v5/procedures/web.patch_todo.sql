/*
	PATCH
	Accepted Input: 
	Payload: '[{"id":1, "todo":{"id": 10, "title": "updated title", "completed": 1 },{...}]'
    Context: '{}' or '{"UserId": 123}
*/
create procedure [web].[patch_todo]
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