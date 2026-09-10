using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;

namespace SimulateurPret.Api.Services;

public record ResultatPret(
    decimal Mensualite,
    decimal CoutTotal,
    decimal InteretsTotal
);

public class ServiceMoteurCobol
{
    private readonly string _cheminExecutable;

    public ServiceMoteurCobol(string cheminExecutable)
    {
        _cheminExecutable = cheminExecutable;
    }

    public async Task<ResultatPret> CalculerAsync(
        decimal montant, decimal tauxAnnuel, int dureeMois
    )
    {
        string dossierTravail = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dossierTravail);

        try
        {
            string cheminEntree = Path.Combine(dossierTravail, "entree.txt");
            string cheminSortie = Path.Combine(dossierTravail, "sortie.txt");

            await File.WriteAllTextAsync(cheminEntree, FormaterLigneEntree(montant, tauxAnnuel, dureeMois));

            await ExecuterProgrammeCobolAsync(dossierTravail);

            string ligneSortie = (await File.ReadAllLinesAsync(cheminSortie)).First();

            return AnalyserLigneSortie(ligneSortie);
        }
        finally
        {
            Directory.Delete(dossierTravail, recursive: true);
        }
    }

    private static string FormaterLigneEntree(decimal montant, decimal tauxAnnuel, int dureeMois)
    {
        string montantFormate = FormaterMontantImplicite(montant, partieEntiere: 7);
        string tauxFormate = FormaterMontantImplicite(tauxAnnuel, partieEntiere: 2);
        string dureeFormate = dureeMois.ToString("D3", CultureInfo.InvariantCulture);

        return montantFormate + tauxFormate + dureeFormate;
    }

    private static string FormaterMontantImplicite(decimal valeur, int partieEntiere)
    {
        long centimes = (long)Math.Round(valeur * 100, MidpointRounding.AwayFromZero);
        return centimes.ToString().PadLeft(partieEntiere + 2, '0');
    }

    private Task ExecuterProgrammeCobolAsync(string dossierTravail)
    {
        var tcs = new TaskCompletionSource();

        var demarrage = new ProcessStartInfo
        {
            FileName = _cheminExecutable,
            WorkingDirectory = dossierTravail,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var processus = new Process { StartInfo = demarrage, EnableRaisingEvents = true};

        processus.Exited += (sender, args) =>
        {
            if(processus.ExitCode != 0)
            {
                tcs.SetException(new Exception($"Programme COBOL a echoue (code {processus.ExitCode})."));
            }
            else
            {
                tcs.SetResult();
            }
            processus.Dispose();
        };

        processus.Start();
        return tcs.Task;
    }

    private static ResultatPret AnalyserLigneSortie(string ligne)
    {
        decimal mensualite = ExtraireDecimaleImplicite(ligne.Substring(0,9), 2);
        decimal coutTotal = ExtraireDecimaleImplicite(ligne.Substring(9,11), 2);
        decimal InteretsTotal = ExtraireDecimaleImplicite(ligne.Substring(20,11), 2);

        return new ResultatPret(mensualite, coutTotal, InteretsTotal); 
    }

    private static decimal ExtraireDecimaleImplicite(string bloc, int nbDecimales)
    {
        long valeurBrute = long.Parse(bloc, CultureInfo.InvariantCulture);
        return valeurBrute / (decimal)Math.Pow(10, nbDecimales);
    }
}