namespace TestBlazor.Services
{
    public class CompassService
    {
        // Propriété stockant le cap (heading) actuel.
        // "private set" empêche la modification directe depuis l'extérieur, obligeant à passer par SetHeading.
        public int Heading { get; private set; } = 0;

        // Événement déclenché à chaque changement de cap.
        // Les composants peuvent s'abonner à cet événement pour mettre à jour leur interface.
        public event Action? OnChange;

        public void SetHeading(int heading)
        {
            // Normalisation de la valeur entre 0 et 360 degrés.
            // L'opérateur modulo (%) permet de gérer les valeurs > 360 ou négatives.
            Heading = heading % 360;
            if (Heading < 0) Heading += 360;
            
            // Notifie tous les abonnés que l'état a changé.
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
