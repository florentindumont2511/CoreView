
CoreView

CoreView est un logiciel de monitoring système développé en C# / WPF (.NET 9), conçu pour être affiché sur un écran secondaire dédié (comme un écran HDMI 5" ou 7" intégré au setup PC).

L’objectif du projet est de proposer une interface :

minimaliste
lisible à distance
fluide
légère
optimisée pour un affichage permanent

Le logiciel récupère les données système via :

HWiNFO64 Shared Memory
et/ou LibreHardwareMonitor

Aperçu :
Fonctionnalités actuelles

-----------------------------------------------------------------------------------------------------
CPU
Usage CPU
Température CPU
Fréquence CPU
Consommation CPU
Tension CPU
-----------------------------------------------------------------------------------------------------
GPU
Usage GPU
Température GPU
Fréquence GPU
VRAM utilisée
Hotspot GPU
Memory Junction
Consommation GPU
Tension GPU
-----------------------------------------------------------------------------------------------------
Système
Horloge temps réel
Date complète
Affichage multi-écrans
Mode always-on-top
Démarrage automatique avec Windows
Icône tray Windows
Fenêtre d’options
Sauvegarde des paramètres


Technologies utilisées
C#
WPF
.NET 9
HWiNFO Shared Memory API
LibreHardwareMonitor
Hardcodet.NotifyIcon.Wpf
Interface

Le dashboard utilise :

une interface sombre
Mise en page dual-column CPU/GPU


Les paramètres sont stockés dans :

settings.json

Actuellement :

écran sélectionné
options utilisateur
Lancement automatique

CoreView peut être lancé automatiquement avec Windows via :

HKCU\Software\Microsoft\Windows\CurrentVersion\Run

Aucun droit administrateur requis.

Multi-écrans

CoreView peut être affiché automatiquement sur :

l’écran principal
ou un écran secondaire dédié

Le choix de l’écran est sauvegardé automatiquement.

Capture d'écran :
<img width="1891" height="1058" alt="image" src="https://github.com/user-attachments/assets/b85e8d03-d383-44b7-9b86-529f3d01108d" />



Auteur


Projet développé par VégaNova_D.
