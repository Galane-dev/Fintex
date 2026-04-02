# CI/CD Setup

This repository now uses:

- GitHub Actions pipeline for backend CI/CD to Azure App Service
- Azure DevOps YAML pipeline (optional/legacy path kept for reference)
- Vercel native Git integration for the frontend

## Backend to Azure with GitHub Actions

Workflow file:

- `.github/workflows/backend-azure-appservice.yml`

### What the GitHub Actions backend pipeline does

1. Triggers on backend changes to `master` and `dev` (and supports manual runs)
2. Runs restore + build + publish for `Fintex.Web.Host`
3. Deploys to Azure App Service on pushes to `master` and `dev` (including merges)
4. Runs build validation on pull requests to `master` and `dev` without deploying

### GitHub Secrets required

Add these in GitHub repository settings:

- `AZURE_WEBAPP_PUBLISH_PROFILE`
  - Value is the full XML from Azure App Service "Get publish profile"

Recommended GitHub variable:

- `AZURE_WEBAPP_NAME`
  - Example: `fintex`
  - Add under `Settings -> Secrets and variables -> Actions -> Variables`

Alternative (if you prefer): store `AZURE_WEBAPP_NAME` as a secret instead.

### Important: runtime app settings are not read from GitHub deploy secrets

Deployment secrets only authenticate/publish the app. Your backend runtime configuration should be set in Azure App Service `Environment variables`.

Use these Azure App Service entries (from current Fintex settings):

- `ConnectionStrings__Default`
- `Authentication__JwtBearer__SecurityKey`
- `OpenAI__ApiKey`
- `Notifications__Email__Username`
- `Notifications__Email__Password`
- `MarketData__Oanda__AccountId`
- `MarketData__Oanda__ApiToken`
- `MarketData__Oanda__Instruments`
- `App__SelfUrl`
- `App__ServerRootAddress`
- `App__ClientRootAddress`
- `App__CorsOrigins`
### How to get the publish profile

1. Azure Portal -> App Service (`fintex`)
2. Select `Overview`
3. Click `Get publish profile`
4. Copy XML contents and paste into GitHub secret `AZURE_WEBAPP_PUBLISH_PROFILE`

### Trigger behavior

- Merge into `master` or `dev` -> automatic deploy to Azure App Service
- PR to `master` or `dev` -> CI build only

## Backend to Azure with Azure DevOps

Pipeline file:

- `azure-pipelines.yml`

### What the backend pipeline does

1. Triggers on backend changes to `master` or `main`
2. Restores, builds, and tests the ASP.NET Core solution
3. Builds a Docker image from `Backend/aspnet-core/src/Fintex.Web.Host/Dockerfile`
4. Pushes the image to Azure Container Registry
5. Updates the Azure Web App for Containers to the new image tag

### Azure DevOps service connections you need

- Azure Resource Manager service connection
  - example variable value: `fintex-azure-rm-sc`
- Azure Container Registry service connection
  - example variable value: `fintex-acr-sc`

### Azure DevOps pipeline variables you need

- `azureServiceConnection`
- `acrServiceConnection`
- `acrLoginServer`
- `webAppName`

You can set these in the pipeline UI or a variable group. They do not need to be committed to the repository.

### Azure requirements

Before the pipeline can deploy successfully:

1. Create or reuse an Azure Container Registry.
2. Create or reuse an Azure Web App for Containers.
3. Ensure the Web App can pull from your ACR image, ideally with managed identity or preconfigured registry access.
4. Add the backend runtime settings in Azure App Service `Environment variables`.

## Frontend to Vercel

Use Vercel native Git deployment instead of GitHub Actions.

### Recommended setup

1. Import the GitHub repository into Vercel
2. Set the project root to `Frontend/nextjs`
3. Add `NEXT_PUBLIC_API_BASE_URL`
4. Let Vercel deploy automatically on pushes to your chosen production branch

## Why this setup works well

- Azure DevOps gives you a real YAML CI/CD pipeline to present
- Vercel gives you simple, reliable frontend auto-deploys without GitHub Actions billing
- Docker is still part of the backend delivery path, so your deployment remains containerized
