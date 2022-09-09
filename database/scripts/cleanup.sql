-- A simple script to clean database from all created objects, if needed

drop table dbo.[$__dbup_journal]
drop table dbo.todos
drop procedure web.delete_todo
drop procedure web.patch_todo
drop procedure web.post_todo
drop procedure web.get_todo
drop schema web
drop sequence dbo.global_sequence
drop user webapp