# Guide de Test - Module Tracking avec Postman

Ce guide vous permet de simuler un utilisateur qui exécute son workout étape par étape avec le module Tracking.

## 🔧 Configuration Initiale

### 1. Variables Postman à configurer

```json
{
  "base_url": "https://localhost:7001/api/v1/tracking",
  "jwt_token": "VOTRE_TOKEN_JWT_ICI",
  "user_id": "550e8400-e29b-41d4-a716-446655440999",
  "workout_id": "WORKOUT_ID_DU_WORKOUT_CHOISI"
}
```

## 🏃 Scénario Complet : Utilisateur Lance son Workout

### Étape 1 : Démarrer une Session de Workout

**POST** `/sessions/start`

```json
{
  "workoutId": "BEGINNER_HIIT_WORKOUT_ID"
}
```

**Réponse attendue :** Session créée avec `status: "InProgress"` et `sessionId` généré

---

### Étape 2 : Vérifier la Session Active

**GET** `/sessions/active`

**Réponse attendue :** Détails de la session en cours

---

### Étape 3 : Exécuter les Exercices du Workout

#### 🔥 Phase Warm-Up : Jumping Jacks

**POST** `/sessions/{sessionId}/exercises`

```json
{
  "exerciseId": "550e8400-e29b-41d4-a716-446655440100",
  "metricType": "Duration",
  "value": 30,
  "repetitions": null,
  "weight": null,
  "durationSeconds": 30,
  "distance": null,
  "sets": 2,
  "notes": "Échauffement - rythme modéré pour préparer le corps"
}
```

#### 💪 Circuit Principal : Squats

**POST** `/sessions/{sessionId}/exercises`

```json
{
  "exerciseId": "550e8400-e29b-41d4-a716-446655440200",
  "metricType": "Repetitions",
  "value": 15,
  "repetitions": 15,
  "weight": null,
  "durationSeconds": null,
  "distance": null,
  "sets": 4,
  "notes": "Série 1/4 - forme parfaite maintenue"
}
```

#### 💪 Circuit Principal : Knee Push-ups

**POST** `/sessions/{sessionId}/exercises`

```json
{
  "exerciseId": "550e8400-e29b-41d4-a716-446655440201",
  "metricType": "Repetitions",
  "value": 12,
  "repetitions": 12,
  "weight": null,
  "durationSeconds": null,
  "distance": null,
  "sets": 4,
  "notes": "Série 1/4 - légère fatigue aux bras"
}
```

#### 🔄 Mise à jour Performance (l'utilisateur fait mieux que prévu)

**PUT** `/sessions/{sessionId}/exercises/550e8400-e29b-41d4-a716-446655440200`

```json
{
  "value": 18,
  "repetitions": 18,
  "weight": null,
  "durationSeconds": null,
  "distance": null,
  "sets": 4,
  "notes": "Série 2/4 - Je me sens en forme! 3 répétitions bonus"
}
```

#### 🏃 Circuit Principal : Jumping Jacks (Round 2)

**POST** `/sessions/{sessionId}/exercises`

```json
{
  "exerciseId": "550e8400-e29b-41d4-a716-446655440100",
  "metricType": "Duration",
  "value": 30,
  "repetitions": null,
  "weight": null,
  "durationSeconds": 30,
  "distance": null,
  "sets": 4,
  "notes": "Circuit principal - intensité élevée maintenue"
}
```

#### 🏔️ Circuit Principal : Mountain Climbers

**POST** `/sessions/{sessionId}/exercises`

```json
{
  "exerciseId": "550e8400-e29b-41d4-a716-446655440102",
  "metricType": "Duration",
  "value": 30,
  "repetitions": null,
  "weight": null,
  "durationSeconds": 30,
  "distance": null,
  "sets": 4,
  "notes": "Exercice le plus intense - cardio au maximum!"
}
```

#### 🧘 Phase Recovery : Quadriceps Stretch

**POST** `/sessions/{sessionId}/exercises`

```json
{
  "exerciseId": "550e8400-e29b-41d4-a716-446655440300",
  "metricType": "Duration",
  "value": 60,
  "repetitions": null,
  "weight": null,
  "durationSeconds": 60,
  "distance": null,
  "sets": 2,
  "notes": "Étirement chaque jambe - récupération active"
}
```

#### 🧘 Phase Recovery : Hamstring Stretch

**POST** `/sessions/{sessionId}/exercises`

```json
{
  "exerciseId": "550e8400-e29b-41d4-a716-446655440301",
  "metricType": "Duration",
  "value": 60,
  "repetitions": null,
  "weight": null,
  "durationSeconds": 60,
  "distance": null,
  "sets": 2,
  "notes": "Étirement final - respiration profonde"
}
```

---

### Étape 4 : Finaliser le Workout

**POST** `/sessions/{sessionId}/complete`

```json
{
  "perceivedDifficulty": "Moderate",
  "notes": "Excellent workout! Je me sens énergisé et fier d'avoir terminé le circuit complet. Prêt pour la prochaine fois!",
  "estimatedCalories": 320
}
```

---

## 📊 Scénario : Enregistrement des Métriques Utilisateur

### Pesée du matin

```json
{
  "metricType": "Weight",
  "value": 75.2,
  "recordedAt": "2025-09-22T08:00:00Z",
  "notes": "Pesée du matin à jeun - objectif -2kg d'ici fin octobre",
  "unit": "kg"
}
```

### Mesure du taux de graisse

```json
{
  "metricType": "BodyFat",
  "value": 15.8,
  "recordedAt": "2025-09-22T08:05:00Z",
  "notes": "Mesure avec balance impédancemétrique - en baisse!",
  "unit": "%"
}
```

### Record de force (bench press)

```json
{
  "metricType": "StrengthRecord",
  "value": 100,
  "recordedAt": "2025-09-22T10:30:00Z",
  "notes": "NOUVEAU PR au bench press! Enfin la barre des 100kg 🎉",
  "unit": "kg"
}
```

---

## 📅 Scénario : Planification des Workouts

### Planifier workout pour demain soir

```json
{
  "workoutId": "INTERMEDIATE_STRENGTH_WORKOUT_ID",
  "scheduledDate": "2025-09-23T18:00:00Z",
  "isFromProgram": false,
  "programId": null
}
```

### Planifier workout dans le cadre d'un programme

```json
{
  "workoutId": "EXPRESS_CARDIO_WORKOUT_ID",
  "scheduledDate": "2025-09-24T07:00:00Z",
  "isFromProgram": true,
  "programId": "CARDIO_PROGRAM_ID"
}
```

---

## 📈 Scénario : Analyse et Statistiques

### Consulter ses statistiques globales

**GET** `/stats`
_Permet de voir : workouts completed, average duration, calories burned, current streak..._

### Analyser sa progression sur un exercice

**GET** `/exercises/550e8400-e29b-41d4-a716-446655440200/performance`
_Historique des performances sur les squats_

### Fréquence d'entraînement du mois

**GET** `/frequency?startDate=2025-09-01&endDate=2025-09-30`
_Analyse de la régularité d'entraînement_

---

## 🚨 Scénarios d'Exception

### Abandonner un workout en cours

**POST** `/sessions/{sessionId}/abandon`

```json
"Douleur au genou - je préfère arrêter par précaution"
```

### Reprogrammer un workout

**PUT** `/planned-workouts/{plannedWorkoutId}/reschedule`

```json
{
  "newScheduledDate": "2025-09-24T19:00:00Z"
}
```

### Supprimer une métrique erronée

**DELETE** `/metrics/{metricId}`

---

## 💡 Tips pour les Tests

1. **Ordre recommandé** : Suivez les étapes dans l'ordre pour un test réaliste
2. **Variables Postman** : Utilisez les variables pour éviter de copier-coller les IDs
3. **Tests automatisés** : Les scripts de test Postman sauvegardent automatiquement les IDs de session
4. **Données réalistes** : Utilisez des valeurs cohérentes (poids, répétitions, durées)
5. **Notes descriptives** : Ajoutez des notes pour rendre les tests plus vivants

## 🎯 Cas d'Usage Avancés

### Workout avec différents types d'exercices

- **Cardio** : Duration + estimated calories
- **Strength** : Reps + Sets + Weight (optionnel)
- **Flexibility** : Duration + notes sur ressenti

### Progression tracking

- Comparer les performances entre sessions
- Analyser l'évolution des métriques corporelles
- Suivre la régularité d'entraînement

### Gestion des programmes

- Workouts planifiés vs workouts spontanés
- Suivi de l'adhérence au programme
- Analyses de performance à long terme
