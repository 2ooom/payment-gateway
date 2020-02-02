# PaymentGateway HTTP API
=====

## Asusmptions
 1. Storing card details is not allowed. For identifications purposes only 4last digits of thecard are stored. Together with other payment details (Id, Timestamp, Amount) this should be enogh to uniquely identify transactions.
 2. Merchants use single account per acquiering bank. If several banks are needed a new Merchant could be created.
