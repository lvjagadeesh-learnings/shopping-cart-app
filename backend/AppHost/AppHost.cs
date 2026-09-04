var builder = DistributedApplication.CreateBuilder(args);

// Single Postgres server for local dev, mirroring the single-Aurora-cluster,
// schema-per-service design used in AWS (see infra/cloudformation/database.yaml).
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();

var shoppingCartDb = postgres.AddDatabase("shoppingcart");

var authApi = builder.AddProject<Projects.AuthService_Api>("auth-service")
    .WithReference(shoppingCartDb)
    .WaitFor(shoppingCartDb);

var catalogApi = builder.AddProject<Projects.CatalogService_Api>("catalog-service")
    .WithReference(shoppingCartDb)
    .WaitFor(shoppingCartDb);

var cartApi = builder.AddProject<Projects.CartService_Api>("cart-service")
    .WithReference(shoppingCartDb)
    .WaitFor(shoppingCartDb)
    .WithReference(catalogApi);

var inventoryApi = builder.AddProject<Projects.InventoryService_Api>("inventory-service")
    .WithReference(shoppingCartDb)
    .WaitFor(shoppingCartDb)
    .WithReference(catalogApi);

var paymentApi = builder.AddProject<Projects.PaymentService_Api>("payment-service")
    .WithReference(shoppingCartDb)
    .WaitFor(shoppingCartDb);

var orderApi = builder.AddProject<Projects.OrderService_Api>("order-service")
    .WithReference(shoppingCartDb)
    .WaitFor(shoppingCartDb)
    .WithReference(cartApi)
    .WithReference(inventoryApi)
    .WithReference(paymentApi);

var notificationApi = builder.AddProject<Projects.NotificationService_Api>("notification-service")
    .WithReference(shoppingCartDb)
    .WaitFor(shoppingCartDb);

var reviewApi = builder.AddProject<Projects.ReviewService_Api>("review-service")
    .WithReference(shoppingCartDb)
    .WaitFor(shoppingCartDb);

var recommendationApi = builder.AddProject<Projects.RecommendationService_Api>("recommendation-service")
    .WithReference(shoppingCartDb)
    .WaitFor(shoppingCartDb);

// Frontend (React 19 + Vite) is wired in once scaffolded under /frontend:
// builder.AddNpmApp("frontend", "../../frontend", "dev")
//     .WithReference(authApi).WithReference(catalogApi).WithReference(cartApi).WithReference(orderApi)
//     .WithHttpEndpoint(env: "PORT")
//     .WithExternalHttpEndpoints();

builder.Build().Run();
