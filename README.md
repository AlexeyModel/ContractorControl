---

**📄 Central Documentation File: README.md**

Contract Execution Registration Service for Event-Driven Orchestration of Microservices Based on the DAG (Directed Acyclic Graph) Model (p. 1).

---

**🚀 1. Launch Instructions and Configuration Parameters**

The application uses HTTP port **6767** by default (AppSettings:Port parameter in the appsettings.json file) (pp. 8, 11). Before starting, a running PostgreSQL 14+ server with the database (default contractordb) is required (p. 8).

## **🐧 Launch on Linux**

1. Clone the repository:  
   git clone https://github.com && cd ContractorControl (p. 8)  
2. Configure the connection string in ContractorControl/appsettings.json (specify your password and Search Path=contractor_control) (p. 8).  
3. Build and run the project:  
   cd ContractorControl && dotnet restore && dotnet run (p. 8)  
4. The service and Swagger UI will be available at: http://localhost:6767 and http://localhost:6767/swagger (p. 9).

## **🪟 Launch on Windows**

1. Clone the repository and open the solution file ContractorControl.sln in Visual Studio 2022+ (p. 9).  
2. Configure the connection string in ContractorControl/appsettings.json (p. 9).  
3. Select ContractorControl as the startup project and press **F5** (or **Ctrl+F5**) (p. 9).  
4. Alternative launch via developer console: dotnet restore -> dotnet run (p. 9).

## **🐳 Launch in Docker**

For deployment, a combination of the .NET 10 application and PostgreSQL DBMS is used (pp. 6, 9). Building and running are done with a single command from the root directory of the solution (pp. 9, 11):

docker-compose up --build

*Note: When configuring via Docker environment variables/developers, double underscores must be used instead of colons for key hierarchy (e.g., ConnectionStrings__DefaultConnection) (p. 11).*

---

**📊 2. Database Structure (Schema contractor_control)**

All tables are automatically initialized in the contractor_control schema on first launch (pp. 11-12). The system implements a soft-delete pattern at the ORM level through the IAuditable interface (columns create_at, update_at, delete_at) (pp. 7, 12).

## **Main Tables and Relationships (pp. 12-16):**

* **contracts** — registry of business processes (contract templates). Key fields: id (SERIAL, PK), code (VARCHAR, unique process code) (p. 12).  
* **state_type** — directory of state types (p. 12). Contains 4 system hardcoded types (Seed data):  
  1. GLOBAL (system status, set without graph validation) (pp. 13, 17)  
  2. RUN (process execution stage) (p. 13)  
  3. SUCCESS (successful completion of the stage) (p. 13)  
  4. FAILED (error at the execution stage) (p. 13)  
* **states** — DAG graph nodes linked to a specific contract (p. 13). System statuses FINISHED (id=1), FAILED (id=2) and QUEUE (id=3) are reserved and linked to a dummy contract with contract_id = 0 (p. 14).  
* **dags** — graph edges defining transitions (links) between states (p. 14). Fields: state_source_id (outgoing) and state_destination_id (incoming) (p. 14).  
* **contract_execution** — instances of contract executions (p. 14). Stores an external unique identifier instance_contract_execution (UUID/GUID) for integration with external microservices (p. 15).  
* **events** — immutable log (journal) of achieved states (p. 15). Unique index (contract_execution_id, state_id, delete_at) prevents re-recording of the same state for a specific instance (p. 15).

**Relationships (Entity Relationships):**  
contracts (1) ----------- (*) states (*) ----------- (1) state_type

   |                            |  
   |                            +--- (*) dags (*) --- (1) states (self-reference)  
   |  
(1) ----------- (*) contract_execution (1) ----------- (*) events (*) ----------- (1) states

---

**📂 3. Project File Structure**

The project is developed in accordance with the principles of Clean Architecture and is divided into two main executable entities within a single .sln file (p. 18):

```
ContractorControl.sln
├── ContractorControl/                # Main web application (ASP.NET Core API)
│   ├── Api/                          # Controllers (ContractExecution, States, Crud, Init) and Middleware
│   ├── Application/                  # Business logic layer: CQRS (Commands, Queries, Handlers, FluentValidators)
│   │                                   and embedded SQL scripts for stored procedures
│   ├── Domain/                       # DB entities (Contract, State, Event, Dag), common interfaces (IAuditable)
│   └── Infrastructure/               # EF Core settings, ApplicationDbContext, interceptors (Interceptors)
│                                       and initialization services (InitService, CrudService)
└── GenerationCC/                     # Console application for generating graph connections (DAG)
    ├── Program.cs                    # Entry point for parser and CLI logic
    └── GenerationCC.csproj           # Configuration file for the generator project
```

---

**⚙️ 4. API Business Logic and Event Architecture**

**ContractorControl** acts as a **passive event registrar** (pp. 6, 28). It does not actively manage token transit, but only verifies the correctness of task execution facts sent by microservices (pp. 6, 28).

## **Key API Endpoints (pp. 22-23):**

1. **POST api/ContractExecution/create** — creation of a new contract execution instance by its code, returns an external GUID (instance_contract_execution) (pp. 15, 22).  
2. **POST api/States/setStates** — registration of an event (transition to a new state) (p. 22). Includes the DAG validation algorithm (p. 22).  
3. **POST api/States/checkStateToSet** — preliminary check: is it allowed to set this state now (p. 22).  
4. **POST api/ContractExecution/checkIsFinished** — check for completion of the entire contract (p. 22).  
5. **POST api/crud/[insert|update|delete]** — universal dynamic CRUD service operating through reflection for metadata management (requires SecretKey transmission) (pp. 23, 25).

## **Logic for Working with States and Global Statuses:**

* **Transition Validation**: When attempting to set a state of type RUN, SUCCESS, or FAILED, the system searches for all incoming edges in the dags table (pp. 13, 24). The transition is allowed only when **all** parent nodes (predecessors) already have confirming records in the events table (p. 24).  
* **Global Statuses**: If the state type is defined as GLOBAL (e.g., FINISHED, FAILED, QUEUE), graph validation is completely ignored, allowing recording of a system failure or forced process termination at any time (pp. 7, 27).  
* **Global Context**: To ensure maximum performance, validation is moved to the PostgreSQL DBMS side in the form of two optimized stored procedures (check_state_to_set and check_is_finished), called through the CQRS layer (MediatR library) (pp. 7, 16, 23).

---

**🔀 5. Architectural Differences from Camunda BPM Class Systems**

The project offers an alternative approach to automating inter-service interaction (p. 29).

| Comparison Criterion                  | Camunda (BPMN Approach)                                      | ContractorControl (DAG Approach)                                      |
| :------------------------------------ | :----------------------------------------------------------- | :-------------------------------------------------------------------- |
| **Role in Architecture**              | **Active Orchestrator**. Independently moves the process, manages tokens, calls external services (pp. 29-30). | **Passive Registrar**. Accepts facts from autonomous microservices and validates their order (pp. 6, 29-30). |
| **Mathematical Model**                | BPMN 2.0. Allows cycles, gateways, complex conditional branches, and subprocesses (pp. 29-30). | **DAG (Directed Acyclic Graph)**. Cycles are impossible at the schema level (pp. 6, 26, 30). |
| **Execution Guarantees**              | Requires configuration of compensations, timeouts, and error handlers at the engine level (pp. 29-30). | **Determinism**. The absence of cycles mathematically guarantees process completability (pp. 26, 29). |
| **Complexity and Speed**              | High resource intensity, complex UI (Cockpit, Modeler), long learning curve (pp. 29-30). | Lightweight, high performance due to checks at the PostgreSQL function level (p. 30). |

**When to Choose ContractorControl:** The project is ideal for systems whose business logic can be described as a strict sequence or parallel chains of steps without going back (Data Pipelines, approval chains, distributed transactions in EDA) (pp. 26, 29, 31).

---

**🛠️ 6. Second Application: Graph Generator Module (GenerationCC)**

GenerationCC is a console utility on .NET 10 designed to automate filling the dags connection table based on text configurations (pp. 7, 32). It works directly through the Npgsql driver within a single transaction to ensure atomicity (pp. 32, 34).

## **Operating Principle:**

The developer describes the transition tree structure in a text file, using **indents (spaces)** to indicate the hierarchy of parent and child nodes (p. 32). The generator parses the file line by line using a stack algorithm, matches names with the states directory in the DB, and links them with edges (pp. 32-33). If some state is not in the DB, the utility writes a log to a file in the format errors_YYYY-MM-DD.log (p. 34).

## **CLI Invocation Command:**

```bash
dotnet run --project GenerationCC -- "<ConnectionString>" <ContractId> "<FilePath>"
```

*Example:* 
```bash
dotnet run --project GenerationCC -- "Host=localhost;Database=contractordb;Username=postgres;Password=secret" 42 "my_dag.txt" (p. 34)
```

## **Example Input File (my_dag.txt) (p. 33):**

```
OrderReceived-GLOBAL
  CheckInventory-RUN
    InventorySuccess-SUCCESS
    InventoryFailed-FAILED
  ReserveItems-RUN
    ReserveSuccess-SUCCESS
  ProcessPayment-RUN
    PaymentSuccess-SUCCESS
    PaymentFailed-FAILED
  ShipOrder-RUN
    ShippingConfirmed-SUCCESS
OrderCompleted-GLOBAL
```

This file will automatically generate the following directed edges in the dags table (p. 33):

* OrderReceived -> CheckInventory, ReserveItems, ProcessPayment, ShipOrder (p. 33)  
* CheckInventory -> InventorySuccess, InventoryFailed (p. 33)  
* ReserveItems -> ReserveSuccess (p. 33)  
* ProcessPayment -> PaymentSuccess, PaymentFailed (p. 33)  
* ShipOrder -> ShippingConfirmed (p. 33)

---

**Next Steps**

What will be our next step? I can prepare detailed **SQL migration scripts** for manual population of directories or provide examples of **JSON payloads for integrating** your microservices with the setStates endpoints.

# ContractorControl API Integration Payloads

This document provides complete JSON payload examples for integrating your distributed microservices with the **ContractorControl** API endpoints.

---

## 1. Create a Contract Execution Instance
Called at the very beginning of a business process (e.g., by an API Gateway or Order Service) to initialize the flow and obtain a unique tracking GUID.

*   **Method**: `POST`
*   **Endpoint**: `api/ContractExecution/create`
*   **Input DTO**: `ContractExecutionCreateDto`

### Request Body
```json
{
  "Code": "order_processing"
}
```

### Expected Response (`InstanceDto`)
```json
{
  "status": 200,
  "message": "Execution instance created successfully.",
  "data": {
    "InstanceContractExecution": "e1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"
  }
}
```
> **Note:** This generated GUID must be passed down to all subsequent microservices via event broker headers or request payloads to keep track of the process execution.

---

## 2. Register an Event (State Transition)
Called by each microservice as soon as it finishes its specific task. The system runs the DAG validation algorithm to ensure all preceding steps have been completed before appending this event to the journal.

*   **Method**: `POST`
*   **Endpoint**: `api/States/setStates`
*   **Input DTO**: `SetStateDto`

### Example A: Registering a standard processing stage (e.g., Inventory Service)
```json
{
  "InstanceContractExecution": "e1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d",
  "StateName": "CheckInventory",
  "StateType": "RUN"
}
```

### Example B: Registering a successful state completion (e.g., Payment Service)
```json
{
  "InstanceContractExecution": "e1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d",
  "StateName": "PaymentSuccess",
  "StateType": "SUCCESS"
}
```

---

## 3. Pre-check: Can a State Be Set? (Predicate Check)
Used when a microservice wants to check with ContractorControl whether its prerequisites in the DAG graph are met before starting heavy background or database processing.

*   **Method**: `POST`
*   **Endpoint**: `api/States/checkStateToSet`
*   **Input DTO**: `CheckStateDto`

### Request Body
```json
{
  "InstanceContractExecution": "e1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d",
  "StateName": "ShipOrder",
  "StateType": "RUN"
}
```

### Expected Response (`ApiResponse`)
```json
{
  "status": 200,
  "message": "Success",
  "data": true
}
```

---

## 4. Verification: Is the Contract Finished?
Used by polling clients, monitoring dashboards, or notification services to verify if the contract execution instance has successfully reached the `FINISHED` global milestone.

*   **Method**: `POST`
*   **Endpoint**: `api/ContractExecution/checkIsFinished`
*   **Input DTO**: `InstanceDto`

### Request Body
```json
{
  "InstanceContractExecution": "e1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"
}
```

---

## 5. Administrative Configuration (Generic Reflection-based CRUD)
Payloads used by administrators to modify process topology dynamically via the `CrudController`. This maps properties directly to database columns and requires a secure application key.

*   **Method**: `POST`
*   **Endpoints**: `api/crud/insert`, `api/crud/update`, `api/crud/delete`
*   **Input Container Model**: `SetPropertyInfo`

### Example A: Creating a new transition dependency (DAG Edge) between two states
```json
{
  "TableName": "dags",
  "SecretKey": "SuperSecretKey123",
  "Data": {
    "state_source_id": 12,
    "state_destination_id": 15
  }
}
```

### Example B: Adding a brand new contract template code to the registry
```json
{
  "TableName": "contracts",
  "SecretKey": "SuperSecretKey123",
  "Data": {
    "code": "vendor_delivery"
  }
}
```
