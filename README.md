# Recipedia

Recipedia is a modern recipe web application that lets you search, save, and generate recipes. 

## Live Site
Check it out here if the site is currently up: [Recipedia](https://recipedia.cc) 

## Run Locally
If the website is no longer available, you can run it locally on your machine using Docker. 
Before running locally, make sure you have:

- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- API keys for (all free):
  - Gemini API
  - [Google Custom Search (CSE)](https://programmablesearchengine.google.com/controlpanel/create)
  - [Spoonacular](https://spoonacular.com/food-api)
  - Google OAuth + A local SQL Server Instance (Optional, only if you want to test login functionality locally)
---
## Setup Instructions
### 1) Clone the Repository
```bash
git clone https://github.com/YOUR_USERNAME/recipedia.git
cd recipedia

### 2) Open the compose.yaml file and input your api key values.
```yaml
services:
  recipedia:
    build: .
    container_name: recipedia
    ports:
      - "8080:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - Authentication__Google__ClientId=${GOOGLE_CLIENT_ID}
      - Authentication__Google__ClientSecret=${GOOGLE_CLIENT_SECRET}
      - Gemini__ApiKey=${GEMINI_API_KEY}
      - Gemini__BaseUrl=${GEMINI_BASE_URL}
      - GoogleCSE__ApiKey=${GOOGLE_CSE_API_KEY}
      - GoogleCSE__CseId=${GOOGLE_CSE_ID}
      - Spoonacular__ApiKey=${SPOONACULAR_API_KEY}
    volumes:
      - recipedia-keys:/root/.aspnet/DataProtection-Keys
      - recipedia-data:/app/data

volumes:
  recipedia-keys:
  recipedia-data:

### (Optional) 3) Setup the local database by changing the default connection string.
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=RecipediaDB;Trusted_Connection=True;TrustServerCertificate=True;"
}

### 4) Run the app
```bash
docker compose up --build

### 5) Open the browser and visit http://localhost:8080.

## Features
- **Recipe Search** – Find recipes online using Google Custom Search Engine (CSE).  
- **AI-Generated Recipes** – Generate unique recipes using Gemini AI.  
- **User Authentication** – Sign in securely with Google.  
- **Save Favorites** – Keep your favorite recipes in one place.  
- **Responsive Design** – Works on both desktop and mobile devices.  
- **Cloud Deployment** – Hosted on AWS and containerized with Docker.  

## Tech Stack
- **Backend:** ASP.NET Core
- **Frontend:** HTML, CSS, JavaScript  
- **AI Integration:** Gemini AI
- **Database:** SQL (SQL Server Management Studio)
- **External APIs:** Google Custom Search Engine (CSE), Spoonacular, Google OAuth  
- **Deployment:** Docker, AWS

## License
This project is licensed under the MIT License.  
