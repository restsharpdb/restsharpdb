using System.Diagnostics.CodeAnalysis;

namespace PostgrestSharp.Abstract.DTOs;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class SuccessResponse
{
    
    public string object_type { get; set; } 
    
    // ReSharper disable once CollectionNeverQueried.Global
    public List<Guid> ids { get; set; } = [];

    public SuccessResponse(string objectType, params Guid[] guids)
    {
        object_type = objectType;
        ids.AddRange(guids);
    }
    
}