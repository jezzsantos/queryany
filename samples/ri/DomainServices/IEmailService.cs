namespace DomainServices
{
    public interface IEmailService
    {
        bool EnsureEmailIsUnique(string emailAddress, string personId);
    }
}