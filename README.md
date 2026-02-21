# POC-AIPoweredFAQAPI

Minimal AI-powered FAQ API (Proof of Concept). This service accepts FAQ data, creates embeddings, stores them in a small knowledge base (SQLite or Postgres), and answers user questions using a small RAG flow + OpenAI.

1) Flow
- Client POSTs a question to `/api/faq/ask`.
- The service embeds the question via the configured embedding client.
- The `IKnowledgeBaseRepository` is queried for nearest FAQ entries (SQLite computes cosine in memory; Postgres expects a `vector` type and uses <-> operator).
- Retrieved FAQ items are passed into `IPromptBuilder` to build a prompt.
- The prompt is sent to the chat completion client and the text response is returned and recorded as a `Conversation`.

2) Project layout and key components
- `Controllers/` - API controllers (`FaqController`, `FaqIngestionController`, `FileUploadController`).
- `Models/` - DTOs and option classes (`OpenAiOptions`, `FaqItem`, `FaqAskRequest`, `FaqAskResponse`, `Conversation`, `SqliteOptions`, `PostgresOptions`, `RagOptions`, OpenAI request/response models).
- `Services/` - Business logic: `FaqService`, `FaqIngestionService`, `ContextRetriever`, `OpenAiClient`, `OpenAiEmbeddingClient`, `PromptBuilder`.
- `IRepositories/` - repository interfaces for knowledge base and conversation/faq stores.
- `Repositories/` - implementations: in-memory, SQLite and Postgres ingestion/query repositories.
- `Infrastructure/RetryPolicyProvider.cs` - provides a Polly exponential-backoff retry policy for outbound HTTP calls.

3) Configuration (`appsettings.json`)
- `OpenAI` section: set `ApiKey`, `Endpoint`, `EmbeddingsEndpoint`, `Model`, `EmbeddingModel`.
- `Rag` section: `Provider` = `Sqlite` or `Postgres`, `MaxResults` = number of context items to retrieve.
- `Sqlite:ConnectionString` default `Data Source=faq.db`.
- `Postgres:ConnectionString` example `Host=localhost;Port=5432;Database=faqdb;Username=postgres;Password=postgres`.

4) Databases
- SQLite: embeddings are stored as JSON in `faq_embeddings(question TEXT PRIMARY KEY, answer TEXT, embedding TEXT)`.
  - Table is created automatically by the SQLite ingestion repository.
  - Cosine similarity is computed in memory when querying.
- Postgres: expects `faq_embeddings` table with `question`, `answer`, and `embedding` (vector) columns and pgvector installed.
  - Query uses `ORDER BY embedding <-> @embedding::vector LIMIT @limit`.

5) Build and run
1. Restore packages: `dotnet restore ./POC-AIPoweredFAQAPI/POC.AIPoweredFAQAPI.csproj`
2. Build: `dotnet build ./POC-AIPoweredFAQAPI/POC.AIPoweredFAQAPI.csproj`
3. Run: `dotnet run --project ./POC-AIPoweredFAQAPI/POC.AIPoweredFAQAPI.csproj`
4. Open Swagger UI in development: `http://localhost:{port}/swagger`.

6) Endpoints
- POST `/api/faq/ask` - Body: `FaqAskRequest { Question }` -> returns `FaqAskResponse { Answer, Confidence, Timestamp }`.
- POST `/api/faq/ingest` - Body: `FaqIngestRequest { Items: [ { Question, Answer } ] }` -> ingests and upserts items into the knowledge base.
- POST `/api/FileUpload` - multipart file upload, accepts PDF only and saves it to `Uploads/`.

7) Notes and troubleshooting
- Ensure `OpenAI:ApiKey` is set. If empty, the client will still attempt calls but will likely fail with 401.
- For Postgres RAG, install `pgvector` extension and create `faq_embeddings` with `embedding` as `vector` or compatible type.
- Polly retry policy handles transient HTTP errors and 429 responses.
- The project uses a single project file `POC.AIPoweredFAQAPI.csproj` targeting .NET 10.


