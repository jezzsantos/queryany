namespace DomainServices.Interfaces
{
    public interface IEmailService
    {
        bool EnsureEmailIsUnique(string emailAddress, string personId);
    }
}