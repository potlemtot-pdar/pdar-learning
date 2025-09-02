# Expense Tracker Application

The Expense Tracker application is a serverless solution built using Azure Functions and Cosmos DB. It allows users to manage their expenses through a simple HTTP interface. The application leverages Managed Service Identity (MSI) for secure access to Cosmos DB.

## Project Structure

- **ExpenseTracker.Functions**: Contains the Azure Functions implementation.
  - **src**: Source code for the Azure Functions.
    - **ExpenseFunction.cs**: Handles HTTP requests for expense management.
    - **CosmosDbService.cs**: Manages interactions with Cosmos DB.
    - **Models**: Contains data models used in the application.
      - **Expense.cs**: Represents the expense data structure.
  - **host.json**: Configuration settings for the Azure Functions host.
  - **local.settings.json**: Local development settings (not included in source control).
  
- **ExpenseTracker.Tests**: Contains unit tests for the application.
  - **ExpenseFunctionTests.cs**: Tests for the `ExpenseFunction` class.
  - **CosmosDbServiceTests.cs**: Tests for the `CosmosDbService` class.
  
## Setup Instructions

1. Clone the repository to your local machine.
2. Navigate to the `ExpenseTracker.Functions` directory.
3. Set up your Azure Cosmos DB account and configure the connection settings in `local.settings.json`.
4. Deploy the Azure Functions to your Azure account.
5. Use tools like Postman or curl to interact with the HTTP endpoints for managing expenses.

## Usage

The Expense Tracker application provides endpoints for creating, reading, updating, and deleting expenses. Each expense consists of an ID, description, amount, and date. 

## Contributing

Contributions are welcome! Please submit a pull request or open an issue for any enhancements or bug fixes.

## License

This project is licensed under the MIT License.