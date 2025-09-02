# ExpenseTracker.Functions README.md

# Expense Tracker Functions

This project contains Azure Functions for the Expense Tracker application. The functions are designed to handle HTTP requests for managing expenses, utilizing Azure Cosmos DB for data storage.

## Project Structure

- **src/**: Contains the source code for the Azure Functions.
  - **ExpenseFunction.cs**: The main function that handles HTTP requests for creating, reading, updating, and deleting expenses.
  - **CosmosDbService.cs**: Service class that manages interactions with Azure Cosmos DB using Managed Service Identity (MSI).
  - **Models/**: Contains data models used in the application.
    - **Expense.cs**: Represents the structure of an expense.

- **host.json**: Configuration settings for the Azure Functions host, including logging and timeout settings.

- **local.settings.json**: Local development settings, including connection strings and application settings. This file should not be included in source control.

## Setup Instructions

1. Clone the repository to your local machine.
2. Navigate to the `ExpenseTracker.Functions` directory.
3. Ensure you have the necessary Azure resources set up, including Cosmos DB.
4. Configure the `local.settings.json` file with your Cosmos DB connection settings.
5. Run the Azure Functions locally using the Azure Functions Core Tools.

## Usage

The Azure Functions expose HTTP endpoints for managing expenses. You can use tools like Postman or curl to interact with these endpoints. The available operations include:

- **Create Expense**: POST request to create a new expense.
- **Get Expenses**: GET request to retrieve all expenses.
- **Update Expense**: PUT request to update an existing expense.
- **Delete Expense**: DELETE request to remove an expense.

Refer to the individual function implementations for specific endpoint details and request/response formats.