# Soda SDK

## Project Overview

A Unity SDK test case showcasing remote configuration management with native Android plugin architecture and a user-friendly editor interface supporting real-time configuration overrides for efficient development and testing.

### Key Features
- Remote Config
- Native Android Integration 
- Unity Editor Override Support
- Simple A/B testing mechanism
- Automatic fallback to default values on network issues
- Easy installation via Unity Package Manager
- Frontend Dashboard

---

## Tech Stack

### Backend Architecture
- **Java Spring Boot**
- **H2 Database**
- **Docker**
- **AWS ECS/ECR**

**API Endpoint:** `http://sodabackend.emircagan.com/api/`
**Dashboard** 'https://soda-sdk-dashboard.pages.dev'
---

## How Remote Configuration Works

<img width="464" height="959" alt="image" src="https://github.com/user-attachments/assets/7ae4c32c-a06b-4e69-9459-69b944b79502" />

The system follows:

1. **Unity Application** requests configuration from the backend server
2. **Backend API** processes the request and applies A/B testing logic if configured
3. **Configuration Response** is returned with appropriate values based on user segmentation
4. **Unity SDK** caches the response and provides type-safe access methods
5. **Fallback Mechanism** ensures the app continues functioning even during network issues

---

## Installation & Setup

### Unity Package Installation
1. Open Unity Package Manager (`Window > Package Manager`)
2. Select **"Add package from git URL"**
3. Enter the repository URL: `https://github.com/your-repo/soda-unity-sdk.git`
4. Click **Add** to install the package

### Initial Setup
1. Navigate to `SodaSDK > Settings` from the Unity toolbar
2. Create a new **Soda Settings** asset when prompted
3. Configure your server endpoint and bundle identifier
4. Place the **SodaInitializer** prefab in your first scene

---

## Unity Integration Guide

### Configuration Setup

<img width="623" height="810" alt="image" src="https://github.com/user-attachments/assets/4b9018b9-77b4-4dfa-b856-74bc5034040b" />

Access the Soda SDK settings through the Unity toolbar to configure your remote configuration parameters.

<img width="625" height="835" alt="image" src="https://github.com/user-attachments/assets/10e06f42-21ff-422d-affe-c9989a9cc0a0" />


**Essential Configuration Fields:**
- **Server URL** - Your backend API endpoint (`http://sodabackend.emircagan.com/api`)
- **Bundle ID** - Your application identifier (e.g., `com.company.gamename`)
- **Config Name** - The configuration set to fetch (e.g., `game_config`)
- **Override Mode** - Enable local testing with custom values

### Code Integration

#### Basic Usage
```csharp
// Initialize the SDK (automatically handled by SodaInitializer prefab)
// Wait for initialization to complete before accessing configs

// Fetch different data types with fallback values
string carColor = SodaSDK.RemoteConfig.GetString("carColor", "red");
int carSpeed = SodaSDK.RemoteConfig.GetInt("carSpeed", 100);
```

#### Advanced Usage with Callbacks
```csharp
// Listen for config updates
SodaSDK.OnInitialize
SodaSDK.OnInitializationFailed
SodaSDK.RemoteConfig.OnConfigLoaded
SodaSDK.RemoteConfig.OnError
```

#### Editor Override System
During development, you can override remote values locally:
1. Add key-value pairs in the override section
2. Add to settings
3. Enable **Override Mode** in Soda Settings
4. Test different configurations without server changes

---

## Backend API Reference

### Base Configuration
- **Base URL:** `http://sodabackend.emircagan.com/api`
- **Content-Type:** `application/json`
- **Optional Header:** `X-Device-ID` for consistent A/B testing

### Game Management Endpoints

#### Create Game
```bash
POST /games
Content-Type: application/json

{
  "bundleId": "com.hypermonk.racing"
}
```

#### List All Games
```bash
GET /games
```

#### Get Specific Game
```bash
GET /games/{bundleId}
```

### Configuration Management

#### Create Configuration
```bash
POST /games/{bundleId}/configs
Content-Type: application/json
X-Device-ID: user123 (optional)

{
  "configName": "car_config_A",
  "configJson": "{\"carColor\":\"blue\",\"carSpeed\":150}",
  "weight": 70
}
```

#### Fetch Configuration (A/B Testing Enabled)
```bash
GET /games/{bundleId}/configs/{configName}
X-Device-ID: user123 (optional for consistency)
```

**Response:**
```json
{
  "success": true,
  "bundleId": "com.hypermonk.racing",
  "configName": "car_config",
  "config": {
    "carColor": "blue",
    "carSpeed": 150,
    "engineSoundVolume": 0.8
  }
}
```

#### List All Configurations
```bash
GET /games/{bundleId}/configs
```

**Response:**
```json
{
  "bundleId": "com.hypermonk.racing",
  "configs": [
    {
      "id": 1,
      "configName": "car_config_A",
      "configJson": "{\"carColor\":\"blue\",\"carSpeed\":150}",
      "weight": 70,
      "isActive": true,
      "createdAt": "2025-01-09T10:30:00"
    }
  ]
}
```

#### Update Configuration
```bash
PUT /games/{bundleId}/configs/{configName}
Content-Type: application/json
X-Device-ID: user123 (optional)

{
  "configJson": "{\"carColor\":\"red\",\"carSpeed\":120}",
  "weight": 50,
  "isActive": true
}
```

#### Delete Configuration
```bash
DELETE /games/{bundleId}/configs/{configName}
X-Device-ID: user123 (optional)
```

**Success Response:**
```json
{
  "message": "Config deleted successfully"
}
```

#### Delete Game (and all its configurations)
```bash
DELETE /games/{bundleId}
```

**Success Response:**
```json
{
  "message": "Game deleted successfully"
}
```

### Health Check
```bash
GET /health
```

---

## A/B Testing Guide

### Concept Overview
The Soda SDK implements **weight-based A/B testing**

### Setting Up A/B Tests

1. **Create Variant Configurations**
   ```bash
   # Version A (70% of users)
   POST /games/com.game.racing/configs
   {
     "configName": "car_config_A",
     "configJson": "{\"carColor\":\"red\",\"carSpeed\":100}",
     "weight": 70
   }
   
   # Version B (30% of users) 
   POST /games/com.game.racing/configs
   {
     "configName": "car_config_B", 
     "configJson": "{\"carColor\":\"blue\",\"carSpeed\":150}",
     "weight": 30
   }
   ```

2. **Fetch with Base Name**
   ```bash
   GET /games/com.game.racing/configs/car_config
   X-Device-ID: user123
   ```

3. **Results**
   - 70% of users receive red car with speed 100
   - 30% of users receive blue car with speed 150
   - Same device ID always gets the same configuration
   - Distribution is automatically handled by weight values

### Testing Different Scenarios
```csharp
// In Unity - same code works for all variants
string carColor = SodaSDK.RemoteConfig.GetColor("carColor", Color.red);
int carSpeed = SodaSDK.RemoteConfig.GetInt("carSpeed", 100);

// The SDK automatically receives the appropriate variant
```

---

## Best Practices

```csharp
// Always provide sensible default values
string carColor = SodaSDK.RemoteConfig.GetColor("carColor", Color.red);
