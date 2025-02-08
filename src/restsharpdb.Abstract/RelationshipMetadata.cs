using PostgrestSharp.Abstract.Enums;

namespace PostgrestSharp.Abstract;

public class RelationshipMetadata
{
    // public string? SourceTable { get; set; }
    // public string? RelatedTable { get; set; }
    // public string? ForeignKeyColumn { get; set; }
    // public string? PrimaryKeyColumn { get; set; }
    // public RelationType RelationType { get; set; }
    // public string? JunctionTable { get; set; }
    // public string? TargetForeignKey { get; set; }
    // public string? RelatedPrimaryKey { get; set; }
    // public string? RelationshipName { get; set; }
    // public bool IsSelfReferencing { get; set; }
    // public string? SelfReferencingAlias { get; set; }
    // public string? SourceForeignKey { get; set; }
    
    public string SourceTable{get;set;}
    public string TargetTable{get;set;}
    public string FkColumn{get;set;}
    public string PkColumn{get;set;}
    public RelationsipType RelationshipType{get;set;}
    public string? JunctionTable{get;set;}
    public string? TargetFkColumn{get;set;}
    public string? TargetPkColumn{get;set;}
    public string? RelationshipName{get;set;}
    public bool IsSselfReferencing{get;set;}
    public string? SelfReferencingAlias{get;set;}
    
}

//source_table;target_table;fk_column;pk_column;relationship_type;junction_table;target_fk_column;target_pk_column;relationship_name;is_self_referencing;self_referencing_alias