public class CheckResult{
    bool success;
    string message;

    public string Message => message;

    public CheckResult(bool success, string message)
    {
        this.success = success;
        this.message = message;
    }

    public static CheckResult Success()
    {
        return new CheckResult(true, "");
    }

    public static CheckResult Fail(string message)
    {
        return new CheckResult(false, message);
    }

    //as bool
    public static implicit operator bool(CheckResult result)
    {
        return result.success;
    }

    //cast string to CheckResult
    public static implicit operator CheckResult(string message)
    {
        return new CheckResult(false, message);
    }
}