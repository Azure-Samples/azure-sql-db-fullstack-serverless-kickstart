-- This file contains SQL statements that will be executed before the build script.

if (object_id('dbo.todoso') is not null) begin
    delete from dbo.todos
end
