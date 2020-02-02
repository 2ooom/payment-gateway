namespace PaymentGateway
{
    public interface IConfig
    {
        string JwtSecret { get; }
    }
}