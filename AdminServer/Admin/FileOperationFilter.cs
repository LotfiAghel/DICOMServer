namespace ChunkedUploadWebApi.Controllers
{
    /*public class FileOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.OperationId == "ApiFileUploadUserByUserIdSessionBySessionIdPut")
            {
                var p =operation.Parameters.Where(op => op.In == "formData").ToList();
                p.ForEach(item => operation.Parameters.Remove(item));

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "files",
                    In = "formData",
                    Description = "Upload File",
                    Required = true,
                    Type = "file"
                });
                operation.Consumes.Add("multipart/form-data");
            }
        }
    }*/
}