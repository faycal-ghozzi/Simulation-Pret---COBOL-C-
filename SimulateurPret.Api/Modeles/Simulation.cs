namespace SimulateurPret.Api.Modeles;

public class Simulation
{
    public int Id { get; set; }
    public decimal Montant { get; set; }
    public decimal TauxAnnuel { get; set; }
    public int DureeMois { get; set; }
    public decimal Mensualite { get; set; }
    public decimal CoutTotal { get; set; }
    public decimal InteretsTotal { get; set; }
    public DateTime DateCreation { get; set; }
    public string UserId { get; set; } = string.Empty;

}