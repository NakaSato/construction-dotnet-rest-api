# dotnet REST API

This project is a simple REST API built using .NET that manages Todo items. It provides a set of endpoints to perform CRUD (Create, Read, Update, Delete) operations on Todo items.

## Project Structure

- **Controllers**
  - `TodoController.cs`: Manages HTTP requests related to Todo items.
  
- **Models**
  - `TodoItem.cs`: Defines the data structure of a Todo item.
  
- **Services**
  - `ITodoService.cs`: Declares the contract for Todo services.
  
- **Data**
  - `TodoContext.cs`: Manages the database connection and access to Todo items.
  
- **Program.cs**: Entry point of the application, sets up the web host and middleware.
  
- **appsettings.json**: Contains configuration settings for the application.
  
- **appsettings.Development.json**: Contains development-specific configuration settings.
  
- **dotnet-rest-api.csproj**: Project file defining dependencies and build settings.

## Setup Instructions

1. **Clone the repository**:
   ```
   git clone <repository-url>
   cd dotnet-rest-api
   ```

2. **Restore dependencies**:
   ```
   dotnet restore
   ```

3. **Run the application**:
   ```
   dotnet run
   ```

4. **Access the API**:
   The API will be available at `http://localhost:5000` (or the configured port).

## Usage

- **GET /todos**: Retrieve all Todo items.
- **GET /todos/{id}**: Retrieve a specific Todo item by ID.
- **POST /todos**: Create a new Todo item.
- **PUT /todos/{id}**: Update an existing Todo item.
- **DELETE /todos/{id}**: Delete a Todo item.

## Contributing

Feel free to submit issues or pull requests for improvements or bug fixes.