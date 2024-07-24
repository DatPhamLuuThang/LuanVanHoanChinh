using AspNetCoreHero.ToastNotification.Abstractions;
using CoreModels;

namespace Utilities;

public static class Helper
{
    public static class FormatButton
    {
        public static class Details
        {
            public const string Icon = "anticon anticon-info-circle";
            public const string Title = "Chi tiết";
        }
        public static class Create
        {
            public const string Icon = "anticon anticon-plus-circle";
            public const string Title = "Thêm mới";
        }
        public static class Edit
        {
            public const string Icon = "anticon anticon-edit";
            public const string Title = "Chỉnh sửa";
        }
        public static class Delete
        {
            public const string Icon = "anticon anticon-delete";
            public const string Title = "Xóa";
        }
        public static class Duplicate
        {
            public const string Icon = "anticon anticon-copy";
            public const string Title = "Nhân bản";
        }
        public static class Close
        {
            public const string Icon = "anticon anticon-close-circle";
            public const string Title = "Đóng";
        }
        public static class Cancel
        {
            public const string Icon = "anticon anticon-close-circle";
            public const string Title = "Hủy";
        }
    }
    public static class FormatDateTime
    {
        public const string OnlyDate = "dd/MM/yyyy";
        public const string OnlyTime = "HH:mm";
    }
    
    public static class NotyfMsg
    {
        public const string Success = "Thao tác hoàn tất";
        public const string Warning = "Kiểm tra lại thông tin";
        public const string Error = "Đã xãy ra lỗi";
    }
    
    public static void NotyfAssist(ResultMessage result, INotyfService notyfService)
    {
        if (result.IsSuccess)
        {
            notyfService.Success(result.Message);
        }
        else
        {
            notyfService.Error(result.Message);
        }
    }


    public static class StaticUrl
    {
        private const string BaseUrl = "~/Views/Dynamic/";
        
        public const string Empty = BaseUrl + "Empty.cshtml";
        public const string Index = BaseUrl + "Index.cshtml";
        public const string Details = BaseUrl + "Details.cshtml";
        public const string Delete = BaseUrl + "Delete.cshtml";
        public const string CreateOrUpdate = BaseUrl + "Modify.cshtml";
        public const string Button = BaseUrl + "Button.cshtml";                      
        public const string Duplicate = BaseUrl + "Duplicate.cshtml";

        public static class ButtonChild
        {
            private const string BaseUrlButton = BaseUrl + "Button/";
            
            public const string ButtonDetails = BaseUrlButton + "Details.cshtml";
            public const string ButtonCreate = BaseUrlButton + "Create.cshtml";
            public const string ButtonEdit = BaseUrlButton + "Edit.cshtml";
            public const string ButtonDelete = BaseUrlButton + "Delete.cshtml";
            public const string ButtonDuplicate = BaseUrlButton + "Duplicate.cshtml";
        } 
    }
    
    public static class Method
    {
        public const string Add = "Add";
        public const string Update = "Update";
        public const string SoftDelete = "SoftDelete";
        public const string Duplicate = "Duplicate";
    }
    
    public static class DeniedAttribute
    {
        public static readonly List<string> List = new List<string>
        {
            "IsDeleted",
            "CreatedBy", "CreatedAt",
            "UpdatedBy", "UpdatedAt",
            "DeletedBy", "DeletedAt"
        };
    }
}