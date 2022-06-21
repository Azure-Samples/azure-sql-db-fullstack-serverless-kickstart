-- This file contains SQL statements that will be executed before the build script.

if (object_id('dbo.todos') is not null) begin
    delete from dbo.todos;
end

if (user_id('webapp') is not null) begin
    drop user [webapp];
end
