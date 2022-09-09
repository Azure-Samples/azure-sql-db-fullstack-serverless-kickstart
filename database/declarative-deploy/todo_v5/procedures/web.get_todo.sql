/*
	GET
	Accepted Input: 
	Payload: '' or '[{"id":1}, {"id":2}]'
    Context: '{}' or '{"UserId": 123}
*/
create procedure [web].[get_todo]
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
