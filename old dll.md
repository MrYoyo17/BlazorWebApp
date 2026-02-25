Approche 2 : Le Multi-ciblage (Multi-targeting) avec directives de compilation
Le SDK moderne de .NET permet à un seul projet de générer plusieurs DLLs différentes en même temps (une pour .NET 4, une pour .NET Standard). C'est ce qu'utilisent les créateurs de packages NuGet.

Dans le code, vous utilisez des directives de compilation (#if) pour dire au compilateur quelle version de la classe utiliser selon la cible.

C#
// Partout dans votre code où vous avez besoin d'un point :

#if NET472 // (ou la version exacte de votre .NET Framework)
    using MonTypeDePoint = System.Windows.Point;
#else
    using MonTypeDePoint = MonAncienProjet.Modeles.MonPoint;
#endif

public class DecodeurUDP
{
    public MonTypeDePoint ObtenirCoordonnees(byte[] trame)
    {
        // ... logique de décodage ...
        return new MonTypeDePoint(10, 20);
    }
}
Avantage : Vous gardez un seul projet et un seul code source. Lorsqu'il est compilé pour .NET 4, il utilise la DLL WindowsBase et le vrai Point de WPF. Lorsqu'il est compilé pour Blazor (.NET Standard), il utilise votre structure personnalisée.

Le choix de la solution
L'approche 1 (l'extraction d'un Core) est généralement plus saine sur le long terme car elle force une bonne séparation des responsabilités. L'approche 2 (Multi-ciblage) est plus rapide à mettre en place si votre projet est petit, mais le code peut vite devenir illisible si vous abusez des #if / #else.


Voici la procédure pas à pas pour transformer votre projet actuel sans casser l'existant.

Étape 1 : Passer au format "SDK Style"
Les anciens fichiers .csproj (ceux qui commencent par <Project ToolsVersion=...) gèrent très mal le multi-ciblage. Il faut moderniser le format du fichier projet.

Faites un clic droit sur votre projet de décodage dans l'Explorateur de solutions -> Décharger le projet.

Clic droit à nouveau -> Modifier le fichier projet.

Supprimez TOUT le contenu de ce fichier (oui, c'est effrayant, mais nécessaire).

Copiez-collez le contenu suivant à la place. C'est le format moderne, beaucoup plus court :

XML
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net472;netstandard2.0</TargetFrameworks>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
  </PropertyGroup>

  <ItemGroup Condition="'$(TargetFramework)' == 'net472'">
    <Reference Include="WindowsBase" />
  </ItemGroup>

  <ItemGroup Condition="'$(TargetFramework)' == 'netstandard2.0'">
    </ItemGroup>

</Project>
Fermez le fichier et faites un clic droit sur le projet -> Recharger le projet.

Étape 2 : Créer le "Faux" Point (Polyfill)
Maintenant, si vous essayez de compiler, la version .NET 4.7.2 va réussir (car elle a WindowsBase), mais la version netstandard2.0 va échouer car elle ne connaît pas System.Windows.Point.

L'astuce consiste à créer une classe qui imite System.Windows.Point, mais qui n'est visible que par la version .NET Standard.

Créez un nouveau fichier nommé WpfCompatibility.cs dans votre projet.

Copiez ce code dedans :

C#
// Ce bloc ne sera compilé QUE si on est en train de générer la version pour Blazor/Standard
#if NETSTANDARD2_0 

namespace System.Windows // Oui, on utilise le MÊME namespace que Microsoft !
{
    // On recrée la structure exactement comme elle existe dans WPF
    public struct Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        // Ajoutez ici d'autres méthodes si votre code les utilise (ex: Offset)
        public void Offset(double offsetX, double offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }
    }
}
#endif
Étape 3 : Le résultat
C'est là que c'est génial : Vous n'avez pas besoin de toucher à votre code métier existant.

Quand vous compilez pour WPF : Le compilateur ignore votre fichier WpfCompatibility.cs (grâce au #if). Votre code utilise donc le "vrai" System.Windows.Point de la DLL WindowsBase. Vos anciennes applications continuent de fonctionner sans rien changer.

Quand vous compilez pour Blazor : Le compilateur n'a pas WindowsBase, mais il trouve votre structure de remplacement dans le namespace System.Windows. Il l'utilise de manière transparente.

Vérification
Compilez la solution.

Allez dans le dossier de sortie (bin/Debug/).

Vous verrez deux dossiers : net472 et netstandard2.0.

Le dossier net472 contient la DLL compatible avec vos vieilles applications.

Le dossier netstandard2.0 contient la DLL compatible avec Blazor.

C'est la méthode la plus propre pour migrer progressivement sans casser l'existant.