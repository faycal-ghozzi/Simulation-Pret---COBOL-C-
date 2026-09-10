namespace SimulateurPret.Web.Services;

public class ServiceAuthEtat
{
    public string? Token {get; private set;}
    public event Action? EtatChange;
    public void Connecter(string token)
    {
        Token = token;
        EtatChange?.Invoke();
    }
    public void Deconnecter()
    {
        Token = null;
        EtatChange?.Invoke();
    }

    public bool EstConnecte => !string.IsNullOrEmpty(Token);
}