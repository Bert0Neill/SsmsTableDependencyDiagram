Select
S.[name] as 'Dependent_Tables', S.[type]
From
sys.objects S inner join sys.sysreferences R
on S.object_id = R.rkeyid
Where
S.[type] = 'U' AND
R.fkeyid = OBJECT_ID('[SalesLT].[CustomerAddress]')

-- tables that DEPEND on Table
declare @tableName varchar(64);
set @tableName = 'Customer';
select
SO_P.name as [parent table]
,SC_P.name as [parent column]
,'is a foreign key of' as [direction]
,SO_R.name as [referenced table]
,SC_R.name as [referenced column]
,*
from sys.foreign_key_columns FKC
inner join sys.objects SO_P on SO_P.object_id = FKC.parent_object_id
inner join sys.columns SC_P on (SC_P.object_id = FKC.parent_object_id) AND (SC_P.column_id = FKC.parent_column_id)
inner join sys.objects SO_R on SO_R.object_id = FKC.referenced_object_id
inner join sys.columns SC_R on (SC_R.object_id = FKC.referenced_object_id) AND (SC_R.column_id = FKC.referenced_column_id)
where
    ((SO_P.name = @tableName) AND (SO_P.type = 'U'))
    OR
    ((SO_R.name = @tableName) AND (SO_R.type = 'U'))


-- Tables that your table Depends on
use [AdventureWorksLT2016]
Select
S.[name] as 'Dependent_Tables', S.[type]
From
sys.objects S inner join sys.sysreferences R
on S.object_id = R.rkeyid
Where
S.[type] = 'U' AND
R.fkeyid = OBJECT_ID('Customer')

--------------------------------------------------------------------
use [AdventureWorksLT2016]
SELECT
s.name AS '@Schema',
t.name AS '@Name',
t.object_id AS '@ColumnId',
(
SELECT c.name AS '@Name',
    c.column_id AS '@ColumnId',
    IIF(i.object_id IS NOT NULL,1,0) AS '@IsPrimaryKey',
    f.referenced_object_id AS '@ReferencesTableId',
    f.referenced_column_id AS '@ReferencesColumnId'
FROM sys.columns AS c
LEFT OUTER JOIN sys.index_columns AS i ON c.object_id = i.object_id
  AND c.column_id = i.column_id
  AND i.index_id = 1
LEFT OUTER JOIN sys.foreign_key_columns AS f ON c.object_id = f.parent_object_id
  AND c.column_id = f.parent_column_id
WHERE c.object_id = t.object_id
FOR XML PATH ('Column'),TYPE
)
FROM sys.schemAS AS s
  INNER JOIN sys.tables AS t ON s.schema_id = t.schema_id
FOR XML PATH('Table'),ROOT('Tables')
--------------------------------------------------------------------
SELECT  TABLE_NAME      as name,
        TABLE_SCHEMA    as [schema],
        (
            SELECT Column_Name as Name,
                    DATA_TYPE as DataType,
                    CHARACTER_MAXIMUM_LENGTH as [Length]
            FROM INFORMATION_SCHEMA.COLUMNS
            For XML PATH ('Column'),root('columns'), type
        )
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA='dbo'
ORDER BY TABLE_NAME ASC
For XML PATH ('Table'),Root('Tables')
--------------------------------------------------------------------
use [AdventureWorksLT2016]
SELECT * FROM sys.sql_expression_dependencies  
WHERE referenced_id = OBJECT_ID(N'[SalesLT].[ProductModel]');   
--------------------------------------------------------------------

CREATE TABLE #tempdep (objid int NOT NULL, objname sysname NOT NULL, objschema sysname NULL, objdb sysname NOT NULL, objtype smallint NOT NULL)

BEGIN TRANSACTION

exec sp_executesql N'INSERT INTO #tempdep 
SELECT
tbl.object_id AS [ID],
tbl.name AS [Name],
SCHEMA_NAME(tbl.schema_id) AS [Schema],
db_name(),
3
FROM
sys.tables AS tbl
WHERE
(tbl.name=@_msparam_0 and SCHEMA_NAME(tbl.schema_id)=@_msparam_1)',N'@_msparam_0 nvarchar(4000),@_msparam_1 nvarchar(4000)',
@_msparam_0=N'ProductCategory',@_msparam_1=N'SalesLT'

COMMIT TRANSACTION

declare @find_referencing_objects int
set @find_referencing_objects = 0
-- parameters:
-- 1. create table #tempdep (objid int NOT NULL, objtype smallint NOT NULL)
--    contains source objects
-- 2. @find_referencing_objects defines ordering
--    1 order for drop
--    0 order for script

declare @must_set_nocount_off bit
set @must_set_nocount_off = 0

IF @@OPTIONS & 512 = 0 
   set @must_set_nocount_off = 1
set nocount on

declare @u int
declare @udf int
declare @v int
declare @sp int
declare @def int
declare @rule int
declare @tr int
declare @uda int
declare @uddt int
declare @xml int
declare @udt int
declare @assm int
declare @part_sch int
declare @part_func int
declare @synonym int
declare @sequence int
declare @udtt int
declare @ddltr int
declare @unknown int
declare @pg int

set @u = 3
set @udf = 0
set @v = 2
set @sp = 4
set @def = 6
set @rule = 7
set @tr = 8
set @uda = 11
set @synonym = 12
set @sequence = 13
--above 100 -> not in sys.objects
set @uddt = 101
set @xml = 102
set @udt = 103
set @assm = 1000
set @part_sch = 201
set @part_func = 202
set @udtt = 104
set @ddltr = 203
set @unknown = 1001
set @pg = 204

-- variables for referenced type obtained from sys.sql_expression_dependencies
declare @obj int
set @obj = 20
declare @type int
set @type = 21
-- variables for xml and part_func are already there

create table #t1
(
    object_id int NULL,
    object_name sysname collate database_default NULL,
    object_schema sysname collate database_default NULL,
    object_db sysname NULL,
    object_svr sysname NULL,
    object_type smallint NOT NULL,
    relative_id int NOT NULL,
    relative_name sysname collate database_default NOT NULL,
    relative_schema sysname collate database_default NULL,
    relative_db sysname NULL,
    relative_svr sysname NULL,
    relative_type smallint NOT NULL,
    schema_bound bit NOT NULL,
    rank smallint NULL,
    degree int NULL
)

-- we need to create another temporary table to store the dependencies from sys.sql_expression_dependencies till the updated values are inserted finally into #t1
create table #t2
(
    object_id int NULL,
    object_name sysname collate database_default NULL,
    object_schema sysname collate database_default NULL,
    object_db sysname NULL,
    object_svr sysname NULL,
    object_type smallint NOT NULL,
    relative_id int NOT NULL,
    relative_name sysname collate database_default NOT NULL,
    relative_schema sysname collate database_default NULL,
    relative_db sysname NULL,
    relative_svr sysname NULL,
    relative_type smallint NOT NULL,
    schema_bound bit NOT NULL,
    rank smallint NULL
)

-- This index will ensure that we have unique parent-child relationship
create unique clustered index i1 on #t1(object_name, object_schema, object_db, object_svr, object_type, relative_name, relative_schema, relative_type) with IGNORE_DUP_KEY

declare @iter_no int
set @iter_no = 1

declare @rows int
set @rows = 1

insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank) 
   select l.objid, l.objname, l.objschema, l.objdb, l.objtype, l.objid, l.objname, l.objschema, l.objdb, l.objtype, 1, @iter_no from #tempdep l

-- change the object_id of table types to their user_defined_id
update #t1 set object_id = tt.user_type_id, relative_id = tt.user_type_id
from sys.table_types as tt where tt.type_table_object_id = #t1.object_id and object_type = @udtt

while @rows > 0
begin
    set @rows = 0
    if (1 = @find_referencing_objects)
    begin
        -- HARD DEPENDENCIES
        -- these dependencies have to be in the same database only

        -- tables that reference uddts or udts
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select tbl.object_id, tbl.name, SCHEMA_NAME(tbl.schema_id), t.object_db, @u, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.columns as c on c.user_type_id = t.object_id
            join sys.tables as tbl on tbl.object_id = c.object_id
            where @iter_no = t.rank and (t.object_type = @uddt OR t.object_type = @udt) and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- udtts that reference uddts or udts
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select tt.user_type_id, tt.name, SCHEMA_NAME(tt.schema_id), t.object_db, @udtt, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.columns as c on c.user_type_id = t.object_id
            join sys.table_types as tt on tt.type_table_object_id = c.object_id
            where @iter_no = t.rank and (t.object_type = @uddt OR t.object_type = @udt) and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- tables/views that reference triggers
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select o.object_id, o.name, SCHEMA_NAME(o.schema_id), t.object_db, @tr, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.objects as o on o.parent_object_id = t.object_id and o.type = 'TR'
            where @iter_no = t.rank and (t.object_type = @u OR  t.object_type = @v) and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- tables that reference defaults (only default objects)
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select o.object_id, o.name, SCHEMA_NAME(o.schema_id), t.object_db, @u, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.columns as clmns on clmns.default_object_id = t.object_id
            join sys.objects as o on o.object_id = clmns.object_id and 0 = isnull(o.parent_object_id, 0)
            where @iter_no = t.rank and t.object_type = @def and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- types that reference defaults (only default objects)
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select tp.user_type_id, tp.name, SCHEMA_NAME(tp.schema_id), t.object_db, @uddt, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.types as tp on tp.default_object_id = t.object_id
            join sys.objects as o on o.object_id = t.object_id and 0 = isnull(o.parent_object_id, 0)
            where @iter_no = t.rank and t.object_type = @def and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- tables that reference rules
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select tbl.object_id, tbl.name, SCHEMA_NAME(tbl.schema_id), t.object_db, @u, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.columns as clmns on clmns.rule_object_id = t.object_id
            join sys.tables as tbl on tbl.object_id = clmns.object_id
            where @iter_no = t.rank and t.relative_type = @rule and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- types that reference rules
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select tp.user_type_id, tp.name, SCHEMA_NAME(tp.schema_id), t.object_db, @uddt, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.types as tp on tp.rule_object_id = t.object_id
            where @iter_no = t.rank and t.object_type = @rule and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- tables that reference XmlSchemaCollections
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select tbl.object_id, tbl.name, SCHEMA_NAME(tbl.schema_id), t.object_db, @u, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.columns as c on c.xml_collection_id = t.object_id
            join sys.tables as tbl on tbl.object_id = c.object_id -- eliminate views
            where @iter_no = t.rank and t.object_type = @xml and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- table types that reference XmlSchemaCollections
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select tt.user_type_id, tt.name, SCHEMA_NAME(tt.schema_id), t.object_db, @udtt, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.columns as c on c.xml_collection_id = t.object_id
            join sys.table_types as tt on tt.type_table_object_id = c.object_id
            where @iter_no = t.rank and t.object_type = @xml and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- procedures that reference XmlSchemaCollections
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select o.object_id, o.name, SCHEMA_NAME(o.schema_id), t.object_db, (case when o.type in ( 'P', 'RF', 'PC') then @sp else @udf end), t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.parameters as c on c.xml_collection_id = t.object_id
            join sys.objects as o on o.object_id = c.object_id
            where @iter_no = t.rank and t.object_type = @xml and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount
        -- udf, sp, uda, trigger all that reference assembly
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select o.object_id, o.name, SCHEMA_NAME(o.schema_id), t.object_db, (case o.type when 'AF' then @uda when 'PC' then @sp when 'FS' then @udf when 'FT' then @udf when 'TA' then @tr else @udf end), t.object_id, t.object_name, t.object_schema, t.object_db, 
t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.assembly_modules as am on ((am.assembly_id = t.object_id) and (am.assembly_id >= 65536))
            join sys.objects as o on am.object_id = o.object_id
            where @iter_no = t.rank and t.object_type = @assm and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount
        -- udt that reference assembly
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select at.user_type_id, at.name, SCHEMA_NAME(at.schema_id), t.object_db, @udt, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.assembly_types as at on ((at.assembly_id = t.object_id) and (at.is_user_defined = 1))
            where @iter_no = t.rank and t.object_type = @assm and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- assembly that reference assembly
        insert #t1 (object_id, object_name, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select asm.assembly_id, asm.name, t.object_db, @assm, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.assembly_references as ar on ((ar.referenced_assembly_id = t.object_id) and (ar.referenced_assembly_id >= 65536))
            join sys.assemblies as asm on asm.assembly_id = ar.assembly_id
            where @iter_no = t.rank and t.object_type = @assm and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- table references table
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select tbl.object_id, tbl.name, SCHEMA_NAME(tbl.schema_id), t.object_db, @u, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.foreign_keys as fk on fk.referenced_object_id = t.object_id
            join sys.tables as tbl on tbl.object_id = fk.parent_object_id
            where @iter_no = t.rank and t.object_type = @u and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- uda references types
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select o.object_id, o.name, SCHEMA_NAME(o.schema_id), t.object_db, @uda, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.parameters as p on p.user_type_id = t.object_id
            join sys.objects as o on o.object_id = p.object_id and o.type = 'AF'
            where @iter_no = t.rank and t.object_type in (@udt, @uddt, @udtt) and (t.object_svr IS null and t.object_db = db_name())

        -- table,view references partition scheme
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select o.object_id, o.name, SCHEMA_NAME(o.schema_id), t.object_db, (case o.type when 'V' then @v else @u end), t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.indexes as idx on idx.data_space_id = t.object_id
            join sys.objects as o on o.object_id = idx.object_id
            where @iter_no = t.rank and t.object_type = @part_sch and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- partition scheme references partition function
        insert #t1 (object_id, object_name, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select ps.data_space_id, ps.name, t.object_db, @part_sch, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.partition_schemes as ps on ps.function_id = t.object_id
            where @iter_no = t.rank and t.object_type = @part_func and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount
        
        -- plan guide references sp, udf, triggers
        insert #t1 (object_id, object_name, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select pg.plan_guide_id, pg.name, t.object_db, @pg, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.plan_guides as pg on pg.scope_object_id = t.object_id
            where @iter_no = t.rank and t.object_type in (@sp, @udf, @tr) and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- synonym refrences object
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select s.object_id, s.name, SCHEMA_NAME(s.schema_id), t.object_db, @synonym, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 0, @iter_no + 1
            from #t1 as t
            join sys.synonyms as s on object_id(s.base_object_name) = t.object_id
            where @iter_no = t.rank and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount						
        
        --  sequences that reference uddts 
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select s.object_id, s.name, SCHEMA_NAME(s.schema_id), t.object_db, @sequence, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 0, @iter_no + 1
            from #t1 as t
            join sys.sequences as s on s.user_type_id = t.object_id
            where @iter_no = t.rank and (t.object_type = @uddt) and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount	
        

        -- SOFT DEPENDENCIES
        DECLARE name_cursor CURSOR
        FOR
            SELECT DISTINCT t.object_id, t.object_name, t.object_schema, t.object_type
            FROM #t1 as t
            WHERE @iter_no = t.rank and (t.object_svr IS null and t.object_db = db_name()) and t.object_type NOT IN (@part_sch, @assm, @tr, @ddltr)
        OPEN name_cursor
        DECLARE @objid int
        DECLARE @objname sysname
        DECLARE @objschema sysname
        DECLARE @objtype smallint
        DECLARE @fullname sysname
        DECLARE @objecttype sysname
        FETCH NEXT FROM name_cursor INTO @objid, @objname, @objschema, @objtype
        WHILE (@@FETCH_STATUS <> -1)
        BEGIN
            SET @fullname = case when @objschema IS NULL then quotename(@objname)
                            else quotename(@objschema) + '.' + quotename(@objname) end
            SET @objecttype = case when @objtype in (@uddt, @udt, @udtt) then 'TYPE'
                                when @objtype = @xml then 'XML_SCHEMA_COLLECTION'
                                when @objtype = @part_func then 'PARTITION_FUNCTION'
                                else 'OBJECT' end
            insert #t2 (object_type, object_id, object_name, object_schema, object_db, object_svr, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
                select
                    case dep.referencing_class when 1 then (select
                        case when obj.type = 'U' then @u
                        when obj.type = 'V' then @v
                        when obj.type = 'TR' then @tr
                        when obj.type in ('P', 'RF', 'PC') then @sp
                        when obj.type in ('AF') then @uda
                        when obj.type in ('TF', 'FN', 'IF', 'FS', 'FT') then @udf
                        when obj.type = 'D' then @def
                        when obj.type = 'SN' then @synonym
                        when obj.type = 'SO' then @sequence
                        else @obj
                        end
                    from sys.objects as obj where obj.object_id = dep.referencing_id)
                when 6 then (select 
                        case when (tp.is_assembly_type = 1) then @udt
                        when (tp.is_table_type = 1) then @udtt
                        else @uddt
                        end
                    from sys.types as tp where tp.user_type_id = dep.referencing_id)
                when 7 then @u
                when 9 then @u	
                when 10 then @xml 
                when 12 then @ddltr 
                when 21 then @part_func 
                end,
            dep.referencing_id,
            dep.referencing_entity_name,
            dep.referencing_schema_name,
            db_name(), null,
            @objid, @objname,
            @objschema, db_name(), @objtype, 
            0, @iter_no + 1
            from sys.dm_sql_referencing_entities(@fullname, @objecttype) dep

            FETCH NEXT FROM name_cursor INTO @objid, @objname, @objschema, @objtype
        END
        CLOSE name_cursor
        DEALLOCATE name_cursor

        update #t2 set object_id = obj.object_id, object_name = obj.name, object_schema = schema_name(obj.schema_id), object_type = case when obj.type = 'U' then @u when obj.type = 'V' then @v end		
        from sys.objects as o
        join sys.objects as obj on obj.object_id = o.parent_object_id
        where o.object_id = #t2.object_id and (#t2.object_type = @obj OR o.parent_object_id != 0) and #t2.rank = @iter_no + 1

        insert #t1 (object_id, object_name, object_schema, object_db, object_svr, object_type, relative_id, relative_name, relative_schema, relative_db, relative_svr, relative_type, schema_bound, rank)
            select object_id, object_name, object_schema, object_db, object_svr, object_type, relative_id, relative_name, relative_schema, relative_db, relative_svr, relative_type, schema_bound, rank 
            from #t2 where @iter_no + 1 = rank and #t2.object_id != #t2.relative_id
        set @rows = @rows + @@rowcount

    end
    else
    begin
        -- SOFT DEPENDENCIES
        -- insert all values from sys.sql_expression_dependencies for the corresponding object
        -- first insert them in #t2, update them and then finally insert them in #t1
        insert #t2 (object_type, object_name, object_schema, object_db, object_svr, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select 
                case dep.referenced_class when 1 then @obj
                when 6 then @type
                when 7 then @u
                when 9 then @u	
                when 10 then @xml
                when 21 then @part_func
                end,
            dep.referenced_entity_name,
            dep.referenced_schema_name,
            dep.referenced_database_name,
            dep.referenced_server_name,
            t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type,
            dep.is_schema_bound_reference, @iter_no + 1
            from #t1 as t
            join sys.sql_expression_dependencies as dep on dep.referencing_id = t.object_id
            where @iter_no = t.rank and t.object_svr IS NULL and t.object_db = db_name()

        -- insert all the dependency values in case of a table that references a check
        insert #t2 (object_type, object_name, object_schema, object_db, object_svr, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select 
                case dep.referenced_class when 1 then @obj
                when 6 then @type
                when 7 then @u
                when 9 then @u	
                when 10 then @xml
                when 21 then @part_func
                end,
            dep.referenced_entity_name,
            dep.referenced_schema_name,
            dep.referenced_database_name,
            dep.referenced_server_name,
            t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type,
            dep.is_schema_bound_reference, @iter_no + 1
            from #t1 as t
            join sys.sql_expression_dependencies as d on d.referenced_id = t.object_id
            join sys.objects as o on o.object_id = d.referencing_id and o.type = 'C'
            join sys.sql_expression_dependencies as dep on dep.referencing_id = d.referencing_id and dep.referenced_id != t.object_id
            where @iter_no = t.rank and t.object_svr IS NULL and t.object_db = db_name() and t.object_type = @u

        -- insert all the dependency values in case of an object that belongs to another object whose dependencies are being found
        insert #t2 (object_type, object_name, object_schema, object_db, object_svr, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select
                case dep.referenced_class when 1 then @obj
                when 6 then @type
                when 7 then @u
                when 9 then @u	
                when 10 then @xml
                when 21 then @part_func
                end,
            dep.referenced_entity_name,
            dep.referenced_schema_name,
            dep.referenced_database_name,
            dep.referenced_server_name,
            t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type,
            dep.is_schema_bound_reference, @iter_no + 1
            from #t1 as t
            join sys.objects as o on o.parent_object_id = t.object_id
            join sys.sql_expression_dependencies as dep on dep.referencing_id = o.object_id
            where @iter_no = t.rank and t.object_svr IS NULL and t.object_db = db_name()

        -- queries for objects with object_id null and object_svr null - resolve them
        -- we will build the query to resolve the objects 
        -- increase @rows as we bind the objects
        
        DECLARE db_cursor CURSOR
        FOR
            select distinct ISNULL(object_db, db_name()) from #t2 as t
            where t.rank = (@iter_no+1) and t.object_id IS NULL and t.object_svr IS NULL
        OPEN db_cursor
        DECLARE @dbname sysname
        DECLARE @quote_quoted_dbname sysname
        DECLARE @bracket_quoted_dbname sysname
        FETCH NEXT FROM db_cursor INTO @dbname
        WHILE (@@FETCH_STATUS <> -1)
        BEGIN
            IF (db_id(@dbname) IS NULL) 
            BEGIN
                FETCH NEXT FROM db_cursor INTO @dbname
                CONTINUE
            END
            SET @quote_quoted_dbname = quotename(@dbname, '''')
            SET @bracket_quoted_dbname = quotename(@dbname, ']')
            DECLARE @query nvarchar(MAX)
            -- when schema is not null 
            -- @obj
            SET @query = 'update #t2 set object_db = N' + @quote_quoted_dbname + ', object_id = obj.object_id, object_type = 
                            case when obj.type = ''U'' then ' + CAST(@u AS nvarchar(8)) +
                            ' when obj.type = ''V'' then ' + CAST(@v AS nvarchar(8)) +
                            ' when obj.type = ''TR'' then ' + CAST(@tr AS nvarchar(8)) +
                            ' when obj.type in ( ''P'', ''RF'', ''PC'' ) then ' + CAST(@sp AS nvarchar(8)) +
                            ' when obj.type in ( ''AF'' ) then ' + CAST(@uda AS nvarchar(8)) +
                            ' when obj.type in ( ''TF'', ''FN'', ''IF'', ''FS'', ''FT'' ) then ' + CAST(@udf AS nvarchar(8)) +
                            ' when obj.type = ''D'' then ' + CAST(@def AS nvarchar(8)) +
                            ' when obj.type = ''SN'' then ' + CAST(@synonym AS nvarchar(8)) +
                            ' when obj.type = ''SO'' then ' + CAST(@sequence AS nvarchar(8)) +
                            ' else ' + CAST(@unknown AS nvarchar(8)) +
                            ' end
                from ' + @bracket_quoted_dbname + '.sys.objects as obj 
                join ' + @bracket_quoted_dbname + '.sys.schemas as sch on sch.schema_id = obj.schema_id
                where obj.name = #t2.object_name collate database_default
                and sch.name = #t2.object_schema collate database_default
                and #t2.object_type = ' + CAST(@obj AS nvarchar(8)) + ' and #t2.object_schema IS NOT NULL 
                and (#t2.object_db IS NULL or #t2.object_db = N' + @quote_quoted_dbname + ')
                and #t2.rank = (' + CAST(@iter_no AS nvarchar(8)) + '+1) and #t2.object_id IS NULL and #t2.object_svr IS NULL'
            EXEC (@query)
            -- @type
            SET @query = 'update #t2 set object_db = N' + @quote_quoted_dbname + ', object_id = t.user_type_id, object_type = case when t.is_assembly_type = 1 then ' + CAST(@udt AS nvarchar(8)) + ' when t.is_table_type = 1 then ' + CAST(@udtt AS nvarchar(8)) + ' else 
' + CAST(@uddt AS nvarchar(8)) + ' end
                from ' + @bracket_quoted_dbname + '.sys.types as t
                join ' + @bracket_quoted_dbname + '.sys.schemas as sch on sch.schema_id = t.schema_id
                where t.name = #t2.object_name collate database_default
                and sch.name = #t2.object_schema collate database_default
                and #t2.object_type = ' + CAST(@type AS nvarchar(8)) + ' and #t2.object_schema IS NOT NULL 
                and (#t2.object_db IS NULL or #t2.object_db = N' + @quote_quoted_dbname + ')
                and #t2.rank = (' + CAST(@iter_no AS nvarchar(8)) + '+1) and #t2.object_id IS NULL and #t2.object_svr IS NULL'
            EXEC (@query)

            -- @xml
            SET @query = 'update #t2 set object_db = N' + @quote_quoted_dbname + ', object_id = x.xml_collection_id 
                from ' + @bracket_quoted_dbname + '.sys.xml_schema_collections as x
                join ' + @bracket_quoted_dbname + '.sys.schemas as sch on sch.schema_id = x.schema_id
                where x.name = #t2.object_name collate database_default
                and sch.name = #t2.object_schema collate database_default
                and #t2.object_type = ' + CAST(@xml AS nvarchar(8)) + ' and #t2.object_schema IS NOT NULL 
                and (#t2.object_db IS NULL or #t2.object_db = N' + @quote_quoted_dbname + ')
                and #t2.rank = (' + CAST(@iter_no AS nvarchar(8)) + '+1) and #t2.object_id IS NULL and #t2.object_svr IS NULL'
            EXEC (@query)
            -- @part_func - schema is always null
            -- @schema is null
            -- consider schema as 'dbo'
            -- @obj
            SET @query = 'update #t2 set object_db = N' + @quote_quoted_dbname + ', object_id = obj.object_id, object_schema = SCHEMA_NAME(obj.schema_id), object_type = 
                            case when obj.type = ''U'' then ' + CAST(@u AS nvarchar(8)) +
                            ' when obj.type = ''V'' then ' + CAST(@v AS nvarchar(8)) +
                            ' when obj.type = ''TR'' then ' + CAST(@tr AS nvarchar(8)) +
                            ' when obj.type in ( ''P'', ''RF'', ''PC'' ) then ' + CAST(@sp AS nvarchar(8)) +
                            ' when obj.type in ( ''AF'' ) then ' + CAST(@uda AS nvarchar(8)) +
                            ' when obj.type in ( ''TF'', ''FN'', ''IF'', ''FS'', ''FT'' ) then ' + CAST(@udf AS nvarchar(8)) +
                            ' when obj.type = ''D'' then ' + CAST(@def AS nvarchar(8)) +
                            ' when obj.type = ''SN'' then ' + CAST(@synonym AS nvarchar(8)) +
                            ' when obj.type = ''SO'' then ' + CAST(@sequence AS nvarchar(8)) +
                            ' else ' + CAST(@unknown AS nvarchar(8)) +
                            ' end
                from ' + @bracket_quoted_dbname + '.sys.objects as obj 
                where obj.name = #t2.object_name collate database_default
                and SCHEMA_NAME(obj.schema_id) = ''dbo''
                and #t2.object_type = ' + CAST(@obj AS nvarchar(8)) + ' and #t2.object_schema IS NULL 
                and (#t2.object_db IS NULL or #t2.object_db = N' + @quote_quoted_dbname + ')
                and #t2.rank = (' + CAST(@iter_no AS nvarchar(8)) + '+1) and #t2.object_id IS NULL and #t2.object_svr IS NULL'
            EXEC (@query)
            -- @type
            SET @query = 'update #t2 set object_db = N' + @quote_quoted_dbname + ', object_id = t.user_type_id, object_schema = SCHEMA_NAME(t.schema_id), object_type = case when t.is_assembly_type = 1 then ' + CAST(@udt AS nvarchar(8)) + ' when t.is_table_type = 1 
then ' + CAST(@udtt AS nvarchar(8)) + ' else ' + CAST(@uddt AS nvarchar(8)) + ' end
                from ' + @bracket_quoted_dbname + '.sys.types as t
                where t.name = #t2.object_name collate database_default
                and SCHEMA_NAME(t.schema_id) = ''dbo''
                and #t2.object_type = ' + CAST(@type AS nvarchar(8)) + ' and #t2.object_schema IS NULL 
                and (#t2.object_db IS NULL or #t2.object_db = N' + @quote_quoted_dbname + ')
                and #t2.rank = (' + CAST(@iter_no AS nvarchar(8)) + '+1) and #t2.object_id IS NULL and #t2.object_svr IS NULL'
            EXEC (@query)
            -- @xml
            SET @query = 'update #t2 set object_db = N' + @quote_quoted_dbname + ', object_id = x.xml_collection_id, object_schema = SCHEMA_NAME(x.schema_id)
                from ' + @bracket_quoted_dbname + '.sys.xml_schema_collections as x
                where x.name = #t2.object_name collate database_default
                and SCHEMA_NAME(x.schema_id) = ''dbo''
                and #t2.object_type = ' + CAST(@xml AS nvarchar(8)) + ' and #t2.object_schema IS NULL 
                and (#t2.object_db IS NULL or #t2.object_db = N' + @quote_quoted_dbname + ')
                and #t2.rank = (' + CAST(@iter_no AS nvarchar(8)) + '+1) and #t2.object_id IS NULL and #t2.object_svr IS NULL'
            EXEC (@query)

            -- consider schema as t.relative_schema
            -- the parent object will have the default schema of user in case of dynamic schema binding
            -- @obj
            SET @query = 'update #t2 set object_db = N' + @quote_quoted_dbname + ', object_id = obj.object_id, object_schema = SCHEMA_NAME(obj.schema_id), object_type = 
                            case when obj.type = ''U'' then ' + CAST(@u AS nvarchar(8)) +
                            ' when obj.type = ''V'' then ' + CAST(@v AS nvarchar(8)) +
                            ' when obj.type = ''TR'' then ' + CAST(@tr AS nvarchar(8)) +
                            ' when obj.type in ( ''P'', ''RF'', ''PC'' ) then ' + CAST(@sp AS nvarchar(8)) +
                            ' when obj.type in ( ''AF'' ) then ' + CAST(@uda AS nvarchar(8)) +
                            ' when obj.type in ( ''TF'', ''FN'', ''IF'', ''FS'', ''FT'' ) then ' + CAST(@udf AS nvarchar(8)) +
                            ' when obj.type = ''D'' then ' + CAST(@def AS nvarchar(8)) +
                            ' when obj.type = ''SN'' then ' + CAST(@synonym AS nvarchar(8)) +
                            ' when obj.type = ''SO'' then ' + CAST(@sequence AS nvarchar(8)) +
                            ' else ' + CAST(@unknown AS nvarchar(8)) +
                            ' end
                from ' + @bracket_quoted_dbname + '.sys.objects as obj 
                join ' + @bracket_quoted_dbname + '.sys.schemas as sch on sch.schema_id = obj.schema_id
                where obj.name = #t2.object_name collate database_default
                and sch.name = #t2.relative_schema collate database_default
                and #t2.object_type = ' + CAST(@obj AS nvarchar(8)) + ' and #t2.object_schema IS NULL 
                and (#t2.object_db IS NULL or #t2.object_db = N' + @quote_quoted_dbname + ')
                and #t2.rank = (' + CAST(@iter_no AS nvarchar(8)) + '+1) and #t2.object_id IS NULL and #t2.object_svr IS NULL'
            EXEC (@query)

            -- @type
            SET @query = 'update #t2 set object_db = N' + @quote_quoted_dbname + ', object_id = t.user_type_id, object_schema = SCHEMA_NAME(t.schema_id), object_type = case when t.is_assembly_type = 1 then ' + CAST(@udt AS nvarchar(8)) + ' when t.is_table_type = 1 
then ' + CAST(@udtt AS nvarchar(8)) + ' else ' + CAST(@uddt AS nvarchar(8)) + ' end
                from ' + @bracket_quoted_dbname + '.sys.types as t
                join ' + @bracket_quoted_dbname + '.sys.schemas as sch on sch.schema_id = t.schema_id
                where t.name = #t2.object_name collate database_default
                and sch.name = #t2.relative_schema collate database_default
                and #t2.object_type = ' + CAST(@type AS nvarchar(8)) + ' and #t2.object_schema IS NULL 
                and (#t2.object_db IS NULL or #t2.object_db = N' + @quote_quoted_dbname + ')
                and #t2.rank = (' + CAST(@iter_no AS nvarchar(8)) + '+1) and #t2.object_id IS NULL and #t2.object_svr IS NULL'
            EXEC (@query)

            -- @xml
            SET @query = 'update #t2 set object_db = N' + @quote_quoted_dbname + ', object_id = x.xml_collection_id, object_schema = SCHEMA_NAME(x.schema_id)
                from ' + @bracket_quoted_dbname + '.sys.xml_schema_collections as x
                join ' + @bracket_quoted_dbname + '.sys.schemas as sch on sch.schema_id = x.schema_id
                where x.name = #t2.object_name collate database_default
                and sch.name = #t2.relative_schema collate database_default
                and #t2.object_type = ' + CAST(@xml AS nvarchar(8)) + ' and #t2.object_schema IS NULL 
                and (#t2.object_db IS NULL or #t2.object_db = N' + @quote_quoted_dbname + ')
                and #t2.rank = (' + CAST(@iter_no AS nvarchar(8)) + '+1) and #t2.object_id IS NULL and #t2.object_svr IS NULL'
            EXEC (@query)

            -- @part_func always have schema as null
            SET @query = 'update #t2 set object_db = N' + @quote_quoted_dbname + ', object_id = p.function_id
                from ' + @bracket_quoted_dbname + '.sys.partition_functions as p
                where p.name = #t2.object_name collate database_default
                and #t2.object_type = ' + CAST(@part_func AS nvarchar(8)) + 
                ' and (#t2.object_db IS NULL or #t2.object_db = N' + @quote_quoted_dbname + ')
                and #t2.rank = (' + CAST(@iter_no AS nvarchar(8)) + '+1) and #t2.object_id IS NULL and #t2.object_svr IS NULL'
            EXEC (@query)

            -- update the shared object if any (schema is not null)
            update #t2 set object_db = 'master', object_id = o.object_id, object_type = @sp
            from master.sys.objects as o 
            join master.sys.schemas as sch on sch.schema_id = o.schema_id
            where o.name = #t2.object_name collate database_default and sch.name = #t2.object_schema collate database_default and 
            o.type in ('P', 'RF', 'PC') and #t2.object_id IS null and
            #t2.object_name LIKE 'sp/_%' ESCAPE '/' and #t2.object_db IS null and #t2.object_svr IS null

            -- update the shared object if any (schema is null)
            update #t2 set object_db = 'master', object_id = o.object_id, object_schema = SCHEMA_NAME(o.schema_id), object_type = @sp
            from master.sys.objects as o 
            where o.name = #t2.object_name collate database_default and SCHEMA_NAME(o.schema_id) = 'dbo' collate database_default  and 
            o.type in ('P', 'RF', 'PC') and 
            #t2.object_schema IS null and #t2.object_id IS null and
            #t2.object_name LIKE 'sp/_%' ESCAPE '/' and #t2.object_db IS null and #t2.object_svr IS null

            FETCH NEXT FROM db_cursor INTO @dbname
        END
        CLOSE db_cursor
        DEALLOCATE db_cursor

    update #t2 set object_type = @unknown where object_id IS NULL

        insert #t1 (object_id, object_name, object_schema, object_db, object_svr, object_type, relative_id, relative_name, relative_schema, relative_db, relative_svr, relative_type, schema_bound, rank)
            select object_id, object_name, object_schema, object_db, object_svr, object_type, relative_id, relative_name, relative_schema, relative_db, relative_svr, relative_type, schema_bound, rank 
            from #t2 where @iter_no + 1 = rank
        SET @rows = @rows + @@rowcount


        -- HARD DEPENDENCIES
        -- uddt or udt referenced by table
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select tp.user_type_id, tp.name, SCHEMA_NAME(tp.schema_id), t.object_db, case tp.is_assembly_type when 1 then @udt else @uddt end, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.columns as col on col.object_id = t.object_id
            join sys.types as tp on tp.user_type_id = col.user_type_id and tp.schema_id != 4
            where @iter_no = t.rank and t.object_type = @u and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- uddt or udt referenced by table type
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select tp.user_type_id, tp.name, SCHEMA_NAME(tp.schema_id), t.object_db, case tp.is_assembly_type when 1 then @udt else @uddt end, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.table_types as tt on tt.user_type_id = t.object_id
            join sys.columns as col on col.object_id = tt.type_table_object_id
            join sys.types as tp on tp.user_type_id = col.user_type_id and tp.schema_id != 4
            where @iter_no = t.rank and t.object_type = @udtt and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- table or view referenced by trigger
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select o.object_id, o.name, SCHEMA_NAME(o.schema_id), t.object_db, case o.type when 'V' then @v else @u end, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.triggers as tr on tr.object_id = t.object_id
            join sys.objects as o on o.object_id = tr.parent_id
            where @iter_no = t.rank and t.object_type = @tr and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- defaults (only default objects) referenced by tables
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select o.object_id, o.name, SCHEMA_NAME(o.schema_id), t.object_db, @def, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.columns as clmns on clmns.object_id = t.object_id
            join sys.objects as o on o.object_id = clmns.default_object_id and 0 = isnull(o.parent_object_id, 0)
            where  @iter_no = t.rank and t.object_type = @u and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- defaults (only default objects) referenced by types
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select o.object_id, o.name, SCHEMA_NAME(o.schema_id), t.object_db, @def, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.types as tp on tp.user_type_id = t.object_id
            join sys.objects as o on o.object_id = tp.default_object_id and 0 = isnull(o.parent_object_id, 0)
            where @iter_no = t.rank and t.object_type = @uddt and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount
      
        -- rules referenced by tables
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select o.object_id, o.name, SCHEMA_NAME(o.schema_id), t.object_db, @rule, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.columns as clmns on clmns.object_id = t.object_id
            join sys.objects as o on o.object_id = clmns.rule_object_id and 0 = isnull(o.parent_object_id, 0)
            where @iter_no = t.rank and t.relative_type = @u and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- rules referenced by types
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select o.object_id, o.name, SCHEMA_NAME(o.schema_id), t.object_db, @rule, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.types as tp on tp.user_type_id = t.object_id
            join sys.objects as o on o.object_id = tp.rule_object_id and 0 = isnull(o.parent_object_id, 0)
            where @iter_no = t.rank and t.relative_type = @uddt and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount
        
        -- XmlSchemaCollections referenced by tables
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select x.xml_collection_id, x.name, SCHEMA_NAME(x.schema_id), t.object_db, @xml, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.columns as c on c.object_id = t.object_id
            join sys.xml_schema_collections as x on x.xml_collection_id = c.xml_collection_id and x.schema_id != 4
            where @iter_no = t.rank and t.object_type = @u and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- XmlSchemaCollections referenced by tabletypes
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select x.xml_collection_id, x.name, SCHEMA_NAME(x.schema_id), t.object_db, @xml, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.table_types as tt on tt.user_type_id = t.object_id
            join sys.columns as c on c.object_id = tt.type_table_object_id
            join sys.xml_schema_collections as x on x.xml_collection_id = c.xml_collection_id and x.schema_id != 4
            where @iter_no = t.rank and t.object_type = @udtt and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- XmlSchemaCollections referenced by procedures
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select x.xml_collection_id, x.name, SCHEMA_NAME(x.schema_id), t.object_db, @xml, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.parameters as c on c.object_id = t.object_id
            join sys.xml_schema_collections as x on x.xml_collection_id = c.xml_collection_id and x.schema_id != 4
            where @iter_no = t.rank and t.object_type in (@sp, @udf) and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- table referenced by table
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select tbl.object_id, tbl.name, SCHEMA_NAME(tbl.schema_id), t.object_db, @u, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.foreign_keys as fk on fk.parent_object_id = t.object_id
            join sys.tables as tbl on tbl.object_id = fk.referenced_object_id
            where @iter_no = t.rank and t.object_type = @u and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- uddts referenced by uda
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select tp.user_type_id, tp.name, SCHEMA_NAME(tp.schema_id), t.object_db, case when tp.is_table_type = 1 then @udtt when tp.is_assembly_type = 1 then @udt else @uddt end, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no 
+ 1
            from #t1 as t
            join sys.parameters as p on p.object_id = t.object_id
            join sys.types as tp on tp.user_type_id = p.user_type_id
            where @iter_no = t.rank and t.object_type = @uda and t.object_type = @uda and tp.user_type_id>256
        set @rows = @rows + @@rowcount

        -- assembly referenced by assembly
        insert #t1 (object_id, object_name, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select asm.assembly_id, asm.name, t.object_db, @assm, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.assembly_references as ar on ((ar.assembly_id = t.object_id) and (ar.referenced_assembly_id >= 65536))
            join sys.assemblies as asm on asm.assembly_id = ar.referenced_assembly_id
            where @iter_no = t.rank and t.object_type = @assm and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- assembly referenced by udt
        insert #t1 (object_id, object_name, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select asm.assembly_id, asm.name, t.object_db, @assm, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.assembly_types as at on ((at.user_type_id = t.object_id) and (at.is_user_defined = 1))
            join sys.assemblies as asm on asm.assembly_id = at.assembly_id
            where @iter_no = t.rank and t.object_type = @udt and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- assembly referenced by udf, sp, uda, trigger
        insert #t1 (object_id, object_name, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select asm.assembly_id, asm.name, t.object_db, @assm, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.assembly_modules as am on ((am.object_id = t.object_id) and (am.assembly_id >= 65536))
            join sys.assemblies as asm on asm.assembly_id = am.assembly_id
            where @iter_no = t.rank and t.object_type in ( @udf, @sp, @uda, @tr) and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- Partition Schemes referenced by tables/views
        insert #t1 (object_id, object_name, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select ps.data_space_id, ps.name, t.object_db, @part_sch, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.indexes as idx on idx.object_id = t.object_id
            join sys.partition_schemes as ps on ps.data_space_id = idx.data_space_id
            where @iter_no = t.rank and t.object_type in (@u, @v) and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- Partition Function referenced by Partition Schemes
        insert #t1 (object_id, object_name, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select pf.function_id, pf.name, t.object_db, @part_func, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.partition_schemes as ps on ps.data_space_id = t.object_id
            join sys.partition_functions as pf on pf.function_id = ps.function_id
            where @iter_no = t.rank and t.object_type = @part_sch and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount
        
        -- sp, udf, triggers referenced by plan guide
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select o.object_id, o.name, SCHEMA_NAME(o.schema_id), t.object_db, (case o.type when 'P' then @sp when 'TR' then @tr else @udf end), t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.plan_guides as pg on pg.plan_guide_id = t.object_id
            join sys.objects as o on o.object_id = pg.scope_object_id
            where @iter_no = t.rank and t.object_type = @pg and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount

        -- objects referenced by synonym
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select o.object_id, o.name, SCHEMA_NAME(o.schema_id), t.object_db, (case when o.type = 'U' then @u when o.type = 'V' then @v when o.type in ('P', 'RF', 'PC') then @sp when o.type = 'AF' then @uda else @udf end), t.object_id, t.object_name, 
t.object_schema, t.object_db, t.object_type, 0, @iter_no + 1
            from #t1 as t
            join sys.synonyms as s on s.object_id = t.object_id
            join sys.objects as o on o.object_id = OBJECT_ID(s.base_object_name) and o.type in ('U', 'V', 'P', 'RF', 'PC', 'AF', 'TF', 'FN', 'IF', 'FS', 'FT')
            where @iter_no = t.rank and t.object_type = @synonym and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount
        
        -- uddt referenced by sequence. Used to find UDDT that is in sequence dependencies.
        insert #t1 (object_id, object_name, object_schema, object_db, object_type, relative_id, relative_name, relative_schema, relative_db, relative_type, schema_bound, rank)
            select tp.user_type_id, tp.name, SCHEMA_NAME(tp.schema_id), t.object_db, case tp.is_assembly_type when 1 then @udt else @uddt end, t.object_id, t.object_name, t.object_schema, t.object_db, t.object_type, 1, @iter_no + 1
            from #t1 as t
            join sys.sequences as s on s.object_id = t.object_id
            join sys.types as tp on tp.user_type_id = s.user_type_id and tp.schema_id != 4
            where @iter_no = t.rank and t.object_type = @sequence and (t.object_svr IS null and t.object_db = db_name())
        set @rows = @rows + @@rowcount						
        
    end
    set @iter_no = @iter_no + 1
end

update #t1 set rank = 0
-- computing the degree of the nodes
update #t1 set degree = (
    select count(*) from #t1 t
    where t.relative_id = #t1.object_id and t.object_id != t.relative_id)

-- perform the topological sorting
set @iter_no = 1
while 1 = 1
begin
    update #t1 set rank=@iter_no where degree = 0
    -- end the loop if no more rows left to process
    if (@@rowcount = 0) break
    update #t1 set degree = NULL where rank = @iter_no

    update #t1 set degree = (
        select count(*) from #t1 t
        where t.relative_id = #t1.object_id and t.object_id != t.relative_id
        and t.object_id in (select tt.object_id from #t1 tt where tt.rank = 0))
        where degree is not null

    set @iter_no = @iter_no + 1
end

--correcting naming mistakes of objects present in current database 
--This part need to be removed once SMO's URN comparision gets fixed
        DECLARE @collation sysname;
        DECLARE db_cursor CURSOR
        FOR
            select distinct ISNULL(object_db, db_name()) from #t1 as t
            where t.object_id IS NOT NULL and t.object_svr IS NULL
        OPEN db_cursor
        FETCH NEXT FROM db_cursor INTO @dbname
        WHILE (@@FETCH_STATUS <> -1)
        BEGIN
            IF (db_id(@dbname) IS NULL) 
            BEGIN
                FETCH NEXT FROM db_cursor INTO @dbname
                CONTINUE
            END
            
            SET @collation = (select convert(sysname,DatabasePropertyEx(@dbname,'Collation')));
            SET @query = 'update #t1 set #t1.object_name = o.name,#t1.object_schema = sch.name from #t1  inner join '+ quotename(@dbname)+ '.sys.objects as o on #t1.object_id = o.object_id inner join '+ quotename(@dbname)+ '.sys.schemas as sch on sch.schema_id = 
o.schema_id  where o.name = #t1.object_name collate '+  @collation +' and sch.name = #t1.object_schema collate '+ @collation
            EXEC (@query)	


            FETCH NEXT FROM db_cursor INTO @dbname
        END
        CLOSE db_cursor
        DEALLOCATE db_cursor
    

--final select
select ISNULL(t.object_id, 0) as [object_id], t.object_name, ISNULL(t.object_schema, '') as [object_schema], ISNULL(t.object_db, '') as [object_db], ISNULL(t.object_svr, '') as [object_svr], t.object_type, ISNULL(t.relative_id, 0) as [relative_id], t.relative_name, 
ISNULL(t.relative_schema, '') as [relative_schema], relative_db, ISNULL(t.relative_svr, '') as [relative_svr], t.relative_type, t.schema_bound, ISNULL(CASE WHEN p.type= 'U' then @u when p.type = 'V' then @v end, 0) as [ptype], ISNULL(p.name, '') as [pname], 
ISNULL(SCHEMA_NAME(p.schema_id), '') as [pschema]
 from #t1 as t
 left join sys.objects as o on (t.object_type = @tr and o.object_id = t.object_id) or (t.relative_type = @tr and o.object_id = t.relative_id)
 left join sys.objects as p on p.object_id = o.parent_object_id
 order by rank desc
 
drop table #t1
drop table #t2
drop table #tempdep

IF @must_set_nocount_off > 0 
   set nocount off
	USE [master]

--exec sp_executesql N'SELECT
--SCHEMA_NAME(tbl.schema_id) AS [Schema],
--tbl.name AS [Name],
--tbl.object_id AS [ID]
--FROM
--sys.tables AS tbl
--WHERE
--(tbl.name=@_msparam_0 and SCHEMA_NAME(tbl.schema_id)=@_msparam_1)',N'@_msparam_0 nvarchar(4000),@_msparam_1 nvarchar(4000)',@_msparam_0=N'ProductCategory',@_msparam_1=N'SalesLT'

--exec sp_executesql N'
--        CREATE TABLE #tmp_extended_remote_data_archive_tables
--        (object_id int not null, remote_table_name nvarchar(128) null, filter_predicate nvarchar(max) null, migration_state tinyint null)

--        IF EXISTS(SELECT 1 FROM master.sys.syscolumns WHERE Name = N''remote_data_archive_migration_state'' AND ID = Object_ID(N''sys.tables''))
--        EXECUTE(N''INSERT INTO #tmp_extended_remote_data_archive_tables SELECT rdat.object_id, rdat.remote_table_name,
--        SUBSTRING(rdat.filter_predicate, 2, LEN(rdat.filter_predicate) - 2) as filter_predicate,
--        CASE
--        WHEN tbl.remote_data_archive_migration_state_desc = N''''PAUSED'''' THEN 1
--        WHEN tbl.remote_data_archive_migration_state_desc = N''''OUTBOUND'''' THEN 3
--        WHEN tbl.remote_data_archive_migration_state_desc = N''''INBOUND'''' THEN 4
--        WHEN tbl.remote_data_archive_migration_state_desc = N''''DISABLED'''' THEN 0
--        ELSE 0
--        END AS migration_state
--        FROM sys.tables tbl LEFT JOIN sys.remote_data_archive_tables rdat ON rdat.object_id = tbl.object_id
--        WHERE rdat.object_id IS NOT NULL'')
--        ELSE
--        EXECUTE(N''INSERT INTO #tmp_extended_remote_data_archive_tables SELECT rdat.object_id, rdat.remote_table_name,
--        SUBSTRING(rdat.filter_predicate, 2, LEN(rdat.filter_predicate) - 2) as filter_predicate,
--        CASE
--        WHEN rdat.is_migration_paused = 1 AND rdat.migration_direction_desc = N''''OUTBOUND'''' THEN 1
--        WHEN rdat.is_migration_paused = 1 AND rdat.migration_direction_desc = N''''INBOUND'''' THEN 2
--        WHEN rdat.is_migration_paused = 0 AND rdat.migration_direction_desc = N''''OUTBOUND'''' THEN 3
--        WHEN rdat.is_migration_paused = 0 AND rdat.migration_direction_desc = N''''INBOUND'''' THEN 4
--        ELSE 0
--        END AS migration_state
--        FROM sys.tables tbl LEFT JOIN sys.remote_data_archive_tables rdat ON rdat.object_id = tbl.object_id
--        WHERE rdat.object_id IS NOT NULL'')
      


--SELECT
--tbl.name AS [Name],
--tbl.object_id AS [ID],
--tbl.create_date AS [CreateDate],
--tbl.modify_date AS [DateLastModified],
--ISNULL(stbl.name, N'''') AS [Owner],
--CAST(case when tbl.principal_id is null then 1 else 0 end AS bit) AS [IsSchemaOwned],
--SCHEMA_NAME(tbl.schema_id) AS [Schema],
--CAST(
-- case 
--    when tbl.is_ms_shipped = 1 then 1
--    when (
--        select 
--            major_id 
--        from 
--            sys.extended_properties 
--        where 
--            major_id = tbl.object_id and 
--            minor_id = 0 and 
--            class = 1 and 
--            name = N''microsoft_database_tools_support'') 
--        is not null then 1
--    else 0
--end          
--             AS bit) AS [IsSystemObject],
--CAST(OBJECTPROPERTY(tbl.object_id, N''HasAfterTrigger'') AS bit) AS [HasAfterTrigger],
--CAST(OBJECTPROPERTY(tbl.object_id, N''HasInsertTrigger'') AS bit) AS [HasInsertTrigger],
--CAST(OBJECTPROPERTY(tbl.object_id, N''HasDeleteTrigger'') AS bit) AS [HasDeleteTrigger],
--CAST(OBJECTPROPERTY(tbl.object_id, N''HasInsteadOfTrigger'') AS bit) AS [HasInsteadOfTrigger],
--CAST(OBJECTPROPERTY(tbl.object_id, N''HasUpdateTrigger'') AS bit) AS [HasUpdateTrigger],
--CAST(OBJECTPROPERTY(tbl.object_id, N''IsIndexed'') AS bit) AS [HasIndex],
--CAST(OBJECTPROPERTY(tbl.object_id, N''IsIndexable'') AS bit) AS [IsIndexable],
--CAST(CASE idx.index_id WHEN 1 THEN 1 ELSE 0 END AS bit) AS [HasClusteredIndex],
--CAST(ISNULL((select top 1 1 from sys.indexes ind where ind.object_id = tbl.object_id and ind.type > 1 and ind.is_hypothetical = 0 ), 0) AS bit) AS [HasNonClusteredIndex],
--CAST(case idx.index_id when 1 then case when (idx.is_primary_key + 2*idx.is_unique_constraint = 1) then 1 else 0 end else 0 end AS bit) AS [HasPrimaryClusteredIndex],
--CAST(ISNULL((select top 1 1 from sys.indexes ind where ind.object_id = tbl.object_id and ind.type = 6 and ind.is_hypothetical = 0 ), 0) AS bit) AS [HasNonClusteredColumnStoreIndex],
--CAST(ISNULL((select top 1 1 from sys.indexes ind where ind.object_id = tbl.object_id and ind.type = 3 and ind.is_hypothetical = 0 ), 0) AS bit) AS [HasXmlIndex],
--CAST(CASE idx.type WHEN 0 THEN 1 ELSE 0 END AS bit) AS [HasHeapIndex],
--CAST(ISNULL((select top 1 1 from sys.all_columns as clmns join sys.types as usrt on usrt.user_type_id = clmns.user_type_id where clmns.object_id = tbl.object_id and usrt.name = N''xml''), 0) AS bit) AS [HasXmlData],
--CAST(ISNULL((select top 1 1 from sys.all_columns as clmns join sys.types as usrt on usrt.user_type_id = clmns.user_type_id where clmns.object_id = tbl.object_id and usrt.name in (N''geometry'', N''geography'')), 0) AS bit) AS [HasSpatialData],
--tbl.uses_ansi_nulls AS [AnsiNullsStatus],
--CAST(ISNULL(OBJECTPROPERTY(tbl.object_id,N''IsQuotedIdentOn''),0) AS bit) AS [QuotedIdentifierStatus],
--CAST(0 AS bit) AS [FakeSystemTable],
--ISNULL(dstext.name,N'''') AS [TextFileGroup],
--CAST(0 AS bit) AS [IsLedger],
--CAST(0 AS bit) AS [IsDroppedLedgerTable],
--CAST(tbl.is_memory_optimized AS bit) AS [IsMemoryOptimized],
--case when (tbl.durability=1) then 0 else 1 end AS [Durability],
--tbl.is_replicated AS [Replicated],
--tbl.lock_escalation AS [LockEscalation],
--CAST(case when ctt.object_id is null then 0 else 1  end AS bit) AS [ChangeTrackingEnabled],
--CAST(ISNULL(ctt.is_track_columns_updated_on,0) AS bit) AS [TrackColumnsUpdatedEnabled],
--tbl.is_filetable AS [IsFileTable],
--ISNULL(ft.directory_name,N'''') AS [FileTableDirectoryName],
--ISNULL(ft.filename_collation_name,N'''') AS [FileTableNameColumnCollation],
--CAST(ISNULL(ft.is_enabled,0) AS bit) AS [FileTableNamespaceEnabled],
--CASE WHEN ''PS''=dsidx.type THEN dsidx.name ELSE N'''' END AS [PartitionScheme],
--CAST(CASE WHEN ''PS''=dsidx.type THEN 1 ELSE 0 END AS bit) AS [IsPartitioned],
--CASE WHEN ''FD''=dstbl.type THEN dstbl.name ELSE N'''' END AS [FileStreamFileGroup],
--CASE WHEN ''PS''=dstbl.type THEN dstbl.name ELSE N'''' END AS [FileStreamPartitionScheme],
--CAST(CASE idx.type WHEN 5 THEN 1 ELSE 0 END AS bit) AS [HasClusteredColumnStoreIndex],
--CAST(ISNULL(historyTable.name, N'''') AS sysname) AS [HistoryTableName],
--CAST(ISNULL(SCHEMA_NAME(historyTable.schema_id), N'''') AS sysname) AS [HistoryTableSchema],
--CAST(ISNULL(historyTable.object_id, 0) AS int) AS [HistoryTableID],
--CAST(CASE WHEN periods.start_column_id IS NULL THEN 0 ELSE 1 END AS bit) AS [HasSystemTimePeriod],
--CAST(
--        ISNULL((SELECT cols.name
--        FROM sys.columns cols
--        WHERE periods.object_id = tbl.object_id
--        AND cols.object_id = tbl.object_id
--        AND cols.column_id = periods.start_column_id), N'''')
--       AS sysname) AS [SystemTimePeriodStartColumn],
--CAST(
--        ISNULL((SELECT cols.name
--        FROM sys.columns cols
--        WHERE periods.object_id = tbl.object_id
--        AND cols.object_id = tbl.object_id
--        AND cols.column_id = periods.end_column_id), N'''')
--       AS sysname) AS [SystemTimePeriodEndColumn],
--tbl.temporal_type AS [TemporalType],
--CAST(CASE tbl.temporal_type WHEN 2 THEN 1 ELSE 0 END AS bit) AS [IsSystemVersioned],
--CAST(tbl.is_remote_data_archive_enabled AS bit) AS [RemoteDataArchiveEnabled],
--CAST(
--        ISNULL(rdat.migration_state, 0)
--       AS tinyint) AS [RemoteDataArchiveDataMigrationState],
--CAST(rdat.filter_predicate AS varchar(4000)) AS [RemoteDataArchiveFilterPredicate],
--CAST(rdat.remote_table_name AS sysname) AS [RemoteTableName],
--CAST(CASE WHEN rdat.remote_table_name IS NULL THEN 0 ELSE 1 END AS bit) AS [RemoteTableProvisioned],
--CAST(tbl.is_external AS bit) AS [IsExternal],
--CAST(0 AS bit) AS [IsNode],
--CAST(0 AS bit) AS [IsEdge]
--FROM
--sys.tables AS tbl
--LEFT OUTER JOIN sys.database_principals AS stbl ON stbl.principal_id = ISNULL(tbl.principal_id, (OBJECTPROPERTY(tbl.object_id, ''OwnerId'')))
--INNER JOIN sys.indexes AS idx ON 
--        idx.object_id = tbl.object_id and (idx.index_id < @_msparam_0  or (tbl.is_memory_optimized = 1 and idx.index_id = (select min(index_id) from sys.indexes where object_id = tbl.object_id)))
      
--LEFT OUTER JOIN sys.data_spaces AS dstext  ON tbl.lob_data_space_id = dstext.data_space_id
--LEFT OUTER JOIN sys.change_tracking_tables AS ctt ON ctt.object_id = tbl.object_id 
--LEFT OUTER JOIN sys.filetables AS ft ON ft.object_id = tbl.object_id
--LEFT OUTER JOIN sys.data_spaces AS dsidx ON dsidx.data_space_id = idx.data_space_id
--LEFT OUTER JOIN sys.tables AS t ON t.object_id = idx.object_id
--LEFT OUTER JOIN sys.data_spaces AS dstbl ON dstbl.data_space_id = t.Filestream_data_space_id and (idx.index_id < 2 or (idx.type = 7 and idx.index_id < 3))
--LEFT OUTER JOIN sys.tables as historyTable ON historyTable.object_id = tbl.history_table_id
--LEFT OUTER JOIN sys.periods as periods ON periods.object_id = tbl.object_id
--LEFT OUTER JOIN #tmp_extended_remote_data_archive_tables AS rdat ON rdat.object_id = tbl.object_id
--WHERE
--(tbl.name=@_msparam_1 and SCHEMA_NAME(tbl.schema_id)=@_msparam_2)

--        DROP TABLE #tmp_extended_remote_data_archive_tables
      
--',N'@_msparam_0 nvarchar(4000),@_msparam_1 nvarchar(4000),@_msparam_2 nvarchar(4000)',@_msparam_0=N'2',@_msparam_1=N'ProductCategory',@_msparam_2=N'SalesLT'