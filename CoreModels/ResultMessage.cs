namespace CoreModels;

public class ResultMessage
{
    /// <summary>
    /// Trạng thái kết quả
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Thông báo
    /// </summary>
    public string Message { get; set; }


    
    /// <summary>
    ///Khởi tạo một ResuldMessage với trạng thái thành công và thng báo rỗng
    /// </summary>
    public ResultMessage()
    {
        IsSuccess = true;
        Message = "";
    }

    

    /// <summary>
    /// Khởi tạo một ResuldMessage với trạng thái và thông báo
    /// </summary>
    /// <param name="isSuccess">trạng thái của kết quả</param>
    /// <param name="message">Thông báo về kết quả</param>
    public ResultMessage(bool isSuccess, string message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    
    /// <summary>
    /// Khởi tạo một ResuldMessage với trạng thái và thông báo rỗng
    /// </summary>
    /// <param name="isSuccess">trạng thái của kết quả</param>
    public ResultMessage(bool isSuccess)
    {
        IsSuccess = isSuccess;
        Message = "";
    }
}