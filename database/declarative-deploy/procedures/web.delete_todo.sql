/*
	DELETE
	Accepted Input: 
	Payload: '[{"id":1}, {"id":2}]'
    Context: '{}' or '{"UserId": 123}
*/
create procedure [web].[delete_todo]
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