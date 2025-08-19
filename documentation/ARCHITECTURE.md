# Architecture Modulaire — FitnessApp

## 1. MODULES MÉTIER PRINCIPAUX

### 1. `Users`

**Fonction principale** : Gérer les profils utilisateurs, l’onboarding, les préférences et les abonnements.

**Responsabilités internes** :

- Authentification / gestion des sessions
- Profils (âge, sexe, taille, poids, niveau, objectifs)
- Préférences d’entraînement (durée, lieu, matériel dispo, fréquence)
- Onboarding personnalisé
- Gestion des abonnements (freemium/premium)
- RGPD : anonymisation, export, suppression

**Entités principales** :

- `User`
- `UserProfile`
- `Subscription`
- `Preference`

**Couplage** : Fortement connecté à `Workouts`, `Coach`, `Tracking`, `Payments`

---

### 2. `Workouts`

**Fonction principale** : Gérer la structure des entraînements (workouts), leur composition et exécution.

**Responsabilités internes** :

- Création / stockage des séances
- Workouts dynamiques vs fixes
- Niveaux de difficulté, durée, matériel requis
- Logique de structuration (échauffement, effort, récupération)

**Entités principales** :

- `Workout`
- `WorkoutStep`
- `Exercise`

**Couplage** : Dépend de `Exercises`, connecté à `Coach`, `Tracking`, `Content`

---

### 3. `Exercises`

**Fonction principale** : Bibliothèque d’exercices avec toutes les métadonnées nécessaires à leur exécution.

**Responsabilités internes** :

- Données descriptives (nom, groupes musculaires, durée, calories)
- Matériel requis, variantes, consignes, erreurs courantes
- Multimédia associé (vidéo, image, texte)

**Entités principales** :

- `Exercise`
- `Tag` (e.g., "cardio", "poids du corps")
- `MediaResource`

**Couplage** : Utilisé par `Workouts`, `Content`, `Coach`

---

### 4. `Programs`

**Fonction principale** : Gérer des programmes structurés sur plusieurs semaines.

**Responsabilités internes** :

- Définition de la structure temporelle (semaines, jours)
- Objectifs du programme (perte de poids, prise de masse…)
- Progressivité / planification automatique

**Entités principales** :

- `Program`
- `ProgramPhase`
- `ScheduledWorkout`

**Couplage** : Relié à `Users`, `Coach`, `Tracking`

---

### 5. `Tracking`

**Fonction principale** : Enregistrer toutes les données liées aux séances effectuées, progrès, performances.

**Responsabilités internes** :

- Log de chaque session
- Données sur les répétitions, temps, difficulté perçue
- Historique et journal d’activité
- Évolution des metrics (poids, IMC, force…)

**Entités principales** :

- `WorkoutSession`
- `ExerciseResult`
- `UserMetric`

**Couplage** : Connecté à `Users`, `Coach`, `Programs`, `Objectives`

---

### 6. `Coach`

**Fonction principale** : Adapter automatiquement les entraînements et programmes selon le profil et les performances.

**Responsabilités internes** :

- Algorithme de suggestion / adaptation
- Progression personnalisée
- Détection de stagnation ou fatigue
- Planification intelligente (repos, intensité variable)

**Entités principales** :

- `TrainingSuggestion`
- `AdaptationRule`
- `FeedbackLoop`

**Couplage** : Très connecté à `Tracking`, `Users`, `Workouts`, `Programs`

---

### 7. `Objectives`

**Fonction principale** : Permettre à l’utilisateur de définir, suivre et adapter ses objectifs fitness.

**Responsabilités internes** :

- Définition des objectifs SMART (temps, type, quantité)
- Suivi de progression par rapport à la cible
- Notification de succès ou réajustement

**Entités principales** :

- `Goal`
- `GoalProgress`

**Couplage** : Connecté à `Tracking`, `Coach`, `Users`

---

### 8. `Notifications`

**Fonction principale** : Envoyer des rappels, encouragements, alertes.

**Responsabilités internes** :

- Rappels d'entraînement
- Motivation dynamique (basée sur activité)
- Résumés hebdomadaires

**Entités principales** :

- `Notification`
- `NotificationTrigger`

**Couplage** : Branché à `Users`, `Tracking`, `Coach`, `Objectives`

---

### 9. `Payments`

**Fonction principale** : Gérer les achats, abonnements, renouvellements, offres.

**Responsabilités internes** :

- Intégration Apple/Google/Stripe
- Facturation, historique de paiements
- Gestion du statut premium
- Codes promotionnels

**Entités principales** :

- `Transaction`
- `SubscriptionPlan`
- `Invoice`

**Couplage** : Connecté à `Users`, mais isolé du reste (via events/queues)

---

### 10. `Content`

**Fonction principale** : Gérer les vidéos, images, textes des exercices et entraînements.

**Responsabilités internes** :

- Stockage et indexation multimédia
- Transcodage vidéo
- Localisation / traduction
- Versioning des contenus

**Entités principales** :

- `MediaAsset`
- `ContentBlock`

**Couplage** : Lié à `Exercises`, `Workouts`, `Programs`

---

### 11. `Admin`

**Fonction principale** : Interface de gestion interne pour les équipes produit, support et coaching.

**Responsabilités internes** :

- CRUD utilisateurs, programmes, exercices
- Dashboard de santé système
- Gestion de bugs signalés
- Modération (si communauté)

**Entités principales** :

- `AdminUser`
- `BugReport`
- `ContentAudit`

**Couplage** : Interface sur tout le système, mais doit passer par APIs métiers (pas d’accès direct BDD)

---

### 12. `Public API / Mobile API`

**Fonction principale** : Point d’entrée unique pour le frontend mobile, API publique, ou intégrations tierces.

**Responsabilités internes** :

- Authentification via token
- Orchestration des modules internes
- Versioning
- Protection contre abus / quotas

**Couplage** : Facade pour tous les modules, mais sans logique métier propre.

---

## 2. RECOMMANDATIONS STRUCTURELLES

### Dépendances à éviter

- Ne jamais faire de dépendance directe entre `Payments` et `Workouts` (utiliser `User.IsPremium`)
- `Coach` ne doit pas écrire directement dans `Tracking` (utiliser événements)
- `Admin` ne doit pas accéder directement aux données sans passer par des APIs

---

### Complexité métier élevée

- `Coach` : adaptation dynamique, personnalisation, gestion de fatigue et progression
- `Programs` : construction cohérente sur plusieurs semaines

---

### Sensibilité à la scalabilité

- `Tracking` : pic d’usage autour de 18h–21h (à prendre en compte), sessions d’entraînement lourdes en I/O
- `Notifications` : déclenchement simultané (cron/temps réel)
- `Coach` : calculs personnalisés à optimiser (batching, pré-caching recommandé)

---

## SYNTHÈSE DES MODULES

| Module          | Rôle Principal                       |
| --------------- | ------------------------------------ |
| `Users`         | Gestion des profils et préférences   |
| `Workouts`      | Définition des séances               |
| `Exercises`     | Bibliothèque des mouvements          |
| `Programs`      | Planification sur plusieurs semaines |
| `Tracking`      | Historique et performances           |
| `Coach`         | Adaptation intelligente              |
| `Objectives`    | Objectifs personnalisés              |
| `Notifications` | Engagement et rappels                |
| `Payments`      | Monétisation et abonnements          |
| `Content`       | Média et instructions                |
| `Admin`         | Gestion interne                      |
| `Public API`    | Entrée unifiée et sécurisée          |

---
