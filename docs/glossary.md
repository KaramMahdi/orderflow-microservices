# Glossary (ubiquitous language)

**Status: first draft (Phase 0), aligned with the domain model in Phase 1 (Milestone 3).** Refined
after the event storming answers and during Phase 1.

Rules:

- Names in code match this glossary. When the language changes, the code changes.
- A word has one meaning inside a context. The same word in two contexts can mean two different
  things (a *Customer* in Customers is not a *User* in Identity).
- Terms marked *(to confirm)* are not settled yet.

## Shared kernel (Domain.Shared.Layer)

| Term | Meaning |
|------|---------|
| Entity | An object with a Guid identity and the six audit columns. |
| Aggregate root | The entity that guards the invariants of its aggregate. The only entry point for changes; the only one with a repository. |
| Value object | An immutable object compared by value, created through a validated factory method (Money, Email, Address). |
| Audit columns | CreatorId, CreationTime, LastModifierId, LastModificationTime, DeletedBy, DeletedAt. All nullable. |
| Soft delete | Marking a row deleted (DeletedBy, DeletedAt) instead of removing it. The only kind of delete. |
| Money | An amount (decimal) and a currency code. Never negative; at most two decimal places (an amount with more is rejected, never rounded). Currencies with three minor units are not supported (ADR-0003). |
| Currency | The currency code of an amount: three letters (ISO 4217), stored in upper case. Only the format is checked in the domain. An order has exactly one. |
| Address | A postal address: street, city, optional postal code and a two-letter country code (ISO 3166-1 alpha-2). A value object in the shared kernel, used by Orders and Customers. |
| Domain manager | A domain service (`...Manager`) for a rule that spans several aggregates or needs a repository, such as a unique SKU. It changes aggregates but never saves. |
| Specification | A reusable query rule built on an expression (`...Specification`). It filters a repository query and checks an object in memory. |
| Request source | Where a request came from: Unknown, Web, Mobile or Api, taken from the client_id claim. |
| CorrelationId | The id that follows one request through logs, messages and jobs. Returned in X-Correlation-ID. |
| ErrorCode | An HTTP-like number that classifies an error (400, 401, 403, 404, 409, 422, 429, 500, 503). |
| ErrorDto | What the client receives on error: a code and a message. |
| MessageKey | The module, name and optional list that identify a text in the Translations table. |
| Translation | One text in one language (ar or en) for one message key. |
| Integration event | A message sent between contexts (through RabbitMQ from Phase 3). Its contract lives in Domain.Shared.Layer. |
| Domain event | Something that happened inside an aggregate, raised by the aggregate and dispatched after a successful save. |

## Orders (core)

| Term | Meaning |
|------|---------|
| Order | A customer's request to buy products, in one currency. The aggregate root of this context. |
| Order item | One product line of an order: product, quantity, unit price. Lives only inside an Order. Given when the order is placed, never added or removed afterwards; one item per product. |
| Order item line | The input of one item when an order is placed (`OrderItemLine`): product, quantity, unit price. |
| Order number | The business number of an order. Unique. Different from the Guid Id. Produced by `IOrderNumberGenerator`; the format is decided with the persistence work. |
| Order status | The lifecycle state of an order, controlled by the Order (ADR-0001): `Placed` (1), `Confirmed` (2), `Paid` (3), `Cancelled` (4). Allowed: Placed to Confirmed or Cancelled; Confirmed to Paid or Cancelled. Paid and Cancelled are final. |
| Confirmed | The stock of every product of the order is reserved; the order waits for payment. |
| Place an order | The act of creating an order. Limited by the create-order rate limit. |
| Total | The sum of the order items, in the order's currency. |
| Address | The delivery address of an order (value object). |
| Product snapshot | Orders' own small read model of a product (name, price at order time), updated from Catalog events. Not the Catalog Product. |
| Source | The request source stored on the order. |
| Cancel | Ending an order before it is completed. Decided (ADR-0001): an expired reservation cancels the order; a failed payment does not. Cancelled is final: never reopened. *(who else may cancel: to confirm)* |

## Catalog (supporting)

| Term | Meaning |
|------|---------|
| Product | Something that can be sold. The aggregate root for names and prices. |
| SKU | The stock-keeping unit: the business code of a product. Unique among non-deleted products. Stored in upper case; letters, digits, '-' and '_' only. |
| Normalized name | The product name after the ADR-0014 normalization (`NormalizedName`). Prefix search runs on it. Set by the Product whenever the name changes. |
| Product price | The price of a product in one currency. One per currency. |
| Stock item | The stock of one product: on hand and reserved. The aggregate root for stock. |
| On hand | The quantity physically in stock. |
| Reserved | The counter of the quantity held by Reserved movements. Never above on hand. |
| Available | On hand minus Reserved. What can still be ordered. |
| Stock movement | A record of quantity held or taken for an order: `Reserved` (1), then `Completed` (2) or `Failed` (3). Lives inside its StockItem; one per order and product. |
| Release | Marking a Reserved movement Failed (`ReleaseReservation`). Releasing twice does nothing. |
| Reservation | A Reserved stock movement. Lives 15 to 20 minutes unless paid. |
| Completed (movement) | The order was paid: the stock really leaves (on hand and Reserved both go down). |
| Failed (movement) | The reservation was released (not paid in time). Reserved goes down. |
| Category | *(to confirm: the index plan mentions CategoryId on Products, but no Category is in the domain model yet)* |

## Customers (supporting)

| Term | Meaning |
|------|---------|
| Customer | A person who buys. Not the same as a User in Identity. |
| Email | A validated email address (value object), stored in lower case. Unique among customers. |
| Address | A customer's address (the shared Address value object). |
| Register | Becoming a customer. The Customer keeps the Identity user id (`UserId`); one customer per user (decided by Karam, 2026-11-27). |
| Cart | *(to confirm: Phase 2 mentions a cart, but its owner context is not decided)* |

## Payments (supporting)

| Term | Meaning |
|------|---------|
| Payment | An attempt to collect the total of one order. The aggregate root. |
| Payment provider | The external system that moves the money. Simulated in Phase 1. |
| Provider reference | The provider's id for a payment. Unique when present. |
| Provider adapter | The anti-corruption layer in Payments.Infrastructure that translates the provider's model. |
| Payment status | `Pending` (1), `Completed` (2), `Failed` (3, another attempt may follow), `Refunded` (4, final). |
| Attempt | One try to pay (`PaymentAttempt`), inside the Payment: `Succeeded` or `Failed`. |
| Payment completed | The money was collected. |
| Payment failed | The money was not collected. |
| Refund | Returning a collected payment (`PaymentRefunded`). Every late payment is refunded (ADR-0001). Refunding twice does nothing. Simulated in Phase 1. |
| Late payment | A payment that arrives after its reservation expired (the movement is Failed and the order is Cancelled). |

## Notifications (generic)

| Term | Meaning |
|------|---------|
| Notification | A message (email or SMS style) sent to a recipient after an event. The aggregate root. |
| Channel | How a notification is sent: `Email` (1) or `Sms` (2). |
| Recipient | Who receives the notification: the user (`RecipientId`) and where it goes (`RecipientAddress`, an email or a phone number). |
| Notification status | `Pending` (1), `Sent` (2), `Failed` (3). Sent and Failed are final. |
| Attempt | One try to send (`AttemptCount`). The maximum number and the retry delay come from Options. |
| Next attempt at | When a pending notification is tried again. |

## Identity (generic)

| Term | Meaning |
|------|---------|
| User | An account that can sign in. Built on ASP.NET Core Identity. |
| Role | A set of permissions: Admin, Manager, Customer (and Service for machine-to-machine calls, from Phase 4). |
| Admin | Full system control, including users and roles. |
| Manager | Operations staff: manage products, view all orders and payments. |
| Policy | A named authorization rule used on controllers. Names are constants. |
| Client | A client application (web, mobile, api). Identified by its client_id. |
| client_id | The claim in the token that names the client app. The source of the request source. |
| Access token | A short-lived JWT. |
| Refresh token | A longer-lived token used to get a new access token. Stored as a hash. |
| Login | Exchanging credentials for tokens (`POST /api/auth/login`). |

## Files (generic)

| Term | Meaning |
|------|---------|
| File record | The database record of a file: metadata only, never content. The aggregate root. |
| Storage name | The unique name on disk: the file record's Guid plus a validated extension. |
| Storage path | The relative folder of the file: module and date. |
| Original name | The name the user uploaded, kept for display and downloads only. Never used in a path. |
| Owner link | ModuleName, EntityName and EntityId: the business record the file belongs to. |
| Size and hash | `SizeBytes` and `Sha256` (64 lowercase hexadecimal characters), measured while streaming and saved when the file becomes Active. |
| Pending | The record exists; the content is being written and scanned. |
| Active | The content is saved and clean. The only status that is served. |
| Rejected | Infected or not allowed. Never served. |
| Failed | Stayed Pending too long; marked by the cleanup job. |
| Owner access check | Asking the owning context whether the user may see the record (IFileOwnerAccessChecker). |
