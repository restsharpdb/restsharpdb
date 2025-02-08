namespace PostgrestSharp.Abstract;

public class TableMetadata
{
    //public string TableName { get; set; }
    public string ColumnName { get; set; } 
    public string ColumnDataType { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsNullable { get; set; }
    public int? MaxLength { get; set; }
    public bool IsSelfReferencing { get; set; }
    public int OrdinalPosition { get; set; }
}