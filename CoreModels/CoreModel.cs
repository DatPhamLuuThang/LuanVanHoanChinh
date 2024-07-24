namespace CoreModels;

public abstract class CoreModel<TKey>
{
    /// <summary>
    /// ID 
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Hoạt động
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Được tạo bởi
    /// </summary>
    public required TKey CreatedBy { get; set; }

    /// <summary>
    /// Được tại lúc
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Cập nhật bởi
    /// </summary>
    public TKey? UpdatedBy { get; set; }

    /// <summary>
    /// Cập nhật lúc
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Xóa mềm???
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Được xóa bởi
    /// </summary>
    public TKey? DeletedBy { get; set; }

    /// <summary>
    /// Được xóa vào
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}