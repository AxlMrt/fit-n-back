# FitnessApp API Documentation

## Overview
Cette documentation couvre les APIs complètes pour l'application FitnessApp, incluant l'authentification, la gestion des profils utilisateur et les abonnements.

## Architecture
L'API suit une architecture modulaire avec séparation claire des responsabilités :

- **Authentication Module** : Gestion complète de l'authentification (login, register, 2FA, etc.)
- **Users Module** : Gestion des profils utilisateur et abonnements
- **Authorization Module** : Contrôle d'accès et permissions

## Base URL
- **Development** : `https://localhost:7001`
- **Production** : `https://api.fitnessapp.com` (à configurer)

## Authentication
Toutes les APIs protégées utilisent l'authentification Bearer Token JWT.

### Headers requis
```
Authorization: Bearer {access_token}
Content-Type: application/json
```

## API Endpoints

### 🔐 Authentication (`/api/v1/auth`)

#### Public Endpoints
- `POST /register` - Créer un nouveau compte
- `POST /login` - Se connecter
- `POST /refresh` - Rafraîchir le token
- `POST /forgot-password` - Demander un reset de mot de passe
- `POST /reset-password` - Réinitialiser le mot de passe
- `POST /confirm-email` - Confirmer l'email
- `POST /resend-email-confirmation` - Renvoyer la confirmation email
- `GET /check-username` - Vérifier la disponibilité d'un username
- `GET /check-email` - Vérifier la disponibilité d'un email

#### Protected Endpoints
- `POST /logout` - Se déconnecter
- `PUT /change-password` - Changer le mot de passe
- `PUT /update-email` - Mettre à jour l'email
- `PUT /update-username` - Mettre à jour le username
- `POST /enable-2fa` - Activer l'authentification à deux facteurs
- `POST /disable-2fa` - Désactiver l'authentification à deux facteurs
- `POST /deactivate-account` - Désactiver le compte
- `GET /validate-token` - Valider un token

### 👤 User Profiles (`/api/v1/profiles`)

#### Public Endpoints
- `GET /` - Lister les profils publics (avec filtres)
- `GET /{userId}` - Obtenir un profil public par ID utilisateur

#### Protected Endpoints
- `GET /me` - Obtenir mon profil
- `POST /` - Créer un profil
- `PUT /` - Mettre à jour mon profil
- `PUT /preferences` - Mettre à jour mes préférences
- `GET /completion-status` - Statut de complétion du profil
- `GET /exists` - Vérifier si un profil existe

#### Admin Only
- `GET /statistics` - Statistiques des profils

### 💳 Subscriptions (`/api/v1/subscriptions`)

#### Protected Endpoints
- `GET /current` - Obtenir mon abonnement actuel
- `POST /` - Créer un nouvel abonnement
- `PUT /` - Mettre à jour mon abonnement
- `DELETE /` - Annuler mon abonnement
- `GET /status` - Statut de mon abonnement

## Data Models

### User Registration
```json
{
  "userName": "string",
  "email": "string",
  "password": "string",
  "firstName": "string",
  "lastName": "string"
}
```

### User Login
```json
{
  "email": "string",
  "password": "string"
}
```

### User Profile
```json
{
  "dateOfBirth": "2024-01-01T00:00:00Z",
  "gender": 1,
  "height": 175.5,
  "weight": 70.0,
  "activityLevel": 2,
  "fitnessGoals": [1, 2],
  "bio": "string",
  "isPublic": true,
  "preferences": {
    "language": "en",
    "timezone": "UTC",
    "measurementSystem": 0,
    "notificationsEnabled": true,
    "privateProfile": false
  }
}
```

### Subscription
```json
{
  "level": 1,
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z"
}
```

## Enums

### Gender
- `0` - NotSpecified
- `1` - Male  
- `2` - Female
- `3` - Other

### Activity Level
- `0` - Sedentary
- `1` - LightlyActive
- `2` - ModeratelyActive
- `3` - VeryActive
- `4` - ExtraActive

### Fitness Goals
- `0` - WeightLoss
- `1` - MuscleGain
- `2` - Maintenance
- `3` - Endurance
- `4` - Strength
- `5` - Flexibility

### Subscription Level
- `0` - Free
- `1` - Premium
- `2` - Pro

### Measurement System
- `0` - Metric
- `1` - Imperial

## Error Handling

### Standard Error Response
```json
{
  "message": "Error description",
  "errors": ["Detailed error 1", "Detailed error 2"]
}
```

### HTTP Status Codes
- `200` - OK
- `201` - Created
- `400` - Bad Request
- `401` - Unauthorized
- `403` - Forbidden
- `404` - Not Found
- `409` - Conflict
- `500` - Internal Server Error

## Testing avec Postman

### Installation
1. Importez la collection : `FitnessApp_API_Collection.postman_collection.json`
2. Importez l'environnement : `FitnessApp_Development.postman_environment.json`

### Workflow de test recommandé
1. **Register** - Créer un nouveau compte
2. **Login** - Se connecter (sauvegarde automatique du token)
3. **Create Profile** - Créer un profil utilisateur
4. **Get My Profile** - Vérifier le profil créé
5. **Create Subscription** - Créer un abonnement
6. **Get Current Subscription** - Vérifier l'abonnement

### Variables d'environnement
- `base_url` - URL de base de l'API
- `auth_token` - Token d'authentification (auto-rempli lors du login)
- `refresh_token` - Token de rafraîchissement (auto-rempli lors du login)
- `user_id` - ID utilisateur (auto-rempli lors du login)

## Sécurité

### Best Practices
- Utilisez HTTPS en production
- Les tokens JWT expirent après un temps déterminé
- Utilisez le refresh token pour renouveler l'accès
- Implémentez la limitation de taux (rate limiting)
- Validez toutes les entrées utilisateur
- Utilisez des mots de passe forts
- Activez la 2FA pour plus de sécurité

### Headers de sécurité
```
Authorization: Bearer {token}
Content-Type: application/json
X-API-Version: v1
```

## Support
Pour toute question ou problème :
1. Vérifiez les logs de l'application
2. Consultez la documentation Swagger à `/swagger`
3. Vérifiez que tous les services sont démarrés
4. Assurez-vous que la base de données est accessible

## Changelog
- **v1.0** - Version initiale avec authentification, profils et abonnements
