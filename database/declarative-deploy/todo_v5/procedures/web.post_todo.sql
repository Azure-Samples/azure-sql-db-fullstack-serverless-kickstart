/*
	POST
	Accepted Input: 
	Payload: '[{"id":1, "title":"todo title", "completed": 0}, {"id":2, "title": "another todo"}]'
    Context: '{}' or '{"UserId": 123}
*/
create procedure [web].[post_todo]
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