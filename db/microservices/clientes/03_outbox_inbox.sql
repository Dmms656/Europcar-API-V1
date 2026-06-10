CREATE TABLE IF NOT EXISTS clientes.outbox_messages (
    id              BIGSERIAL PRIMARY KEY,
    event_id        UUID NOT NULL UNIQUE,
    event_type      VARCHAR(120) NOT NULL,
    correlation_id  UUID NOT NULL,
    payload_json    JSONB NOT NULL,
    occurred_at     TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    published_at    TIMESTAMPTZ NULL
);

CREATE INDEX IF NOT EXISTS ix_outbox_unpublished
    ON clientes.outbox_messages (occurred_at)
    WHERE published_at IS NULL;

CREATE TABLE IF NOT EXISTS clientes.inbox_processed_events (
    event_id      UUID PRIMARY KEY,
    processed_at  TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);
