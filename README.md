# Tech Interview Arkitech

## Description

This is a mock project that uses some of the technologies used in Arkitech, but the main goal is to test your skills in the technologies you are familiar with.

The project contains a Python simulation script that publishes data to a MQTT broker, it exposes a CSharp backend that consumes the data and stores it in a MongoDB database, and an Angular frontend that displays the data in a dashboard.

### Requirements: Tooling setup

Install the following tools to run this project for the interview:

#### Backend
- [Install Docker](https://docs.docker.com/get-docker/)
- [Install .NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

#### Frontend
- [Install Node Version Manager - NVM to manage multiple versions of NodeJS](https://www.freecodecamp.org/news/node-version-manager-nvm-install-guide/)

## Backend API setup
C# Project Path: `app`

### Instructions
How to run the Backend API in your local machine:

#### 1. Setup Docker
Go to the project folder and run docker build. This will create a MongoDB container and a simulator script publishing data to MQTT broker:

```bash
docker-compose up
```

#### 2. Install Dotnet 9
If you don't have .NET 9 installed, you can install it by following the instructions in the [official documentation](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).
```bash
dotnet --version
```

#### 3. Run the C# API
Go to the backend folder `app` in the terminal and run the following commands to run the C# API:

```bash
dotnet run
// or
dotnet watch run
```

After running the command, you can go to `http://localhost:8000/swagger/index.html` to see the API documentation (Swagger UI)
   
--- 

## Frontend setup
Path: `web-app/arkitech-dashboard`

Requirements:
- NodeJS version 20 or higher (you can use NVM to manage multiple versions of NodeJS)
  
### Instructions

#### Install NPM packages and run the Angular project
Go to the frontend folder `web-app/arkitech-dashboard` in the terminal and run the following commands to run the Angular project:

Install the NPM packages and run the Angular project:
```bash
npm install
ng dev
```

Open your browser and go to `http://localhost:4200/`

----

## Questions

- The day of the test we will share the tasks with you.
- If you have any questions, please feel free to reach out to us. We are happy to help you with any questions you may have setting up the project.

---