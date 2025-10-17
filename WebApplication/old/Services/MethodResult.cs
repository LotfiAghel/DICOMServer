namespace EnglishToefl.Services
{
  public class MethodResult
  {
    public bool Success { get; set; } = false;

    public string Message { get; set; } = "";
  }

  public class MethodResult<TResult>
  {
    public bool Success { get; set; }

    public string Message { get; set; }

    public TResult Result { get; set; }
  }
}
