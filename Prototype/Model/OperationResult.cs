namespace Prototype.Model
{
    public class OperationResult
    {
        public static readonly OperationResult Ok = new OperationResult();

        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
    }
}
