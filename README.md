# PaymentGateway HTTP API

## Asusmptions
 1. Storing card details is not allowed. For identifications purposes only 4 last digits of the card are stored. Together with other payment details (Id, Timestamp, Amount) this should be enogh to uniquely identify transactions.
 2. Merchants use single account per acquiering bank. If several banks are needed a new Merchant could be created.
 3. Acquiering bank API are non standardised, therefor require individual setup. Hence factory is used (`IBankRegistry`) to create specific implementation of `IAcquirer`. For testing purposes we're `DummyAcquirer` is implemented for all kinds of payment APIs.

## Authenticatio
In order to access Payment APIs merchant needs to be registered (`POST api/merchants`) and authenticated (`POST api/merchants/authenticate`).
Succesful authentication response returns [JWT](https://jwt.io/) bearer token, which should be set as header in all requests to Authorized endpoints (`api/payments`) like so:
```
   Authorization: Bearer <jwtToken>
```

## Swagger/OpenAPI
To facilitte development of the clients. [OpenAPI specification](https://github.com/OAI/OpenAPI-Specification) is automatically generated from controllers definition and is avaialble at `http://host/swagger/v1/swagger.json`. A copy of it is also stored in `$(OutputDir)/generated/swagger.json`.
`swagger.json` allows to generate clients in most popular languages (C#, Java, Go, C++, etc) using [swagger-codegen](https://github.com/swagger-api/swagger-codegen).
C# client is generated at build time along with the spec and could be found in `PaymentGateway.Client` project.

For manual testing purposes and for Swagger UI could be found at `http://host/swagger`.

## Testing
*Unit tests* using in-memory databse are located in `PaymentGateway.Tests`
*Integration test* that bootstrap service along with database and could be found in `PaymentGateway.ITests`