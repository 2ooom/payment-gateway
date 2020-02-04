# PaymentGateway HTTP API

## Asusmptions
 1. Storing card details is not allowed. For identifications purposes only 4last digits of thecard are stored. Together with other payment details (Id, Timestamp, Amount) this should be enogh to uniquely identify transactions.
 2. Merchants use single account per acquiering bank. If several banks are needed a new Merchant could be created.
 3. Acquiering bank API are non standardised, therefor require individual setup. Hence factory is used (`IBankRegistry`) to create specific implementation of `IAcquirer`. For testing purposes we're `DummyAcquirer` is implemented for all kinds of payment APIs.

 ## Swagger/OpenAPI
 To facilitte development of the clients. Standardized specification is automatically generated and is avaialble via `http://host/swagger/v1/swagger.json`.
 Fot manual testing purposes and for Swagger UI could be found at `http://host/swagger`.
