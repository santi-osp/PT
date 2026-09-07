# Database

PostgreSQL is modeled through EF Core and Npgsql. `InitialCreate` owns schema creation and deterministic demonstration seeds, so application startup does not reinsert data.

```mermaid
erDiagram
    COUNTRY ||--o{ DEPARTMENT : contains
    DEPARTMENT ||--o{ MUNICIPALITY : contains
    MUNICIPALITY ||--o{ NEIGHBORHOOD : contains
    TRANSPORT_ZONE ||--o{ NEIGHBORHOOD : groups
    NEIGHBORHOOD ||--o{ CUSTOMER_ADDRESS : resolves
    CENTER ||--o{ RESIDENTIAL_CUSTOMER : serves
    RESIDENTIAL_CUSTOMER ||--|| CUSTOMER_ADDRESS : has

    RESIDENTIAL_CUSTOMER {
        uuid Id PK
        string Code UK
        int DocumentType UK
        string DocumentNumber UK
        uuid CenterId FK
        bool IsBlocked
        timestamptz BlockedAt
    }
    CUSTOMER_ADDRESS {
        uuid Id PK
        uuid CustomerId FK
        uuid NeighborhoodId FK
    }
    CENTER { uuid Id PK string Code UK }
    NEIGHBORHOOD { uuid Id PK uuid MunicipalityId FK uuid TransportZoneId FK }
    MUNICIPALITY { uuid Id PK uuid DepartmentId FK }
    DEPARTMENT { uuid Id PK uuid CountryId FK }
    COUNTRY { uuid Id PK }
    TRANSPORT_ZONE { uuid Id PK string Code UK }
    MODERN_CHANNEL_CUSTOMER {
        uuid Id PK
        int DocumentType UK
        string DocumentNumber UK
    }
```

`(DocumentType, DocumentNumber)` is unique for residential and modern-channel customers. Retirement is logical: `IsBlocked`, `BlockedAt`, and `UpdatedAt` change; no row is deleted. Geography is accepted only through `NeighborhoodId`, whose foreign-key chain resolves municipality, department, country, and transport zone.
